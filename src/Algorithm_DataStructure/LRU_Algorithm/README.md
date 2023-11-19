## 前言

資料結構跟演算法是程式設計師的內功，在中小型系統懂這兩樣東西效用可能有限，但在大型系統中熟不熟用資料結構跟演算法做出來的效益會天差地遠

對於常使用的物件和變數我們會放置在 Redis 上( cache server )，但記憶體有限，我們想放的東西無限，造就無法將所有物件都存放在 Redis 或 Server Memory 上.

假如你有設定過 Redis config 你會看到一個 property `maxmemory_policy` 來管理如何淘汰，記憶體中的物件

* volatile-lru
* allkeys-lru

上面有個名稱　LRU (Least Recently Used Cache)　代表甚麼意涵呢？稍後會跟大家做解釋

## LRU 算法概念

LRU (Least Recently Used Cache) 顧名思義，會記憶體不夠用時優先保留最近常使用的 Cache 資料，並剔除最舊的資料來緩解壓力

這個算法策略可以確保 Hot Data 在記憶體不足時不會被刪除

> 在 Redis 除了 LRU 策略外還有其他 cache policy，但本篇會著重介紹 LRU

LRU 核心概念有幾點

* Get Data 如果有命中 Cache 會返回資料前，把資料拉到前面防止記憶體不夠時被清除
* Put Data 判斷是否已經存在資料
  * 有:更新資料並放在前面
  * 無:假如資料已經新增資料並放在前面

## 實現 LRU 核心資料結構

假如我說 LRU 在 CURD + 移動資料，時間複雜度都是 `O(1)`，讀者們能想到解法嗎?

如果單純 CURD 時間複雜度是 `O(1)` 的資料結構，可以想到 HashTable，但移動資料卻沒辦法 `O(1)`

如果要任意動資料時間複雜度的資料結構可以想到 DoubleLinkedList

說到這邊答案呼之欲出了，假如我們把　HashTable + DoubleLinkedList 這兩種資料結構都用運上不就可以了嗎?

### HashTable + DoubleLinkedList 概念

HashTable + DoubleLinkedList 邏輯圖如下

![](https://i.imgur.com/bd3VgSx.png)

假如使用者讀取 `key=XX3` 資料，結構會變成下圖

`XX3` 會被拉到 Header 那邊，當作最近被取得 Data 資料

![](https://i.imgur.com/mgafeEh.png)

假如現在使用者新增一筆資料 `Key=XX99` 但因為配置記憶體容量最多只能存 3 個單位，所以 LRU 淘汰策略會把 Tail 前資料刪除，並新增資料在 Header 節點後

結果如下圖

![](https://i.imgur.com/HRZnL5X.png)

依照上面特性我們可以整理出下面重點

* Get Data 如果有命中 Cache 會返回資料前，把資料拉到前面防止記憶體不夠時被清除
* Put Data 判斷是否已經存在資料
  * 有:更新資料並放在 Header 節點之後
  * 無:判斷目前存放資料數量是否達上限如果達到上限刪除最少使用的資料，並新增資料在 Header 節點之後

### 程式碼實現

既然談到 `DoubleLinkedList` 就不得先不提`Node`

`Node` 有兩個重要引用當作連接上下節點

* Prev
* Next

```c#
public class Node<Tkey,TValue>{
    public TValue Value { get; set; }
    public Tkey Key { get; set; }
    public Node<Tkey,TValue> Prev { get; set; }
    public Node<Tkey,TValue> Next { get; set; }
}

public class DoubleLinkedList<Tkey,TValue>{
    private Node<Tkey,TValue> _header;

    private Node<Tkey,TValue> _tail;
    public DoubleLinkedList()
    {
        _header = new Node<Tkey, TValue>();
        _tail = new Node<Tkey, TValue>();

        _header.Next = _tail;
        _tail.Prev = _header;
    }

    public void AddHeader(Node<Tkey,TValue> node){
        node.Next = _header.Next;
        node.Prev = _header;
        _header.Next.Prev = node;
        _header.Next = node;
    }

    public void RemoveNode(Node<Tkey,TValue> node){
        node.Next.Prev = node.Prev;
        node.Prev.Next = node.Next;
        node.Prev = node.Next = null;
    }

    public Node<Tkey,TValue> GetLastNode(){
        return _tail.Prev;
    }
    
    public void PrintAll(){
        var cur = _header.Next;
        while(cur != _tail){
            System.Console.WriteLine($"key: {cur.Key} value:{cur.Value}");
            cur = cur.Next;
        }
    }
}
```

在 `LRUCache` 類別中有兩個重要欄位 `Dictionary` 和 `DoubleLinkedList` 互相作用，我們透過 key 可以快速透過 `Dictionary` 找到資料並利用 `Dictionary` 值找到對用 `Node` 資料進行刪除或新增動作

![](https://i.imgur.com/bd3VgSx.png)

因為上面已經整理出算法概念

* Get Data 如果有命中 Cache 會返回資料前，把資料拉到前面防止記憶體不夠時被清除
* Put Data 判斷是否已經存在資料
  * 有:更新資料並放在 Header 節點之後
  * 無:判斷目前存放資料數量是否達上限如果達到上限刪除最少使用的資料，並新增資料在 Header 節點之後

後面我們針對上面資訊撰寫程式就容易許多了

```c#
 public class LRUCache {

    private Dictionary<int,Node<int,int>> _map;
    private DoubleLinkedList<int, int> _linkedList;
    private readonly int _capacity;

    public LRUCache(int capacity) {
        this._capacity = capacity;
        _map = new Dictionary<int, Node<int, int>>();
        _linkedList = new DoubleLinkedList<int,int>();
    }
    
    public int Get(int key) {
        if (_map.TryGetValue(key,out var node))
        {
            _linkedList.RemoveNode(node);
            _linkedList.AddHeader(node);
            return node.Value;
        }

        return -1;
    }
    
    public void Put(int key, int value) {

        var newNode = new Node<int,int>(){
            Value = value,
            Key = key
        };

        if (_map.TryGetValue(key,out var node))
        {
            _linkedList.RemoveNode(node);
            _linkedList.AddHeader(newNode);
            _map[key] = newNode;
        }else{
            if (_map.Count == _capacity)
            {
                var lastNode = _linkedList.GetLastNode();
                _linkedList.RemoveNode(lastNode);
                _map.Remove(lastNode.Key);
            }

            _map[key] = newNode;
            _linkedList.AddHeader(newNode);
        }
    }

    public void PrintAll(){
        _linkedList.PrintAll();
    }
}
```

## 小結

演算法跟資料結構實現不局限於程式語言，重點在他的概念核心

在一些熱門的 open source 會很常看到某些資料結構跟演算法的蹤跡，因為他們利用適合的演算法＋資料結構幫助大家解決高併發或效能問題，所以成為熱門架構

由我上面可以得知，優良程式設計師跟熟用演算法跟資料結構有密不可分的關係

就像 Redis 有針對 LRU 算法實現兩種方式

* volatile-lru
* allkeys-lru
