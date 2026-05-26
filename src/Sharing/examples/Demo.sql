\set ROWS 100000

-- ============================================================
-- Demo 1: String Type - Simple Key-Value Storage
-- ============================================================
-- Redis String is the most basic type: one key → one value
-- Mapped to a single-column foreign table
-- ============================================================

-- Create a foreign table backed by a Redis STRING key
CREATE FOREIGN TABLE demo_config (value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'app:config:version'
    );

-- INSERT sets the Redis key
INSERT INTO demo_config VALUES ('2.1.0');

-- SELECT reads the key
SELECT * FROM demo_config;

-- UPDATE modifies the value
UPDATE demo_config SET value = '2.2.0';
SELECT * FROM demo_config;

-- DELETE removes the key from Redis
DELETE FROM demo_config;
SELECT * FROM demo_config;  -- returns empty

-- Cleanup
DROP FOREIGN TABLE demo_config;


-- ============================================================
-- Demo 7: Multi-Key Pattern Mode
-- ============================================================
-- When table_key_prefix contains a glob pattern (*, ?, []),
-- the FDW scans multiple Redis keys as rows.
-- First column = Redis key name, remaining columns = value data
-- ============================================================

-- First, seed some Redis keys using a helper table
CREATE FOREIGN TABLE demo_session_seed (value text)
    SERVER redis_server
    OPTIONS (database '0', table_type 'string', table_key_prefix 'session:user101');
INSERT INTO demo_session_seed VALUES ('{"logged_in": true, "ip": "10.0.0.1"}');
DROP FOREIGN TABLE demo_session_seed;

CREATE FOREIGN TABLE demo_session_seed2 (value text)
    SERVER redis_server
    OPTIONS (database '0', table_type 'string', table_key_prefix 'session:user102');
INSERT INTO demo_session_seed2 VALUES ('{"logged_in": true, "ip": "10.0.0.2"}');
DROP FOREIGN TABLE demo_session_seed2;

CREATE FOREIGN TABLE demo_session_seed3 (value text)
    SERVER redis_server
    OPTIONS (database '0', table_type 'string', table_key_prefix 'session:user103');
INSERT INTO demo_session_seed3 VALUES ('{"logged_in": false, "ip": "10.0.0.3"}');
DROP FOREIGN TABLE demo_session_seed3;

-- Now create a multi-key pattern table (note the * in the prefix)
CREATE FOREIGN TABLE demo_sessions (key text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'session:*'
    );

-- SELECT scans all keys matching the pattern
SELECT * FROM demo_sessions;

-- Use PostgreSQL filtering on the results
SELECT key, value FROM demo_sessions WHERE key LIKE '%user101%';

-- INSERT into a specific key (first column = key name)
INSERT INTO demo_sessions VALUES ('session:user104', '{"logged_in": true, "ip": "10.0.0.4"}');
SELECT * FROM demo_sessions;

-- DELETE a specific key
DELETE FROM demo_sessions WHERE key = 'session:user104';
SELECT * FROM demo_sessions;

-- Cleanup
DROP FOREIGN TABLE demo_sessions;

-- ============================================================
-- Demo 8: TTL (Time-To-Live) Support
-- ============================================================
-- Redis keys can have automatic expiration.
-- The FDW supports TTL via:
--   1. Table-level default (ttl option)
--   2. Per-row override (ttl column)
-- ============================================================

-- Table with a default TTL of 300 seconds (5 minutes)
CREATE FOREIGN TABLE demo_cache (value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'cache:page:home',
        ttl '300'
    );

INSERT INTO demo_cache VALUES ('<html>cached homepage</html>');
SELECT * FROM demo_cache;

-- Check TTL from Redis side (use psql's \! or redis-cli):
-- redis-cli -p 8899 TTL cache:page:home
-- Should show ~300

DROP FOREIGN TABLE demo_cache;

-- ============================================================
-- Per-row TTL override using a ttl column
-- ============================================================

CREATE FOREIGN TABLE demo_cache_ttl (value text, ttl bigint)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'cache:api:response'
    );

-- Insert with custom TTL (60 seconds)
INSERT INTO demo_cache_ttl VALUES ('{"data": "fresh"}', 60);
SELECT * FROM demo_cache_ttl;
-- The ttl column shows remaining seconds

-- Insert with no expiry (ttl = -1 means persist forever)
-- First change key for a new entry
DROP FOREIGN TABLE demo_cache_ttl;
CREATE FOREIGN TABLE demo_cache_persist (value text, ttl bigint)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'cache:api:permanent'
    );
INSERT INTO demo_cache_persist VALUES ('{"data": "permanent"}', -1);
SELECT * FROM demo_cache_persist;

-- Cleanup
DROP FOREIGN TABLE demo_cache_persist;


CREATE FOREIGN TABLE bulk_leaderboard (member text, score numeric)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'zset',
        table_key_prefix 'demo11:leaderboard',
        batch_size '10000'
    );

INSERT INTO bulk_leaderboard
SELECT 'player:' || g::text, (random() * 100000)::int
FROM generate_series(1, :ROWS) g;

-- Full scan baseline
EXPLAIN (ANALYZE, VERBOSE)
SELECT COUNT(*) FROM bulk_leaderboard;

-- Pushed score range — ZRANGEBYSCORE, fetches only top ~1%
EXPLAIN (ANALYZE, VERBOSE)
SELECT * FROM bulk_leaderboard
WHERE score >= 99000
ORDER BY score DESC;




-- ============================================================
-- PART 2: Pushdown JOIN — local PG table × Redis FDW
-- ============================================================
-- Strategy: small local driver table (200 rows). When the planner
-- picks a Nested Loop, the FDW gets `field = $1` per outer row
-- and turns each into a single HGET — instead of one HGETALL
-- fetching all 100K rows.

-- A small "VIP" list — 200 specific users we want to enrich
CREATE TEMPORARY TABLE vip_users (user_id text PRIMARY KEY);
INSERT INTO vip_users
SELECT 'user:' || g::text
FROM generate_series(1, 200) g;

-- ---- 2A. Default plan (planner picks Hash Join → full FDW scan) ----
-- Foreign Scan returns all 100K rows via HGETALL.
EXPLAIN (ANALYZE, VERBOSE)
SELECT v.user_id, p.value AS profile
FROM vip_users v
JOIN bulk_user_profiles p ON v.user_id = p.field;
