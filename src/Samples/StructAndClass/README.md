# Struct **V.S** Class

---

*Struct*和*Class*基本上一樣

除了...

| **不同之處**   | Struct            | Class                   |
| :------------: | :---------------: | :---------------------: |
| **型態**       | 值類型            | 參考類型                |
| **放置記憶體** | Stack上           | Heap上                  |
| **繼承**       | 只能實現Interface | 可繼承也可實現Interface |
| **NULL**       | 不能NULL          | 可指向NULL              |

---

## 使用幾個簡單範例來說明

    public class ClassA
    {

    }

    public struct StructType 
    {
        public ClassA a { get; set; }
    }

創建兩個 *Struct* 物件，並且比較可以得到 **True**

    [TestMethod]
    public void StructEquals1()
    {
        StructType s = new StructType();
        StructType s1 = new StructType();
        Assert.IsTrue(s.Equals(s1));
    }

創建兩個 *Struct* 物件，並且比較可以得到 **True**

`Equals` 方法比較存放在*Stack*執行個體是否一樣

`s`和`s1`都是 `StructType`

    [TestMethod]
    public void StructEquals2()
    {
        StructType s = new StructType();
        StructType s1 = new StructType();

        s.a = new ClassA();

        Assert.AreEqual(s.Equals(s1),false);
    }

    [TestMethod]
    public void StructEquals3()
    {
        StructType s = new StructType();
        StructType s1 = new StructType();

        ClassA aObj = new ClassA();

        s.a = aObj;
        s1.a = aObj;

        Assert.IsTrue(s.Equals(s1));
    }



*Struct*和*Class*牽扯一些記憶體位置細節想了解可看看小弟之前寫的 [記憶體Heap，Stack解說][1] 

  [1]: https://dotblogs.com.tw/daniel/2017/10/20/174725        "Heap 參考類型 V.S Stack 值類型 ??"
