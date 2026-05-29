\timing on
\set ROWS 100000

-- ============================================================
-- SETUP — extension / FDW / server
-- ============================================================

CREATE EXTENSION IF NOT EXISTS redis_fdw_rs;

CREATE FOREIGN DATA WRAPPER redis_wrapper
    HANDLER redis_fdw_handler
    VALIDATOR redis_fdw_validator;

CREATE SERVER redis_server
    FOREIGN DATA WRAPPER redis_wrapper
    OPTIONS (host_port '127.0.0.1:8899');

SELECT srvname, srvoptions FROM pg_foreign_server WHERE srvname = 'redis_server';

-- ============================================================
-- PART B — MULTI-KEY PATTERN (glob scan)
-- ============================================================
-- When table_key_prefix contains *, ?, or [], the FDW scans
-- many Redis keys as rows. First column = key name.
-- ============================================================

-- Seed three session keys (one helper table per write — same key namespace)
CREATE FOREIGN TABLE demo_session_seed (value text)
    SERVER redis_server
    OPTIONS (database '0', table_type 'string', table_key_prefix 'session:user101');
-- redis-cli> SET session:user101 '{"logged_in": true, "ip": "10.0.0.1"}'
INSERT INTO demo_session_seed VALUES ('{"logged_in": true, "ip": "10.0.0.1"}');
DROP FOREIGN TABLE demo_session_seed;

CREATE FOREIGN TABLE demo_session_seed (value text)
    SERVER redis_server
    OPTIONS (database '0', table_type 'string', table_key_prefix 'session:user102');
-- redis-cli> SET session:user102 '{"logged_in": true, "ip": "10.0.0.2"}'
INSERT INTO demo_session_seed VALUES ('{"logged_in": true, "ip": "10.0.0.2"}');
DROP FOREIGN TABLE demo_session_seed;

CREATE FOREIGN TABLE demo_session_seed (value text)
    SERVER redis_server
    OPTIONS (database '0', table_type 'string', table_key_prefix 'session:user103');
-- redis-cli> SET session:user103 '{"logged_in": false, "ip": "10.0.0.3"}'
INSERT INTO demo_session_seed VALUES ('{"logged_in": false, "ip": "10.0.0.3"}');
DROP FOREIGN TABLE demo_session_seed;

-- Pattern table — note the * in the prefix
CREATE FOREIGN TABLE demo_sessions (key text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'session:*'
    );

-- redis-cli> SCAN 0 MATCH session:* COUNT 100   →   GET each key
SELECT * FROM demo_sessions;

-- PG-side LIKE filter on the key column
SELECT key, value FROM demo_sessions WHERE key LIKE '%user101%';

-- redis-cli> SET session:user104 '{"logged_in": true, "ip": "10.0.0.4"}'
INSERT INTO demo_sessions VALUES ('session:user104', '{"logged_in": true, "ip": "10.0.0.4"}');

SELECT * FROM demo_sessions;

-- redis-cli> DEL session:user104
DELETE FROM demo_sessions WHERE key = 'session:user104';
SELECT * FROM demo_sessions;


-- ============================================================
-- PART C — TTL SUPPORT
-- ============================================================
-- Two ways to set expiry:
--   1. Table-level default via the `ttl` option
--   2. Per-row override via a `ttl` column
-- ============================================================

-- C1. Table-level default TTL of 300s
CREATE FOREIGN TABLE demo_cache (value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'cache:page:home',
        ttl '300'
    );

-- redis-cli> SET cache:page:home "<html>cached homepage</html>" EX 300
INSERT INTO demo_cache VALUES ('<html>cached homepage</html>');
SELECT * FROM demo_cache;

-- Verify on the Redis side:
-- redis-cli -p 8899 TTL cache:page:home


-- C2. Per-row TTL via a ttl column (60s)
CREATE FOREIGN TABLE demo_cache_ttl (value text, ttl bigint)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'cache:api:response'
    );

-- redis-cli> SET cache:api:response '{"data":"fresh"}' EX 60
INSERT INTO demo_cache_ttl VALUES ('{"data": "fresh"}', 60);
-- ttl column = remaining seconds
-- redis-cli> GET cache:api:response   /   TTL cache:api:response
SELECT * FROM demo_cache_ttl;


