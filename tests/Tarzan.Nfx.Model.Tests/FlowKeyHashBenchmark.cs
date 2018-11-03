using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;
namespace Tarzan.Nfx.Model.Tests
{
    /// <summary>
    /// Implements benchmarks for various hash functions for key flow.
    /// The key flow has a fixed length of 40 bytes. We need a hash function
    /// that has a good properties and is really fast.
    /// </summary>
    [CoreJob]
    public class FlowKeyHashBenchmark
    {
        
    }
}