using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard.Interface;
using NUnit.Framework;
using vergiBlue;
using vergiBlue.BoardModel;

namespace PerftTests
{
    [TestFixture]
    class PerftBoards_ValidateNodesTests
    {
        [Test]
        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 20)]
        [TestCase(2, ExpectedResult = 400)]
        [TestCase(3, ExpectedResult = 8902)]
        [TestCase(4, ExpectedResult = 197281)]
        //[TestCase(5, ExpectedResult = 4865609)]
        public long StartPosition(int depth)
        {
            return Cases.StartPosition(depth);
        }

        /// <summary>
        /// Promotion, castling, en passant
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        [Test]
        [TestCase(1, ExpectedResult = 48)]
        [TestCase(2, ExpectedResult = 2039)]
        [TestCase(3, ExpectedResult = 97862)]
        [TestCase(4, ExpectedResult = 4085603)]
        //[TestCase(5, ExpectedResult = 193690690)]
        public long GoodPositions_AndrewWagner(int depth)
        {
            return Cases.GoodPositions_AndrewWagner(depth);
        }

        [Test]
        public void GoodPositions_TempDivide()
        {
            var (board, whiteStarts) = CaseBoards.GetGoodPositions();
            var next = BoardFactory.CreateFromMove(board, new SingleMove("d5", "d6"));

            //var nodes = Perft.Divide(board, 4, whiteStarts);
            //var nodes = Perft.Divide(next, 3, !whiteStarts);

            var next2 = BoardFactory.CreateFromMove(next, new SingleMove("b4", "b3"));
            var next3 = BoardFactory.CreateFromMove(next2, new SingleMove("d6", "e7", true));

            var faultMoves = next3.MoveGenerator.MovesQuick(false, true).ToList();
            //var nodes = Perft.Divide(next2, 2, whiteStarts);
            var nodes = Perft.Divide(next3, 1, !whiteStarts);

            TestContext.WriteLine($"Total: {nodes}");
        }

        [Test]
        public void PawnRow_TempDivide()
        {
            var board = BoardFactory.CreateFromFen("8/PPPk4/8/8/8/8/4Kppp/8 w - - 0 1", out var whiteStarts);

            //var nodes = Perft.Divide(board, 3, whiteStarts);
            var next = BoardFactory.CreateFromMove(board, new SingleMove("c7", "c8"){PromotionType = PromotionPieceType.Bishop});
            var nodes = Perft.Divide(next, 2, !whiteStarts);
            TestContext.WriteLine($"Total: {nodes}");

            return;

            //var nodes = Perft.Divide(next, 3, !whiteStarts);

            next = BoardFactory.CreateFromMove(next, new SingleMove("b4", "b3"));
            next = BoardFactory.CreateFromMove(next, new SingleMove("d6", "e7", true));

            var faultMoves = next.MoveGenerator.MovesQuick(false, true).ToList();
            //var nodes = Perft.Divide(next2, 2, whiteStarts);
            //var nodes = Perft.Divide(next, 1, !whiteStarts);

            TestContext.WriteLine($"Total: {nodes}");
        }

        [Test]
        public void r3k2r_Divide()
        {
            var board = BoardFactory.CreateFromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", out var whiteStarts);
            board.ExecuteMove(SingleMoveFactory.Create("e1f1"));
            board.ExecuteMove(SingleMoveFactory.Create("c7c5"));

            var nodes = Perft.Divide(board, 1, true);
            TestContext.WriteLine($"Total: {nodes}");

        }

        /// <summary>
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        [Test]
        [TestCase(1, ExpectedResult = 24)]
        [TestCase(2, ExpectedResult = 496)]
        [TestCase(3, ExpectedResult = 9483)]
        [TestCase(4, ExpectedResult = 182838)]
        [TestCase(5, ExpectedResult = 3605103)]
        public long Promotion_AndrewWagner(int depth)
        {
            return Cases.Promotion_AndrewWagner(depth);
        }
    }
}
