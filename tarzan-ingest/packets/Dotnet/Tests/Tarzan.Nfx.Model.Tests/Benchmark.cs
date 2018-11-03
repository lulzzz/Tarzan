using System;
using BenchmarkDotNet.Running;
namespace Tarzan.Nfx.Model.Tests
{
    public class Benchmark
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running benchmarks...");
            BenchmarkRunner.Run<FlowKeyHashBenchmark>(); //new AllowNonOptimized());
            Console.WriteLine("Done.");
        }
    }
}
