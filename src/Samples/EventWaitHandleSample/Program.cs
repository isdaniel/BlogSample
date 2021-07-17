using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventWaitHandleSample
{
    /// <summary>
    /// 假如有一個面試題目是
    /// 目前有三個Thread 每個Thread個別負責Print "A","B","C"
    /// 要求:請按照A,B,C順序打印出20次資訊,中間不能錯號
    /// ex:
    /// A
    /// B
    /// C
    /// A
    /// B
    /// C
    /// ..
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Caller c = new Caller();
            var t1 = new Thread(() => c.Call(new CallerInfo() { Index = 1,Name = "A"}));
            var t2 = new Thread(() => c.Call(new CallerInfo() { Index = 2, Name = "B" }));
            var t3 = new Thread(() => c.Call(new CallerInfo() { Index = 3, Name = "C" }));

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            Console.ReadKey();
        }
    }

    public class CallerInfo {
        public int Index { get; set; }
        public string Name { get; set; }
    }

    public class Caller {
        private class NotifyMap {
            public AutoResetEvent Wait { get; set; } 
            public AutoResetEvent Notify { get; set; }
        }
        private Dictionary<int, NotifyMap> _notifyMapping;
        private volatile int index = 0;

        AutoResetEvent notifyA = new AutoResetEvent(false);
        AutoResetEvent notifyB = new AutoResetEvent(false);
        AutoResetEvent notifyC = new AutoResetEvent(false);

        public Caller()
        {
            _notifyMapping = new Dictionary<int, NotifyMap>
            {
                { 1, new NotifyMap{ Wait = notifyA, Notify = notifyB} },
                { 2, new NotifyMap{ Wait = notifyB, Notify = notifyC} },
                { 3, new NotifyMap{ Wait = notifyC, Notify = notifyA} }
            };

        }


        public void Call(CallerInfo caller)
        {
            for (int i = 0; i < 20; i++)
            {
                var key = index % _notifyMapping.Count + 1;
                var notifyMap = _notifyMapping[caller.Index];
                if (key != caller.Index)
                    notifyMap.Wait.WaitOne();
                Console.WriteLine($"[{i}]:{caller.Name}");
                index++;
                notifyMap.Notify.Set();
            }
        }

        public void CallA()
        {
            for (int i = 0; i < 20; i++)
            {
                if (index != 1)
                    notifyA.WaitOne();

                Console.WriteLine("A");
                index = 2;
                notifyB.Set();
            }

        }

        public void CallB()
        {
            for (int i = 0; i < 20; i++)
            {
                if (index != 2)
                    notifyB.WaitOne();

                Console.WriteLine("B");
                index = 3;
                notifyC.Set();
            }

        }
        public void CallC()
        {
            for (int i = 0; i < 20; i++)
            {
                if (index != 3)
                    notifyC.WaitOne();

                Console.WriteLine("C");
                Console.WriteLine("------------------------------");
                index = 1;
                notifyA.Set();
            }
        }
    }

}
