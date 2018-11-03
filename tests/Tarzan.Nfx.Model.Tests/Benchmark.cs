using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Linq;

namespace Tarzan.Nfx.Model.Tests
{
    public class Benchmark
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running benchmarks...");
            BenchmarkRunner.Run<FlowKeyHashBenchmark>(new AllowNonOptimized());
            Console.WriteLine("Done.");
        }
                public class AllowNonOptimized : ManualConfig
        {
            public AllowNonOptimized()
            {
                Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS
                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
            }
        }
    }
}
