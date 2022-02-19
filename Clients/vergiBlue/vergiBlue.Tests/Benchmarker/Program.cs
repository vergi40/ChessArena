using System;
using BenchmarkDotNet.Running;

namespace Benchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PerftBenchmark>();
            
            Console.ReadKey();
        }
    }
}
