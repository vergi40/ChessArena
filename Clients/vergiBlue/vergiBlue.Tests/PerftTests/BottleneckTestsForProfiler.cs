using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using vergiBlue.Logic;

namespace PerftTests
{
    [TestFixture]
    class BottleneckTestsForProfiler
    {
        [Test]
        public void GoodPositions_ID_Depth4()
        {
            var args = CaseBoards.GetGoodPositions();

            var logic = LogicFactory.CreateForTest(true, args.board);
            logic.Settings.UseTranspositionTables = false;
            logic.Settings.UseIterativeDeepening = true;
            logic.Settings.UseParallelComputation = false;

            // Debug marker 1
            var move = logic.CreateMoveWithDepth(4);

            // Debug marker 2
        }

        [Test]
        public void GoodPositions_TT_Depth4()
        {
            var args = CaseBoards.GetGoodPositions();

            var logic = LogicFactory.CreateForTest(true, args.board);
            logic.Settings.UseTranspositionTables = true;
            logic.Settings.UseIterativeDeepening = false;
            logic.Settings.UseParallelComputation = false;

            // Debug marker 1
            var move = logic.CreateMoveWithDepth(4);

            // Debug marker 2
        }
    }
}
