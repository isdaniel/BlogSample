## 前言

對於常使用的物件和變數我們會放置在 Redis 上( cache server )，但記憶體有限，我們想放的東西無限，造就無法將所有物件都存放在 Redis 或 Server Memory 上.

假如你有設定過 Redis config 你會看到一個 property `maxmemory_policy` 來管理如何淘汰，記憶體中的物件

* volatile-lru
* allkeys-lru

上面有個名稱　LRU (Least Recently Used Cache)　代表甚麼意涵呢？稍後會跟大家做解釋

## LRU

### LRU 算法概念

## 實現 LRU 核心資料結構

> HashTable + LinkedList

## 小結

