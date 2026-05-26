-- ============================================================
-- Demo 12: COPY FROM file - 100K rows into Redis Hash
-- ============================================================
-- COPY FROM uses the BeginForeignInsert / EndForeignInsert path
-- which lets the FDW pipeline rows in batches.
--
-- Workflow:
--   1. (Outside psql, ONCE)  python generate_users_csv.py
--      → produces examples/users_100k.csv (~11 MiB, 100K rows)
--   2. (In psql)  \i 12_copy_from_file.sql
--      → uses \copy (client-side, no superuser needed) to stream
--        the CSV into a Redis hash foreign table.
--
-- \copy reads on the client side, so the CSV path is relative to
-- wherever you launched psql. Adjust the path below if needed.
-- ============================================================

\timing on

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

-- ------------------------------------------------------------
-- Bonus: COPY FROM stdin (inline data, no file needed)
-- ------------------------------------------------------------
CREATE FOREIGN TABLE copy_stdin_demo (field text, value text)
    SERVER redis_server
    OPTIONS (
        database '0',
        table_type 'hash',
        table_key_prefix 'copy:stdin:demo'
    );

COPY copy_stdin_demo (field, value) FROM stdin WITH (FORMAT csv);
sku:1001,"name=keyboard;price=129.99;stock=42"
sku:1002,"name=mouse;price=49.99;stock=120"
sku:1003,"name=monitor;price=399.99;stock=15"
\.

SELECT * FROM copy_stdin_demo;

-- ------------------------------------------------------------
-- Cleanup
-- ------------------------------------------------------------
TRUNCATE copy_users_100k;
TRUNCATE copy_stdin_demo;
DROP FOREIGN TABLE copy_users_100k;
DROP FOREIGN TABLE copy_stdin_demo;
