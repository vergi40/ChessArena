using System.Linq;
using vergiBlue.BoardModel;

namespace PerftTests
{
    public class Perft
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
