CREATE DATABASE mydb;

CREATE TABLE T1(ID INT Primary Key);
CREATE TABLE T2 (ID INT Primary Key,Contents VARCHAR(50));

INSERT INTO T1 VALUES (1);
INSERT INTO T1 VALUES (2);
INSERT INTO T1 VALUES (3);
INSERT INTO T1 VALUES (4);
INSERT INTO T1 VALUES (6);
INSERT INTO T1 VALUES (7);

INSERT INTO T2 VALUES (1,'TEST1');
INSERT INTO T2 VALUES (2,'TEST2');

CREATE TABLE T3 (
    ID INT NOT NULL PRIMARY KEY,
	val INT NOT NULL,
	col1 UUID NOT NULL,
	col2 UUID NOT NULL,
	col3 UUID NOT NULL,
	col4 UUID NOT NULL,
	col5 UUID NOT NULL,
	col6 UUID NOT NULL
);

INSERT INTO T3
SELECT i,
       RANDOM() * 1000000,
	   md5(random()::text || clock_timestamp()::text)::uuid,
	   md5(random()::text || clock_timestamp()::text)::uuid,
	   md5(random()::text || clock_timestamp()::text)::uuid,
	   md5(random()::text || clock_timestamp()::text)::uuid,
	   md5(random()::text || clock_timestamp()::text)::uuid,
	   md5(random()::text || clock_timestamp()::text)::uuid
FROM generate_series(1,200000) i;

SELECT val 
FROM T3 