-- C3. Persist forever with ttl = -1
CREATE FOREIGN TABLE demo_cache_persist (value text, ttl bigint)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'cache:api:permanent'
    );

-- redis-cli> SET cache:api:permanent '{"data":"permanent"}'   (no EX)
INSERT INTO demo_cache_persist VALUES ('{"data": "permanent"}', -1);
SELECT * FROM demo_cache_persist;


-- ============================================================
-- PART D — WHERE-CLAUSE PUSHDOWN OPTIMIZATION
-- ============================================================
-- The FDW translates WHERE into the cheapest Redis command:
--   Hash:  field = 'x'      → HGET
--   Hash:  field IN (...)   → HMGET
--   Set:   member = 'x'     → SISMEMBER
--   ZSet:  score >= N       → ZRANGEBYSCORE
--   List:  index = N        → LINDEX
-- ============================================================

CREATE FOREIGN TABLE demo_product (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'product:sku:12345'
    );

-- redis-cli> HSET product:sku:12345 name "Mechanical Keyboard" price 129.99 brand KeyCraft stock 42 category electronics weight_kg 0.85
INSERT INTO demo_product VALUES ('name',      'Mechanical Keyboard');
INSERT INTO demo_product VALUES ('price',     '129.99');
INSERT INTO demo_product VALUES ('brand',     'KeyCraft');
INSERT INTO demo_product VALUES ('stock',     '42');
INSERT INTO demo_product VALUES ('category',  'electronics');
INSERT INTO demo_product VALUES ('weight_kg', '0.85');

-- redis-cli> HGETALL product:sku:12345         (no pushdown — full fetch)
EXPLAIN (ANALYZE, VERBOSE) SELECT * FROM demo_product;

-- redis-cli> HGET product:sku:12345 price       (single-field pushdown)
EXPLAIN (ANALYZE, VERBOSE) SELECT value FROM demo_product WHERE field = 'price';
SELECT value FROM demo_product WHERE field = 'price';

-- redis-cli> HMGET product:sku:12345 name price stock   (IN-list pushdown)
EXPLAIN (ANALYZE, VERBOSE) SELECT * FROM demo_product WHERE field IN ('name', 'price', 'stock');
SELECT * FROM demo_product WHERE field IN ('name', 'price', 'stock');


-- ZSet score-range pushdown
CREATE FOREIGN TABLE demo_scores (member text, score numeric)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'zset',
        table_key_prefix 'demo:scores:range'
    );

-- redis-cli> ZADD demo:scores:range 10 a 20 b 30 c 40 d 50 e
INSERT INTO demo_scores VALUES ('a', 10);
INSERT INTO demo_scores VALUES ('b', 20);
INSERT INTO demo_scores VALUES ('c', 30);
INSERT INTO demo_scores VALUES ('d', 40);
INSERT INTO demo_scores VALUES ('e', 50);

-- redis-cli> ZRANGEBYSCORE demo:scores:range 25 45 WITHSCORES     (range pushdown)
EXPLAIN (ANALYZE, VERBOSE) SELECT * FROM demo_scores WHERE score >= 25 AND score <= 45;
SELECT * FROM demo_scores WHERE score >= 25 AND score <= 45;


-- ============================================================
-- PART E — UPDATE OPERATIONS
-- ============================================================
-- UPDATE maps to the right Redis write:
--   String → SET   (full value replace)
--   Hash   → HSET  (per-field)
--   ZSet   → ZADD  (score change)
-- ============================================================

-- E1. String UPDATE → SET
CREATE FOREIGN TABLE demo_upd_string (value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'upd:config:version'
    );

-- redis-cli> SET upd:config:version "1.0.0"
INSERT INTO demo_upd_string VALUES ('1.0.0');
SELECT * FROM demo_upd_string;

-- redis-cli> SET upd:config:version "2.0.0"
EXPLAIN (ANALYZE, VERBOSE) UPDATE demo_upd_string SET value = '2.0.0';
UPDATE demo_upd_string SET value = '2.0.0';
SELECT * FROM demo_upd_string;


-- E2. Hash UPDATE → HSET (single field)
CREATE FOREIGN TABLE demo_upd_hash (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'upd:user:profile'
    );

-- redis-cli> HSET upd:user:profile name Alice role engineer city Taipei
INSERT INTO demo_upd_hash VALUES
    ('name', 'Alice'),
    ('role', 'engineer'),
    ('city', 'Taipei');

