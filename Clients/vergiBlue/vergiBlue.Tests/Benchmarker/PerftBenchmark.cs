using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerftTests;

namespace Benchmarker
{
    [SimpleJob(RunStrategy.Monitoring)]
    [MeanColumn, MedianColumn, MinColumn, MaxColumn]
    public class PerftBenchmark
    {
        [Benchmark]
        [Arguments(4)]
        public void StartPosition(int depth)
        {
            Cases.StartPosition(depth);
        }

        [Benchmark]
        [Arguments(3)]
        public void GoodPosition(int depth)
        {
            Cases.GoodPositions_AndrewWagner(depth);
        }

        [Benchmark]
        [Arguments(4)]
        public void PromomotionPosition(int depth)
        {
            Cases.Promotion_AndrewWagner(depth);
        }
    }
}
