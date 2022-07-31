--logical PG
CREATE DATABASE mydb;
CREATE PUBLICATION mypublication FOR ALL TABLES;

--pglogical
CREATE EXTENSION pglogical;
SELECT pglogical.create_node(
    node_name := 'publisher1',
    dsn := 'host=publisher port=5432 dbname=mydb user=postgres password=guest'
);

-- Replica Set replcation of group
SELECT pglogical.replication_set_add_all_tables('default', ARRAY['public']);
--查看 replication table 資訊
SELECT * from pglogical.replication_set_table ;
SELECT * FROM pg_replication_slots;
SELECT * FROM pglogical.show_subscription_status();

--monitor tables.
/*
write_lag：
flush_lag：
replay_lag：
*/
SELECT * FROM pg_stat_replication;