SELECT * FROM demo_upd_hash;

-- redis-cli> HSET upd:user:profile role "staff engineer"     (pushdown — no HGETALL+rewrite)
EXPLAIN (ANALYZE, VERBOSE) UPDATE demo_upd_hash SET value = 'staff engineer' WHERE field = 'role';
UPDATE demo_upd_hash SET value = 'staff engineer' WHERE field = 'role';
SELECT * FROM demo_upd_hash;

-- redis-cli> HSET upd:user:profile name redacted    /    HSET upd:user:profile city redacted
UPDATE demo_upd_hash SET value = 'redacted' WHERE field IN ('name', 'city');
SELECT * FROM demo_upd_hash;


-- E3. ZSet UPDATE → ZADD (score change)
CREATE FOREIGN TABLE demo_upd_zset (member text, score numeric)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'zset',
        table_key_prefix 'upd:game:leaderboard'
    );

-- redis-cli> ZADD upd:game:leaderboard 1000 alice 1500 bob 800 charlie
INSERT INTO demo_upd_zset VALUES
    ('alice',   1000),
    ('bob',     1500),
    ('charlie',  800);

SELECT * FROM demo_upd_zset ORDER BY score DESC;

-- redis-cli> ZADD upd:game:leaderboard 1500 alice
EXPLAIN (ANALYZE, VERBOSE) UPDATE demo_upd_zset SET score = 1500 WHERE member = 'alice';
UPDATE demo_upd_zset SET score = 1500 WHERE member = 'alice';
SELECT * FROM demo_upd_zset ORDER BY score DESC;


-- ============================================================
-- PART F — BULK INSERT (100K) + PUSHDOWN-DRIVEN LARGE JOIN
-- ============================================================
-- The headliner perf demo:
--   F1. Bulk load 100K rows — batch_size A/B test.
--   F2. PG table × Redis FDW JOIN — default Hash Join (full
--       scan) vs. forced Nested Loop (per-row HGET pushdown).
--   F3. Redis FDW × Redis FDW JOIN — both sides pushed.
--   F4. ZSet score-range pushdown at scale.
-- ============================================================

-- F1A. Baseline: small batch_size → many round-trips
CREATE FOREIGN TABLE bulk_user_profiles_slow (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'demo11:users:slow',
        batch_size '500'
    );

-- redis-cli> HSET demo11:users:slow user:1 '{"name":"u_1",...}' ... (batched, 500/req)
INSERT INTO bulk_user_profiles_slow
SELECT
    'user:' || g::text,
    '{"name":"u_' || g::text || '","score":' || (random() * 1000)::int::text || '}'
FROM generate_series(1, :ROWS) g;

SELECT COUNT(*) AS loaded_slow FROM bulk_user_profiles_slow;


-- F1B. Tuned: large batch_size → fewer round-trips
CREATE FOREIGN TABLE bulk_user_profiles (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'demo11:users:profiles',
        batch_size '10000'
    );

-- redis-cli> HSET demo11:users:profiles user:1 '{...}' ... (batched, 10000/req)
INSERT INTO bulk_user_profiles
SELECT
    'user:' || g::text,
    '{"name":"u_' || g::text || '","score":' || (random() * 1000)::int::text || '}'
FROM generate_series(1, :ROWS) g;

SELECT COUNT(*) AS loaded_fast FROM bulk_user_profiles;


-- Companion SET — 50K "active" users
CREATE FOREIGN TABLE bulk_active_users (member text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'set',
        table_key_prefix 'demo11:users:active',
        batch_size '10000'
    );

-- redis-cli> SADD demo11:users:active user:1 user:2 ... user:50000 (batched)
INSERT INTO bulk_active_users
SELECT 'user:' || g::text
FROM generate_series(1, 50000) g;

--EXPLAIN (ANALYZE, VERBOSE)
SELECT COUNT(*) AS active_users FROM bulk_active_users;


-- ------------------------------------------------------------
-- F2. PG table × Redis FDW JOIN
-- ------------------------------------------------------------
-- 200 VIP users we want to enrich from the 100K-row Hash.

CREATE TEMPORARY TABLE vip_users (user_id text PRIMARY KEY);
INSERT INTO vip_users
SELECT 'user:' || g::text FROM generate_series(1, 200) g;

