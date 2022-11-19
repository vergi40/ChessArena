using NUnit.Framework;
using System.Linq;
using vergiBlue.BoardModel;

namespace UnitTests.MoveGeneration
{
    // NOTE: Duplicated simpler cases from PertfTests projects

    [TestFixture]
    class PerftSimpleTests
    {
        [Test]
        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 20)]
        [TestCase(2, ExpectedResult = 400)]
        [TestCase(3, ExpectedResult = 8902)]
        [TestCase(4, ExpectedResult = 197281)]
        //[TestCase(5, ExpectedResult = 4865609)]
        public long PerftMoveCount_StartPosition_ShouldMatch(int depth)
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
        public long PerftMoveCount_GoodPositions_AndrewWagner_ShouldMatch(int depth)
        {
            return Cases.GoodPositions_AndrewWagner(depth);
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
        public long PerftMoveCount_Promotion_AndrewWagner_ShouldMatch(int depth)
        {
            return Cases.Promotion_AndrewWagner(depth);
        }
    }

    public static class CaseBoards
    {
        /// <summary>
        /// Promotion, castling, en passant.
        /// Andrew Wagner
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        public static (IBoard board, bool whiteStart) GetGoodPositions()
        {
            var fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out var whiteStart);
            return (board, whiteStart);
        }

        /// <summary>
        /// Andrew Wagner
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        public static (IBoard board, bool whiteStart) GetPromotion()
        {
            var fen = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out var whiteStart);
            return (board, whiteStart);
        }
    }

    /// <summary>
    /// For a particular position and search depth a perft value is the number of nodes or positions resulting from legal moves.
    /// So for example perft(depth = 1) from the initial board position is 20, representing the total number of legal moves available for white.
    /// https://www.chessprogramming.org/Perft_Results
    /// https://sites.google.com/site/numptychess/perft
    /// </summary>
    internal static class Cases
    {
        public static long StartPosition(int depth)
        {
            var board = BoardFactory.CreateDefault();

            var result = Perft.PerftRec(board, depth, true);
            TestContext.WriteLine($"{nameof(StartPosition)} with depth {depth}: node count {result}");
            return result;
        }


        public static long GoodPositions_AndrewWagner(int depth)
        {
            var (board, whiteStart) = CaseBoards.GetGoodPositions();

            var result = Perft.PerftRec(board, depth, whiteStart);
            return result;
        }

        /// <summary>
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        public static long Promotion_AndrewWagner(int depth)
        {
            var (board, whiteStart) = CaseBoards.GetPromotion();

            var result = Perft.PerftRec(board, depth, whiteStart);
            return result;
        }
    }

    internal class Perft
    {
        public static long PerftRec(IBoard newBoard, int depth, bool forWhite)
        {
            if (depth == 0) return 1;

            var moves = newBoard.MoveGenerator.MovesQuick(forWhite, true).ToList();
            if (!moves.Any())
            {
                return 0;
            }

            long nodes = 0;
            foreach (var move in moves)
            {
                var nextBoard = BoardFactory.CreateFromMove(newBoard, move);
                nodes += PerftRec(nextBoard, depth - 1, !forWhite);
            }

            return nodes;
        }
    }
}
