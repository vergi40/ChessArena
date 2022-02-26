using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerftTests;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace Benchmarker
{
    [SimpleJob(RunStrategy.Throughput)]
    [MeanColumn, MedianColumn, MinColumn, MaxColumn]
    public class EvaluationBenchmark
    {
        [Params(10000)]
        public int N { get; set; }

        [Benchmark]
        public void StartPosition()
        {
            var board = BoardFactory.CreateDefault();

            for (int i = 0; i < N; i++)
            {
                var eval = board.Evaluate(true, false);
            }
        }

        [Benchmark]
        public void GoodPosition()
        {
            var (board, whiteStarts) = CaseBoards.GetGoodPositions();

            for(int i = 0; i < N; i++)
            {
                var eval = board.Evaluate(whiteStarts, false);
            }
        }

        [Benchmark]
        public void PromomotionPosition()
        {
            var (board, whiteStarts) = CaseBoards.GetPromotion();

            for (int i = 0; i < N; i++)
            {
                var eval = board.Evaluate(whiteStarts, false);
            }
        }
    }
}
