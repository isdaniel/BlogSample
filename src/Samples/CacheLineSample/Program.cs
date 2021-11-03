using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CacheLineSample
{
    public class NonPadding
    {
        public int Val1;
        public int Val2;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public class Padding
    {
        [System.Runtime.InteropServices.FieldOffset(60)]
        public int Val1;

        [System.Runtime.InteropServices.FieldOffset(124)]
        public int Val2;
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Task> tasks = new List<Task>();

            NonPadding nonPadding = new NonPadding();
            Padding padding=new Padding();

            int times = 1000000000;

            tasks.Add(Task.Run(()=>{
                Stopwatch sw =new Stopwatch();
                sw.Start();
                for (int i = 0; i < times; i++)
                {
                    nonPadding.Val1++;
                }
                sw.Stop();
                System.Console.WriteLine($"nonPadding Val1 cost time is {sw.ElapsedMilliseconds}");
            }));

            tasks.Add(Task.Run(()=>{
                Stopwatch sw =new Stopwatch();
                sw.Start();
                for (int i = 0; i < times; i++)
                {
                    nonPadding.Val2++;
                }
                sw.Stop();
                System.Console.WriteLine($"nonPadding Val2 cost time is {sw.ElapsedMilliseconds}");
            }));

            tasks.Add(Task.Run(()=>{
                Stopwatch sw =new Stopwatch();
                sw.Start();
                for (int i = 0; i < times; i++)
                {
                    padding.Val1++;
                }
                sw.Stop();
                System.Console.WriteLine($"padding Val1 cost time is {sw.ElapsedMilliseconds}");
            }));

            tasks.Add(Task.Run(()=>{
                Stopwatch sw =new Stopwatch();
                sw.Start();
                for (int i = 0; i < times; i++)
                {
                    padding.Val2++;
                }
                sw.Stop();
                System.Console.WriteLine($"padding Val2 cost time is {sw.ElapsedMilliseconds}");
            }));
            
            Task.WaitAll(tasks.ToArray());

            
            Task.Run(()=>{
                System.Console.WriteLine("----------------for loop--------------------");
                int size = 20000;
                int[,] arr1 = new int[size,size];

                int temp;

                Stopwatch sw =new Stopwatch();
                sw.Start();
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        temp = arr1[i,j];
                    }
                }
                sw.Stop();
                System.Console.WriteLine($"use cache line {sw.ElapsedMilliseconds}");

                sw.Restart();
                for (int j = 0; j < size; j++)
                {
                    for (int i = 0; i < size; i++)
                    {
                        temp = arr1[i,j];
                    }
                }
                sw.Stop();
                System.Console.WriteLine($"miss cache line {sw.ElapsedMilliseconds}");
            }).Wait();
        }
    }
}
