using System.Linq;
using NUnit.Framework;
using vergiBlue.BoardModel;

namespace PerftTests
{
    /// <summary>
    /// For a particular position and search depth a perft value is the number of nodes or positions resulting from legal moves.
    /// So for example perft(depth = 1) from the initial board position is 20, representing the total number of legal moves available for white.
    /// https://www.chessprogramming.org/Perft_Results
    /// https://sites.google.com/site/numptychess/perft
    /// </summary>
    [TestFixture]
    public class Perft
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

        public static long PerftRec(IBoard newBoard, int depth, bool forWhite)
        {
            if (depth == 0) return 1;

            var moves = newBoard.Moves(forWhite, false, true);
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
