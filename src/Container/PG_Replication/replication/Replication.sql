CREATE DATABASE mydb;
--logical PG
CREATE SUBSCRIPTION mySubscription 
CONNECTION 'host=publisher dbname=mydb user=postgres password=guest'
PUBLICATION mypublication
WITH (copy_data='true',create_slot='true', enabled='true',synchronous_commit='off');

--需要 REFRESH PUBLICATION 才會對於新建立 Table 進行 Replication
ALTER SUBSCRIPTION mySubscription REFRESH PUBLICATION

--pglogical
CREATE EXTENSION pglogical;

SELECT pglogical.create_node(
    node_name := 'subscribepubsr1',
    dsn := 'host=replication port=5432 dbname=mydb user=postgres password=guest'
);

SELECT pglogical.create_subscription(
    subscription_name := 'subscription',
    provider_dsn := 'host=publisher port=5432 dbname=mydb user=postgres password=guest'
);

SELECT * 
FROM pglogical.alter_subscription_resynchronize_table('subscription', 'T3');

SELECT pglogical.wait_for_subscription_sync_complete('subscription');
SELECT pglogical.alter_subscription_synchronize('subscription', false);
select * from pglogical.show_subscription_table('subscription','T3');