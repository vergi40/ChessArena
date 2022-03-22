using vergiBlue.Analytics;
using vergiBlue.BoardModel;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    internal static class Common
    {
        public static void AddIterativeDeepeningResultDiagnostics(int depthUsed, int totalMoveCount, int searchMoveCount, double evaluation, SingleMove? move = null, IBoard? board = null)
        {
            if (searchMoveCount < totalMoveCount)
            {
                Collector.AddCustomMessage($" Iterative deepening search depth was {depthUsed - 1} [partial {depthUsed}: ({searchMoveCount}/{totalMoveCount})].");
            }
            else
            {
                Collector.AddCustomMessage($" Iterative deepening search depth was {depthUsed} ({searchMoveCount}/{totalMoveCount}).");
            }
            Collector.AddCustomMessage($" Move evaluation: {evaluation}.");

            // DEBUG
            //if (move != null && board != null && board.Strategic.EndGameWeight > 0.50)
            //{
            //    var newBoard = BoardFactory.CreateFromMove(board, move);
            //    var isWhite = newBoard.ValueAtDefinitely(move.NewPos).IsWhite;
            //    Diagnostics.AddMessage(" EndGameKingToCornerEvaluation: " + Evaluator.EndGameKingToCornerEvaluation(newBoard, isWhite));
            //}
        }
    }
}
