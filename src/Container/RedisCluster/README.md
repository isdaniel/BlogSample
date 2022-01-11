# Redis Cluster POC

在巨量資料下，資料庫合理的分片(DB sharding)可以幫助我們承受更高量的資料量(前提Index還有設定都調教到很好在考慮 sharding)

而在 Redis 世界裡也有 redis cluster 來做 sharding 事情，今天就來跟大家分享介紹

我使用是 [bitnami redis-cluster](https://hub.docker.com/r/bitnami/redis-cluster/) 這個 Image 來做這次Poc

此案例是 3 master - 3 slave Redis server 範例

## 建立 Redis Cluster

### How to Use

```
docker-compose up -d
```

跑完之後就會出現 6 台 Redis Container，如下表

```
$ docker ps
CONTAINER ID   IMAGE                       COMMAND                  CREATED          STATUS          PORTS                                       NAMES
1a0c740cbb96   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   31 seconds ago   Up 27 seconds   0.0.0.0:8105->6379/tcp, :::8105->6379/tcp   rediscluster_redis-node-5_1
1651a81a286f   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   37 seconds ago   Up 30 seconds   0.0.0.0:8102->6379/tcp, :::8102->6379/tcp   rediscluster_redis-node-2_1
5d93edfc55e6   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   37 seconds ago   Up 31 seconds   0.0.0.0:8104->6379/tcp, :::8104->6379/tcp   rediscluster_redis-node-4_1
8ab5bbbb7364   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   37 seconds ago   Up 29 seconds   0.0.0.0:8103->6379/tcp, :::8103->6379/tcp   rediscluster_redis-node-3_1
8edf90bed3fb   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   37 seconds ago   Up 29 seconds   0.0.0.0:8101->6379/tcp, :::8101->6379/tcp   rediscluster_redis-node-1_1
e11ac0ec56aa   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   37 seconds ago   Up 30 seconds   0.0.0.0:8100->6379/tcp, :::8100->6379/tcp   rediscluster_redis-node-0_1
```

### 查看 Redis Cluster 架構

我們進入 `rediscluster_redis-node-0_1` 這台Container 利用 redis-cli 輸入 `CLUSTER INFO` 可以查看目前 Cluster 基本狀態

```text
$ docker exec -it rediscluster_redis-node-0_1 bash
I have no name!@e11ac0ec56aa:/$ redis-cli -a redisCluster

127.0.0.1:6379> CLUSTER INFO
cluster_state:ok
cluster_slots_assigned:16384
cluster_slots_ok:16384
cluster_slots_pfail:0
cluster_slots_fail:0
cluster_known_nodes:6
cluster_size:3
cluster_current_epoch:6
cluster_my_epoch:1
cluster_stats_messages_ping_sent:1723
cluster_stats_messages_pong_sent:1833
cluster_stats_messages_sent:3556
cluster_stats_messages_ping_received:1828
cluster_stats_messages_pong_received:1723
cluster_stats_messages_meet_received:5
cluster_stats_messages_received:3556
```

> 本篇我 redis password 統一使用 `redisCluster` 所以進入 redis 操作之前記得輸入 `-a` + 密碼

進入可以看到幾個重要資訊

* cluster_state
* cluster_size:3
* cluster_current_epoch:6
* cluster_slots_assigned

代表目前 Cluster 啟動狀態OK，再來我們利用 `--cluster check` 了解該 Redis Server Cluster 中 master-slave 跟各節點對應關係

```cmd
redis-cli --cluster check 127.0.0.1:6379 -a redisCluster
```

結果如下

```
I have no name!@e11ac0ec56aa:/$ redis-cli --cluster check 172.22.0.3:6379 -a redisCluster
Warning: Using a password with '-a' or '-u' option on the command line interface may not be safe.
172.22.0.5:6379 (7186bbf7...) -> 0 keys | 5462 slots | 1 slaves.
172.22.0.2:6379 (3decf740...) -> 0 keys | 5461 slots | 1 slaves.
172.22.0.4:6379 (ee76b9f8...) -> 0 keys | 5461 slots | 1 slaves.
[OK] 0 keys in 3 masters.
0.00 keys per slot on average.
>>> Performing Cluster Check (using node 172.22.0.3:6379)
S: 2c25a1a5d1cb1ba780170685c4f39dfe6f0da8f0 172.22.0.3:6379
   slots: (0 slots) slave
   replicates 3decf740935f98a40d2d73416937e63abc3f4781
M: 7186bbf7a1689d66c94a448cb1a197a7bd9b9e5f 172.22.0.5:6379
   slots:[5461-10922] (5462 slots) master
   1 additional replica(s)
M: 3decf740935f98a40d2d73416937e63abc3f4781 172.22.0.2:6379
   slots:[0-5460] (5461 slots) master
   1 additional replica(s)
M: ee76b9f8f8c261918b44caf856070acab1a5072a 172.22.0.4:6379
   slots:[10923-16383] (5461 slots) master
   1 additional replica(s)
S: c93879a2f37ab14b9bb25f54a8e856a2526e4621 172.22.0.7:6379
   slots: (0 slots) slave
   replicates 7186bbf7a1689d66c94a448cb1a197a7bd9b9e5f
S: bbeafa6b1e703035c364a2d0f476951268d0a9ff 172.22.0.6:6379
   slots: (0 slots) slave
   replicates ee76b9f8f8c261918b44caf856070acab1a5072a
[OK] All nodes agree about slots configuration.
>>> Check for open slots...
>>> Check slots coverage...
[OK] All 16384 slots covered.
```

依照上面資訊我們可以畫出架構如圖

![](https://i.imgur.com/0GiiHuE.png)

* Master 1 => slave 1 = 172.22.0.2 => 172.22.0.3
* Master 2 => slave 2 = 172.22.0.5 => 172.22.0.7
* Master 3 => slave 3 = 172.22.0.4 => 172.22.0.6

> redis 幫我們分配 master 對應 slave 並沒有特別順序，所以你建立的跟我建立很可能不一樣

我們可以在 cmd 透過 `docker inspect -f '{{.Name}} - {{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $(docker ps -q)` 顯示 container 對應使用 ip

```
$ docker inspect -f '{{.Name}} - {{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $(docker ps -q)
/rediscluster_redis-node-5_1 - 172.22.0.7
/rediscluster_redis-node-2_1 - 172.22.0.4
/rediscluster_redis-node-4_1 - 172.22.0.3
/rediscluster_redis-node-3_1 - 172.22.0.6
/rediscluster_redis-node-1_1 - 172.22.0.5
/rediscluster_redis-node-0_1 - 172.22.0.2
```

> 使用上面命令可以更好理解，Container Name之間對應關係

### 在 Redis Clsuter 新增 key 注意事項

我進入其中一台 master container `rediscluster_redis-node-0_1`

```text
I have no name!@e11ac0ec56aa:/$ redis-cli -h 172.22.0.2 -a redisCluster
Warning: Using a password with '-a' or '-u' option on the command line interface may not be safe.
172.22.0.2:6379> set k1 1
(error) MOVED 12706 172.22.0.4:6379
172.22.0.2:6379> set k2 1
OK
172.22.0.2:6379> set k3 1
OK
172.22.0.2:6379> set k4 1
(error) MOVED 8455 172.22.0.5:6379
172.22.0.2:6379> set k5 1
(error) MOVED 12582 172.22.0.4:6379
```

使用 `get` 拿資料時發現我們只拿的到 `k2,k3` 的資料

```text
172.22.0.2:6379> get k1
(error) MOVED 12706 172.22.0.4:6379
172.22.0.2:6379> get k2
"1"
172.22.0.2:6379> get k3
"1"
172.22.0.2:6379> get k4
(error) MOVED 8455 172.22.0.5:6379
172.22.0.2:6379> get k5
(error) MOVED 12582 172.22.0.4:6379
```

我們可以透過 `CLUSTER KEYSLOT {key}` 來算出我們 key 算出來的 slot 數值

```text
172.22.0.2:6379> CLUSTER KEYSLOT k1
(integer) 12706
172.22.0.2:6379> CLUSTER KEYSLOT k2
(integer) 449
172.22.0.2:6379> CLUSTER KEYSLOT k3
(integer) 4576
172.22.0.2:6379> CLUSTER KEYSLOT k4
(integer) 8455
172.22.0.2:6379> CLUSTER KEYSLOT k5
(integer) 12582
```

因為我們是進入 `172.22.0.2` 這台 Redis server 操作 我們只能操作屬於我們 slot number 也就是介於 `[0-5460]` slot 資料，所以只有 `k2,k3` 符合

![](https://i.imgur.com/uRvqz9a.png)

那你會想這樣不就跟單機操作一樣，別急讓我來說怎麼正確使用 Redis Cluster

我們先退出當前 redis-cli 操作 `redis-cli -h 172.22.0.2 -a redisCluster -c` 在此輸入並在最後多一個參數 `-c`

> `-c` 代表是要使用 cluster 操作

在執行 `set {key} {value}` 命令，會發現已經沒有 error 但取而代之是 `Redirected to slot`

那是因為 `-c` 模式下會幫我們在資料寫入前轉到該 slot redis server node 中在執行命令.

所以能發現出現 `Redirected to slot` Redis server ip 就會改變

```text
I have no name!@e11ac0ec56aa:/$ redis-cli -h 172.22.0.2 -a redisCluster -c
Warning: Using a password with '-a' or '-u' option on the command line interface may not be safe.
172.22.0.2:6379> set k1 1
-> Redirected to slot [12706] located at 172.22.0.4:6379
OK
172.22.0.4:6379> set k2 1
-> Redirected to slot [449] located at 172.22.0.2:6379
OK
172.22.0.2:6379> set k3 1
OK
172.22.0.2:6379> set k4 1
-> Redirected to slot [8455] located at 172.22.0.5:6379
OK
172.22.0.5:6379> set k5 1
-> Redirected to slot [12582] located at 172.22.0.4:6379
OK
```

> 所以在操作 Redis cluster 記得要使用 cluster 操作模式

## 擴充 Redis Cluster

假如因為業務需求流量突然進來我們想在目前 Cluster 多開幾組 master-slave redis 怎辦?

這邊跟大家來分享如何多加 redis 進入 Cluster 中

### 擴充步驟

1. 在 redis-cluster 中新增多組(或一組) master-slave redis
2. 將新建立 master container 加入 Cluster 中
3. 把目前 Cluster slot 分配給 剛建立 master container (如果可以最好 slot 數量平均)
4. 將新建立 slave container 掛入新建立 master container 上

在 redis-cluster 中新增多組(或一組) master-slave redis，使用下面命令

> 因為在使用 docker-compose 啟動時已經幫我們建立一個 network 所以我們需要指定新加的兩個 Container 在 `rediscluster_default` 中這樣他們才可以訪問的到

```
docker run -d --name redis-new-master01 --network rediscluster_default --privileged=true  -p 8106:6379 redis:6.2 --cluster-enabled yes --appendonly yes  --requirepass "redisCluster"

docker run -d --name redis-new-slave01 --network rediscluster_default --privileged=true  -p 8107:6379 redis:6.2 --cluster-enabled yes --appendonly yes  --requirepass "redisCluster"
```

在利用 `docker ps` 我們發現剛建立的兩個 Container 已經建立完畢

```
CONTAINER ID   IMAGE                       COMMAND                  CREATED          STATUS         PORTS                                       NAMES
e2c017e625db   redis:6.2                   "docker-entrypoint.s…"   4 seconds ago    Up 3 seconds   0.0.0.0:8107->6379/tcp, :::8107->6379/tcp   redis-new-slave01
0bc645dc5ba8   redis:6.2                   "docker-entrypoint.s…"   11 seconds ago   Up 9 seconds   0.0.0.0:8106->6379/tcp, :::8106->6379/tcp   redis-new-master01
1a0c740cbb96   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   3 hours ago      Up 3 hours     0.0.0.0:8105->6379/tcp, :::8105->6379/tcp   rediscluster_redis-node-5_1
1651a81a286f   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   3 hours ago      Up 3 hours     0.0.0.0:8102->6379/tcp, :::8102->6379/tcp   rediscluster_redis-node-2_1
5d93edfc55e6   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   3 hours ago      Up 3 hours     0.0.0.0:8104->6379/tcp, :::8104->6379/tcp   rediscluster_redis-node-4_1
8ab5bbbb7364   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   3 hours ago      Up 3 hours     0.0.0.0:8103->6379/tcp, :::8103->6379/tcp   rediscluster_redis-node-3_1
8edf90bed3fb   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   3 hours ago      Up 3 hours     0.0.0.0:8101->6379/tcp, :::8101->6379/tcp   rediscluster_redis-node-1_1
e11ac0ec56aa   bitnami/redis-cluster:6.2   "/opt/bitnami/script…"   3 hours ago      Up 3 hours     0.0.0.0:8100->6379/tcp, :::8100->6379/tcp   rediscluster_redis-node-0_1

$ docker inspect -f '{{.Name}} - {{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $(docker ps -q)
/redis-new-slave01 - 172.22.0.9
/redis-new-master01 - 172.22.0.8
/rediscluster_redis-node-5_1 - 172.22.0.7
/rediscluster_redis-node-2_1 - 172.22.0.4
/rediscluster_redis-node-4_1 - 172.22.0.3
/rediscluster_redis-node-3_1 - 172.22.0.6
/rediscluster_redis-node-1_1 - 172.22.0.5
/rediscluster_redis-node-0_1 - 172.22.0.2
```

查詢到 `redis-new-master01` 使用 IP 我們可以利用 `--cluster add-node` 將新的 master node 加入 cluster 中

```sh
redis-cli --cluster add-node  172.22.0.8:6379 172.22.0.2:6379 -a redisCluster
```

加入成功後我們在利用 `--cluster check` 命令查詢 Cluster 資訊，會看到目前多了一筆資料 `172.22.0.8:6379 (0885f85b...) -> 0 keys | 0 slots | 0 slaves.` 下一步就需要分配 slot 給這個 master

```sh
I have no name!@e11ac0ec56aa:/$  redis-cli --cluster check 172.22.0.3:6379 -a redisCluster
Warning: Using a password with '-a' or '-u' option on the command line interface may not be safe.
172.22.0.5:6379 (7186bbf7...) -> 1 keys | 5462 slots | 1 slaves.
172.22.0.2:6379 (3decf740...) -> 2 keys | 5461 slots | 1 slaves.
172.22.0.4:6379 (ee76b9f8...) -> 2 keys | 5461 slots | 1 slaves.
172.22.0.8:6379 (0885f85b...) -> 0 keys | 0 slots | 0 slaves.
[OK] 5 keys in 4 masters.
0.00 keys per slot on average.
>>> Performing Cluster Check (using node 172.22.0.3:6379)
S: 2c25a1a5d1cb1ba780170685c4f39dfe6f0da8f0 172.22.0.3:6379
   slots: (0 slots) slave
   replicates 3decf740935f98a40d2d73416937e63abc3f4781
M: 7186bbf7a1689d66c94a448cb1a197a7bd9b9e5f 172.22.0.5:6379
   slots:[5461-10922] (5462 slots) master
   1 additional replica(s)
M: 3decf740935f98a40d2d73416937e63abc3f4781 172.22.0.2:6379
   slots:[0-5460] (5461 slots) master
   1 additional replica(s)
M: ee76b9f8f8c261918b44caf856070acab1a5072a 172.22.0.4:6379
   slots:[10923-16383] (5461 slots) master
   1 additional replica(s)
S: c93879a2f37ab14b9bb25f54a8e856a2526e4621 172.22.0.7:6379
   slots: (0 slots) slave
   replicates 7186bbf7a1689d66c94a448cb1a197a7bd9b9e5f
M: 0885f85b94f8ddc186f1eac8f532be532fb7f5b1 172.22.0.8:6379
   slots: (0 slots) master
S: bbeafa6b1e703035c364a2d0f476951268d0a9ff 172.22.0.6:6379
   slots: (0 slots) slave
   replicates ee76b9f8f8c261918b44caf856070acab1a5072a
[OK] All nodes agree about slots configuration.
>>> Check for open slots...
>>> Check slots coverage...
[OK] All 16384 slots covered.
```

目前要做的是把當前 Cluster 上 master 上的 slot 分配給，剛建立 master container，達到資料分擔

我們可以透過 `redis-cli --cluster reshard {被分配 Redis Cluster 節點 IP}`，由上面資訊得知新建立 master id 是 `172.22.0.8:6379`

```cmd
redis-cli --cluster reshard 172.22.0.8:6379 -a redisCluster
```

執行後我們會被詢問要轉移多少 slot 數量給新的 master，我們知道 Redis Cluster 有 16384 個 slot 所以為了平均分配 `16384/4 = 4096` 

* `How many slots do you want to move (from 1 to 16384)` 我們可以輸入 `4096`
* `What is the receiving node ID?` 我們由上面資訊知道 新建立 master id 是 `0885f85b94f8ddc186f1eac8f532be532fb7f5b1`

最後可以輸入 `all` 就會開始分配 slot

```cmd
Please enter all the source node IDs.
  Type 'all' to use all the nodes as source nodes for the hash slots.
  Type 'done' once you entered all the source nodes IDs.
Source node #1: all
```

跑完後我們利用  `redis-cli --cluster check 172.22.0.8:6379 -a redisCluster` 查詢可以看到 slot 已經平均分配到四個 master 上面了

```cmd
172.22.0.8:6379 (0885f85b...) -> 1 keys | 4096 slots | 0 slaves.
172.22.0.4:6379 (ee76b9f8...) -> 2 keys | 4096 slots | 1 slaves.
172.22.0.2:6379 (3decf740...) -> 1 keys | 4096 slots | 1 slaves.
172.22.0.5:6379 (7186bbf7...) -> 1 keys | 4096 slots | 1 slaves.
[OK] 5 keys in 4 masters.
0.00 keys per slot on average.
>>> Performing Cluster Check (using node 172.22.0.8:6379)
M: 0885f85b94f8ddc186f1eac8f532be532fb7f5b1 172.22.0.8:6379
   slots:[0-1364],[5461-6826],[10923-12287] (4096 slots) master
M: ee76b9f8f8c261918b44caf856070acab1a5072a 172.22.0.4:6379
   slots:[12288-16383] (4096 slots) master
   1 additional replica(s)
M: 3decf740935f98a40d2d73416937e63abc3f4781 172.22.0.2:6379
   slots:[1365-5460] (4096 slots) master
   1 additional replica(s)
S: c93879a2f37ab14b9bb25f54a8e856a2526e4621 172.22.0.7:6379
   slots: (0 slots) slave
   replicates 7186bbf7a1689d66c94a448cb1a197a7bd9b9e5f
S: bbeafa6b1e703035c364a2d0f476951268d0a9ff 172.22.0.6:6379
   slots: (0 slots) slave
   replicates ee76b9f8f8c261918b44caf856070acab1a5072a
S: 2c25a1a5d1cb1ba780170685c4f39dfe6f0da8f0 172.22.0.3:6379
   slots: (0 slots) slave
   replicates 3decf740935f98a40d2d73416937e63abc3f4781
M: 7186bbf7a1689d66c94a448cb1a197a7bd9b9e5f 172.22.0.5:6379
   slots:[6827-10922] (4096 slots) master
   1 additional replica(s)
[OK] All nodes agree about slots configuration.
>>> Check for open slots...
>>> Check slots coverage...
[OK] All 16384 slots covered.
```

最後一步就是把 新的 slave redis 掛載到剛剛的 master redis 上

我們可以利用下面命令 template 來處理

```cmd
redis-cli --cluster add-node {new-slave-redis IP} {cluster-redis IP}  --cluster-slave --cluster-master-id 新節點master-id -a redisCluster
```

轉換成可執行命令如下

```cmd
redis-cli --cluster add-node 172.22.0.9:6379 172.22.0.8:6379  --cluster-slave --cluster-master-id 0885f85b94f8ddc186f1eac8f532be532fb7f5b1 -a redisCluster
```

看到下面執行結果代表掛載成功

```cmd
//...
[OK] All nodes agree about slots configuration.
>>> Check for open slots...
>>> Check slots coverage...
[OK] All 16384 slots covered.
>>> Send CLUSTER MEET to node 172.22.0.9:6379 to make it join the cluster.
Waiting for the cluster to join

>>> Configure node as replica of 172.22.0.8:6379.
[OK] New node added correctly.
```

全部執行完後結果，我們就有四個節點可以存放 Redis 資料

![](https://i.imgur.com/up3oWrr.png)

#### cluster rebalance 命令

如果 每個 master node 分配 slot 數量不小心設定錯誤可以使用  `--cluster rebalance` 來重新分配 slot 數量，讓每個 master 得到平衡的 slot 數量

```cmd
redis-cli --cluster rebalance 172.21.0.8:6379 -a redisCluster
```

### 限縮 Redis Cluster

## 小結

如果會有 Redis 為何使用 16384 當作 slot 數量可以參考作者回答的資料 [why redis-cluster use 16384 slots](https://github.com/redis/redis/issues/2576)

## 參考資料

[cluster-spec](https://redis.io/topics/cluster-spec)

[Redis cluster tutorial](https://redis.io/topics/cluster-tutorial)
