using BenchmarkDotNet.Attributes;
using PerftTests;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace Benchmarker
{
    public class SearchBenchmarkLatestFeatures
    {
        public IBoard InitializedBoard => CaseBoards.GetGoodPositions().board;

        [Benchmark]
        public void GoodPositions_ID_TT_Depth5()
        {
            var board = BoardFactory.CreateClone(InitializedBoard);

            var logic = LogicFactory.CreateForTest(true, board);
            logic.Settings.UseTranspositionTables = true;
            logic.Settings.UseIterativeDeepening = true;
            logic.Settings.UseParallelComputation = false;

            // Debug marker 1
            var move = logic.CreateMoveWithDepth(5);

            // Debug marker 2
        }
    }
}
