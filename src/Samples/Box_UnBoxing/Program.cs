using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace Box_UnBoxing
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkMeasure>(new Config());
            Console.Read();
        }
    }


    [BenchmarkCategory("Framework")]
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 1, invocationCount: 1, baseline: false)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkMeasure
    {
        int intVal = 1;
        int intVal1 = 2;
        int intVal2 = 2;
        int intVal3 = 3;


        string stringVal = "Hello World";
        [Benchmark(Description = "Int_NoUseToString")]
        public void Int_NoUseToString()
        {
            string result = $"{intVal} {intVal2} {intVal3}";
        }

        [Benchmark(Description = "Int_UseToString")]
        public void Int_UseToString()
        {
            string result = $"{intVal.ToString()} {intVal2.ToString()} {intVal3.ToString()}";
        }

        [Benchmark(Description = "String_NoUseToString")]
        public void String_NoUseToString()
        {
            string result = $"{stringVal}";
        }

        [Benchmark(Description = "String_UseToString")]
        public void String_UseToString()
        {
            string result = $"{stringVal.ToString()}";
        }

    }
}
