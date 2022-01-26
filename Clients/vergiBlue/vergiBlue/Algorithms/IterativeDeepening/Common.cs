using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    internal static class Common
    {
        public static void AddIterativeDeepeningResultDiagnostics(int depthUsed, int totalMoveCount, int searchMoveCount, double evaluation, SingleMove? move = null, IBoard? board = null)
        {
            if (searchMoveCount < totalMoveCount)
            {
                Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed - 1} [partial {depthUsed}: ({searchMoveCount}/{totalMoveCount})].");
            }
            else
            {
                Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed} ({searchMoveCount}/{totalMoveCount}).");
            }
            Diagnostics.AddMessage($" Move evaluation: {evaluation}.");

            // DEBUG
            if (move != null && board != null && board.Strategic.EndGameWeight > 0.50)
            {
                var newBoard = BoardFactory.CreateFromMove(board, move);
                var isWhite = newBoard.ValueAtDefinitely(move.NewPos).IsWhite;
                Diagnostics.AddMessage(" EndGameKingToCornerEvaluation: " + newBoard.EndGameKingToCornerEvaluation(isWhite));
            }
        }
    }
}
