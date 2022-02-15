using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using vergiBlue.BoardModel;
using Logger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using TestContext = NUnit.Framework.TestContext;

namespace Benchmarking
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
        //[TestCase(5, ExpectedResult = 4865609u)]
        public long StartPosition(int depth)
        {
            var board = BoardFactory.CreateDefault();

            var result = PerftRec(board, depth, true);
            TestContext.WriteLine($"{nameof(StartPosition)} with depth {depth}: node count {result}");
            return result;
        }

        private static long PerftRec(IBoard newBoard, int depth, bool forWhite)
        {
            if (depth == 0) return 1;

            var moves = newBoard.Moves(forWhite, false, false);
            if (!moves.Any())
            {
                return 0;
            }

            long nodes = 0;
            foreach (var move in moves)
            {
                var nextBoard = BoardFactory.CreateFromMove(newBoard, move);
                if (nextBoard.DebugPostCheckMate) continue;
                
                nodes += PerftRec(nextBoard, depth - 1, !forWhite);
            }

            return nodes;
        }
    }
}
