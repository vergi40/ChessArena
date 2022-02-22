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
    [SimpleJob(RunStrategy.Monitoring, targetCount:15)]
    [MeanColumn, MedianColumn, MinColumn, MaxColumn]
    public class SearchBenchmark
    {

        [Params(4,5)]
        public int Depth { get; set; }

        [Params(false, true)]
        public bool UseID { get; set; }

        [Benchmark]
        public void StartPosition()
        {
            var board = BoardFactory.CreateDefault();

            var logic = LogicFactory.CreateForTest(true, board);
            logic.Settings.UseTranspositionTables = UseID;
            logic.Settings.UseIterativeDeepening = false;
            logic.Settings.UseParallelComputation = false;

            var move = logic.CreateMoveWithDepth(Depth);
        }

        [Benchmark]
        public void GoodPosition()
        {
            var (board, whiteStarts) = CaseBoards.GetGoodPositions();

            var logic = LogicFactory.CreateForTest(whiteStarts, board);
            logic.Settings.UseTranspositionTables = UseID;
            logic.Settings.UseIterativeDeepening = false;
            logic.Settings.UseParallelComputation = false;

            var move = logic.CreateMoveWithDepth(Depth);
        }

        [Benchmark]
        public void PromomotionPosition()
        {
            var (board, whiteStarts) = CaseBoards.GetPromotion();

            var logic = LogicFactory.CreateForTest(whiteStarts, board);
            logic.Settings.UseTranspositionTables = UseID;
            logic.Settings.UseIterativeDeepening = false;
            logic.Settings.UseParallelComputation = false;

            var move = logic.CreateMoveWithDepth(Depth);
        }


    }
}