-- F2A. Default plan — Hash Join → full HGETALL on the FDW side
-- redis-cli equivalent: HGETALL demo11:users:profiles   (all 100K back)
EXPLAIN (ANALYZE, VERBOSE)
SELECT v.user_id, p.value AS profile
FROM vip_users v
JOIN bulk_user_profiles p ON v.user_id = p.field;

-- F2B. Force Nested Loop → parameterized inner scan → HGET per row
-- redis-cli equivalent: HGET demo11:users:profiles user:<id>   × 200

EXPLAIN (ANALYZE, VERBOSE)
SELECT v.user_id, p.value AS profile
FROM vip_users v
JOIN bulk_user_profiles p ON v.user_id = p.field;

-- Same shape with ORDER BY / LIMIT — still per-row HGET
EXPLAIN (ANALYZE, VERBOSE)
SELECT v.user_id, p.value AS profile
FROM vip_users v
JOIN bulk_user_profiles p ON v.user_id = p.field
ORDER BY v.user_id
LIMIT 10;


-- ------------------------------------------------------------
-- F3. Redis FDW × Redis FDW JOIN
-- ------------------------------------------------------------

-- F3A. SET pushdown via IN list — one SISMEMBER per item
-- redis-cli equivalent: SISMEMBER demo11:users:active user:1 ... × 10
EXPLAIN (ANALYZE, VERBOSE)
SELECT a.member
FROM bulk_active_users a
WHERE a.member IN (
    'user:1','user:42','user:100','user:777','user:9999',
    'user:12345','user:55555','user:88888','user:99999','user:99000'
);

-- F3B. Two-sided pushdown — SET filter feeds HASH HGETs
-- redis-cli equivalent: SISMEMBER ... × 10  →  HGET demo11:users:profiles user:<id> × 10

EXPLAIN (ANALYZE, VERBOSE)
SELECT a.member AS user_id, p.value AS profile
FROM bulk_active_users a
JOIN bulk_user_profiles p ON p.field = a.member
WHERE a.member IN (
    'user:1','user:42','user:100','user:777','user:9999',
    'user:12345','user:55555','user:88888','user:99999','user:99000'
);

-- F3C. Contrast — no selective filter → full scans on both sides
-- redis-cli equivalent: HGETALL ... + SMEMBERS ...  → hash join in PG
EXPLAIN (ANALYZE, VERBOSE)
SELECT COUNT(*) AS active_with_profile
FROM bulk_user_profiles p
JOIN bulk_active_users a ON p.field = a.member;


-- ------------------------------------------------------------
-- F4. ZSet score-range pushdown at scale
-- ------------------------------------------------------------
CREATE FOREIGN TABLE bulk_leaderboard (member text, score numeric)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'zset',
        table_key_prefix 'demo11:leaderboard',
        batch_size '10000'
    );

-- redis-cli> ZADD demo11:leaderboard <score> player:<id> ...   (100K members)
INSERT INTO bulk_leaderboard
SELECT 'player:' || g::text, (random() * 100000)::int
FROM generate_series(1, :ROWS) g;

-- Score-range pushdown — fetches only the top ~1%
-- redis-cli equivalent: ZRANGEBYSCORE demo11:leaderboard 99000 +inf WITHSCORES
EXPLAIN (ANALYZE, VERBOSE)
SELECT * FROM bulk_leaderboard
WHERE score >= 99000
ORDER BY score DESC;

-- ------------------------------------------------------------
-- Target table: Redis HASH backed by a single key
-- batch_size = 10000 → ~10 pipelined Redis round-trips for 100K rows
-- ------------------------------------------------------------
CREATE FOREIGN TABLE copy_users_100k (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'copy:users:100k',
        batch_size '10000'
    );

-- ------------------------------------------------------------
-- Bulk load via \copy (client-side; no superuser required)
-- Run psql from the examples/ directory, or adjust the path.
-- ------------------------------------------------------------
\copy copy_users_100k (field, value) FROM 'users_100k.csv' WITH (FORMAT csv);

-- ------------------------------------------------------------
-- Verify
-- ------------------------------------------------------------
SELECT COUNT(*) AS rows_loaded FROM copy_users_100k;

-- Spot-check a few entries (uses HGET pushdown — single round-trip each)
SELECT * FROM copy_users_100k WHERE field = 'user:1';
SELECT * FROM copy_users_100k WHERE field = 'user:50000';
SELECT * FROM copy_users_100k WHERE field = 'user:100000';

