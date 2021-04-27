using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YieldSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //IEnumerator<int> ctx = new Counter01(100);
            //IEnumerator<int> ctx = new Counter02(100);
            //while (ctx.MoveNext())
            //{
            //    Console.WriteLine(ctx.Current);
            //}

            //foreach (var item in new CounterEnumerable02(100))
            //{
            //    Console.WriteLine(item);
            //}

            foreach (var item in Counter03.GetValue(100))
            {
                Console.WriteLine(item);
            }
            


            Console.ReadKey();
        }
    }
}
