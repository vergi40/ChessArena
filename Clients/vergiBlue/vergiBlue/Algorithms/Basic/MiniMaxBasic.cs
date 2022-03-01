using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.Basic
{
    public class MiniMaxBasic : IAlgorithm
    {
        public SingleMove CalculateBestMove(BoardContext context)
        {
            var evaluated = GetMoveScoreList(context.ValidMoves, context.NominalSearchDepth, context.CurrentBoard, context.IsWhiteTurn);

            return MoveResearch.SelectBestMove(evaluated, context.IsWhiteTurn, true);
        }



        private EvaluationResult GetMoveScoreList(IReadOnlyList<SingleMove> moves,
            int searchDepth, IBoard board, bool isMaximizing)
        {
            var result = new EvaluationResult();
            var alpha = MiniMaxGeneral.DefaultAlpha;
            var beta = MiniMaxGeneral.DefaultBeta;

            foreach (var move in moves)
            {
                // Board evaluation at current depth
                var newBoard = BoardFactory.CreateFromMove(board, move);
                var value = MiniMax.ToDepth(newBoard, searchDepth, alpha, beta, !isMaximizing);
                result.Add(value, move);

                if (isMaximizing)
                {
                    alpha = Math.Max(alpha, value);
                }
                else
                {
                    beta = Math.Min(beta, value);
                }
            }

            return result;
        }
    }
}