-- IN-list lookup uses HMGET pushdown — one round-trip for N fields
EXPLAIN (ANALYZE, VERBOSE)
SELECT * FROM copy_users_100k
WHERE field IN ('user:1', 'user:99', 'user:9999', 'user:50000', 'user:99999');

-- ============================================================
-- PART A — BASIC TYPES
-- ============================================================

-- ------------------------------------------------------------
-- A1. STRING — one key, one value
-- ------------------------------------------------------------
CREATE FOREIGN TABLE demo_config (value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'string',
        table_key_prefix 'app:config:version'
    );

-- redis-cli> SET app:config:version "2.1.0"
INSERT INTO demo_config VALUES ('2.1.0');

-- redis-cli> GET app:config:version
SELECT * FROM demo_config;

-- redis-cli> SET app:config:version "2.2.0"
UPDATE demo_config SET value = '2.2.0';
SELECT * FROM demo_config;

-- redis-cli> DEL app:config:version
DELETE FROM demo_config;
SELECT * FROM demo_config;   -- empty


-- ------------------------------------------------------------
-- A2. HASH — structured records
-- ------------------------------------------------------------
CREATE FOREIGN TABLE demo_user_profile (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'user:1001'
    );

-- redis-cli> HSET user:1001 name        "Alice Chen"
-- redis-cli> HSET user:1001 email       "alice@example.com"
-- redis-cli> HSET user:1001 role        "engineer"
-- redis-cli> HSET user:1001 team        "platform"
INSERT INTO demo_user_profile VALUES ('name',  'Alice Chen');
INSERT INTO demo_user_profile VALUES ('email', 'alice@example.com');
INSERT INTO demo_user_profile VALUES ('role',  'engineer');
INSERT INTO demo_user_profile VALUES ('team',  'platform');

-- redis-cli> HGETALL user:1001
SELECT * FROM demo_user_profile;

-- redis-cli> HGET user:1001 email        (single-field pushdown)
EXPLAIN (ANALYZE, VERBOSE) SELECT value FROM demo_user_profile WHERE field = 'email';
SELECT value FROM demo_user_profile WHERE field = 'email';

-- redis-cli> HSET user:1001 role "senior engineer"
UPDATE demo_user_profile SET value = 'senior engineer' WHERE field = 'role';
SELECT * FROM demo_user_profile;

-- redis-cli> HDEL user:1001 team
DELETE FROM demo_user_profile WHERE field = 'team';
SELECT * FROM demo_user_profile;


-- ------------------------------------------------------------
-- A3. LIST — ordered, index-addressable
-- ------------------------------------------------------------
CREATE FOREIGN TABLE demo_task_queue (index int, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'list',
        table_key_prefix 'queue:tasks'
    );

-- redis-cli> RPUSH queue:tasks "deploy-service-a"
-- redis-cli> RPUSH queue:tasks "run-migrations"
-- redis-cli> RPUSH queue:tasks "notify-slack"
-- redis-cli> RPUSH queue:tasks "update-dashboard"
INSERT INTO demo_task_queue (value) VALUES ('deploy-service-a');
INSERT INTO demo_task_queue (value) VALUES ('run-migrations');
INSERT INTO demo_task_queue (value) VALUES ('notify-slack');
INSERT INTO demo_task_queue (value) VALUES ('update-dashboard');

-- redis-cli> LRANGE queue:tasks 0 -1
SELECT * FROM demo_task_queue;

-- redis-cli> LRANGE queue:tasks 0 1     /    LRANGE queue:tasks 2 3
SELECT * FROM demo_task_queue LIMIT 2;
SELECT * FROM demo_task_queue LIMIT 2 OFFSET 2;

-- redis-cli> LREM queue:tasks 0 "run-migrations"
DELETE FROM demo_task_queue WHERE value = 'run-migrations';
SELECT * FROM demo_task_queue;


-- ------------------------------------------------------------
-- A4. SET — unique membership
-- ------------------------------------------------------------
CREATE FOREIGN TABLE demo_user_tags (member text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'set',
        table_key_prefix 'user:1001:tags'
    );

