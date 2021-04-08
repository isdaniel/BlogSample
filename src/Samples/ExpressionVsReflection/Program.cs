using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace ExpressionVsReflection
{
    class Program
    {
        static void Main(string[] args)
        {
   

            var b = BenchmarkRunner.Run<BenchmarkMeasure>(new Config());
            Console.Read();
        }
    }

    [BenchmarkCategory("Framework")]
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 1, invocationCount: 1, baseline: false)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class BenchmarkMeasure
    {
        [Benchmark(Description = "ReflectionCreator no parameter")]
        public void ReflectionCreatorNoParameter()
        {
            ObjectProvider.ReflectionCreator<A>();
        }

        [Benchmark(Description = "ReflectionCreator had parameter")]
        public void ReflectionCreatorParameter()
        {
            ObjectProvider.ReflectionCreator<ParaA>(1);
        }

        [Benchmark(Description = "ExpressionCreator no parameter")]
        public void ExpressionCreatorNoParameter()
        {
            ObjectProvider.ExpressionCreator<A>();
        }

        [Benchmark(Description = "ExpressionCreator had parameter")]
        public void ExpressionCreatorParameter()
        {
            ObjectProvider.ExpressionCreator<ParaA>()(1);
        }
    }

    public class A
    {
    }

    public class ParaA
    {
        private readonly int _val;

        public ParaA(int val)
        {
            _val = val;
        }
    }
}
