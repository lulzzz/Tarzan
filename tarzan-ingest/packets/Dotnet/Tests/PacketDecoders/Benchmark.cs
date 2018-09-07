using BenchmarkDotNet.Running;
using static PacketDecodersTest.ComputeFlowsBenchmark;

namespace PacketDecodersTest
{
    class Benchmark
    {
        public static void Main(string[] args)
        {

            BenchmarkRunner.Run<ComputeFlowsBenchmark>(new AllowNonOptimized());

            BenchmarkRunner.Run<GetKeyBenchmark>(new AllowNonOptimized());
        }
    }
}
