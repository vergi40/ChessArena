using System.Runtime.InteropServices.ComTypes;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerftTests;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace Benchmarker
{
    [SimpleJob(RunStrategy.Monitoring, targetCount: 15)]
    [MeanColumn, MedianColumn, MinColumn, MaxColumn, MemoryDiagnoser]
    public class SearchBenchmarkSmall
    {
        public IBoard InitializedBoard => CaseBoards.GetGoodPositions().board;

        [Benchmark]
        public void GoodPositions_ID_Depth4()
        {
            var board = BoardFactory.CreateClone(InitializedBoard);

            var logic = LogicFactory.CreateForTest(true, board);
            logic.Settings.UseTranspositionTables = false;
            logic.Settings.UseIterativeDeepening = true;
            logic.Settings.UseParallelComputation = false;

            // Debug marker 1
            var move = logic.CreateMoveWithDepth(4);

            // Debug marker 2
        }

        [Benchmark]
        public void GoodPositions_TT_Depth4()
        {
            var board = BoardFactory.CreateClone(InitializedBoard);

            var logic = LogicFactory.CreateForTest(true, board);
            logic.Settings.UseTranspositionTables = true;
            logic.Settings.UseIterativeDeepening = false;
            logic.Settings.UseParallelComputation = false;

            // Debug marker 1
            var move = logic.CreateMoveWithDepth(4);

            // Debug marker 2
        }
    }
}
