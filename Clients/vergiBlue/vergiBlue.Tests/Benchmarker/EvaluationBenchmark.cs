using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerftTests;
using vergiBlue.BoardModel;

namespace Benchmarker
{
    [SimpleJob(RunStrategy.Monitoring, targetCount: 15)]
    [MeanColumn, MedianColumn, MinColumn, MaxColumn, MemoryDiagnoser]
    public class EvaluationBenchmark
    {
        [Params(100000, 1000000)]
        public int N { get; set; }

        private readonly IBoard _startBoard;
        private readonly IBoard _goodBoard;
        private readonly IBoard _promotionBoard;
        public EvaluationBenchmark()
        {
            _startBoard = BoardFactory.CreateDefault();
            _goodBoard = CaseBoards.GetGoodPositions().board;
            _promotionBoard = CaseBoards.GetPromotion().board;
        }

        [Benchmark]
        public void StartPosition()
        {
            for (int i = 0; i < N; i++)
            {
                var eval = _startBoard.Evaluate(true, false);
            }
        }

        [Benchmark]
        public void GoodPosition()
        {
            for(int i = 0; i < N; i++)
            {
                var eval = _goodBoard.Evaluate(true, false);
            }
        }

        [Benchmark]
        public void PromomotionPosition()
        {
            for (int i = 0; i < N; i++)
            {
                var eval = _promotionBoard.Evaluate(false, false);
            }
        }
    }
}
