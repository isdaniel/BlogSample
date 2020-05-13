using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Box_UnBoxing
{
    class Program
    {
        static void Main(string[] args)
        {
            int times = 30000000;
            string s = string.Empty;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < times ; i++)
            {
                s = $"{times}";
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            sw.Restart();
            for (int i = 0; i < times ; i++)
            {
                s = $"{times.ToString()}";
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
