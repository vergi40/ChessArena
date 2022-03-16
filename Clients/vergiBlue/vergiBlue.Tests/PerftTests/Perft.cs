using System.Linq;
using NUnit.Framework;
using vergiBlue.BoardModel;

namespace PerftTests
{
    public class Perft
    {
        public static long PerftRec(IBoard newBoard, int depth, bool forWhite)
        {
            if (depth == 0) return 1;

            var moves = newBoard.GenerateMovesAndUpdateCache(forWhite).ToList();
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

        public static long Divide(IBoard board, int depth, bool forWhite)
        {
            long nodes = 0;
            foreach (var move in board.GenerateMovesAndUpdateCache(forWhite))
            {
                var newBoard = BoardFactory.CreateFromMove(board, move);
                var childNodes = PerftRec(newBoard, depth - 1, !forWhite);
                TestContext.WriteLine($"{move.ToCompactString()}: {childNodes}");

                nodes += childNodes;
            }

            return nodes;
        }
    }
}
