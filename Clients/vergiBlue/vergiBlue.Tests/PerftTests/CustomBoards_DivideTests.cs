using System.Linq;
using CommonNetStandard.Interface;
using NUnit.Framework;
using vergiBlue;
using vergiBlue.BoardModel;

namespace PerftTests
{
    [TestFixture]
    class CustomBoards_DivideTests
    {

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
            var next = BoardFactory.CreateFromMove(board, new SingleMove("c7", "c8") { PromotionType = PromotionPieceType.Bishop });
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

        [Test]
        public void r3k2r_base_Divide()
        {
            var board = BoardFactory.CreateFromFen(
                "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", out var whiteStarts);

            var nodes = Perft.Divide(board, 3, true);
            TestContext.WriteLine($"Total: {nodes}");
        }

        [Test]
        public void Castling_Divide()
        {

        }
    }
}