-- redis-cli> SADD user:1001:tags rust postgresql redis linux
INSERT INTO demo_user_tags VALUES ('rust');
INSERT INTO demo_user_tags VALUES ('postgresql');
INSERT INTO demo_user_tags VALUES ('redis');
INSERT INTO demo_user_tags VALUES ('linux');
INSERT INTO demo_user_tags VALUES ('rust');   -- duplicate ignored by Redis

-- redis-cli> SMEMBERS user:1001:tags
SELECT * FROM demo_user_tags;

-- redis-cli> SISMEMBER user:1001:tags rust       (pushdown)
EXPLAIN (ANALYZE, VERBOSE) SELECT * FROM demo_user_tags WHERE member = 'rust';
SELECT * FROM demo_user_tags WHERE member = 'rust';

-- redis-cli> SREM user:1001:tags linux
DELETE FROM demo_user_tags WHERE member = 'linux';
SELECT * FROM demo_user_tags;



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


-- ------------------------------------------------------------
-- A6. STREAM — append-only event log
-- ------------------------------------------------------------
-- Streams are INSERT-only (no UPDATE).
CREATE FOREIGN TABLE demo_audit_log (id text, user_id text, action text, resource text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'stream',
        table_key_prefix 'audit:events'
    );

-- redis-cli> XADD audit:events * user_id user:alice   action CREATE resource project:alpha
-- redis-cli> XADD audit:events * user_id user:bob     action UPDATE resource project:alpha
-- redis-cli> XADD audit:events * user_id user:alice   action DELETE resource file:readme.md
-- redis-cli> XADD audit:events * user_id user:charlie action CREATE resource project:beta
INSERT INTO demo_audit_log VALUES ('*', 'user:alice',   'CREATE', 'project:alpha');
INSERT INTO demo_audit_log VALUES ('*', 'user:bob',     'UPDATE', 'project:alpha');
INSERT INTO demo_audit_log VALUES ('*', 'user:alice',   'DELETE', 'file:readme.md');
INSERT INTO demo_audit_log VALUES ('*', 'user:charlie', 'CREATE', 'project:beta');

-- redis-cli> XRANGE audit:events - +
SELECT * FROM demo_audit_log;

-- PG-side filter on stream fields
SELECT * FROM demo_audit_log WHERE user_id = 'user:alice';
SELECT * FROM demo_audit_log WHERE action  = 'CREATE';


-- Truncate the bulk tables first so Redis state is clean
TRUNCATE bulk_user_profiles_slow;
TRUNCATE bulk_user_profiles;
TRUNCATE bulk_active_users;
TRUNCATE bulk_leaderboard;

-- Part A
DROP FOREIGN TABLE IF EXISTS demo_config;
DROP FOREIGN TABLE IF EXISTS demo_user_profile;
DROP FOREIGN TABLE IF EXISTS demo_task_queue;
DROP FOREIGN TABLE IF EXISTS demo_user_tags;
DROP FOREIGN TABLE IF EXISTS demo_leaderboard;
DROP FOREIGN TABLE IF EXISTS demo_audit_log;

-- Part B
DROP FOREIGN TABLE IF EXISTS demo_sessions;

-- Part C
DROP FOREIGN TABLE IF EXISTS demo_cache;
DROP FOREIGN TABLE IF EXISTS demo_cache_ttl;
DROP FOREIGN TABLE IF EXISTS demo_cache_persist;

-- Part D
DROP FOREIGN TABLE IF EXISTS demo_product;
DROP FOREIGN TABLE IF EXISTS demo_scores;

-- Part E
DROP FOREIGN TABLE IF EXISTS demo_upd_string;
DROP FOREIGN TABLE IF EXISTS demo_upd_hash;
DROP FOREIGN TABLE IF EXISTS demo_upd_zset;

-- Part F
DROP FOREIGN TABLE IF EXISTS bulk_user_profiles_slow;
DROP FOREIGN TABLE IF EXISTS bulk_user_profiles;
DROP FOREIGN TABLE IF EXISTS bulk_active_users;
DROP FOREIGN TABLE IF EXISTS bulk_leaderboard;
DROP TABLE IF EXISTS vip_users;

-- Server + FDW + extension (uncomment if you want a full teardown)
-- DROP SERVER IF EXISTS redis_server CASCADE;
-- DROP FOREIGN DATA WRAPPER IF EXISTS redis_wrapper CASCADE;
-- DROP EXTENSION IF EXISTS redis_fdw_rs;
