using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.Basic
{
    internal class MiniMaxWithTranspositions : IAlgorithm
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
                var transposition = board.Shared.Transpositions.GetTranspositionForMove(board, move);
                if (transposition != null && transposition.Depth >= searchDepth)
                {
                    // Saved some time
                    // TODO extra parameters to evaluationresult if this was lower or upper bound
                    result.Add(transposition.Evaluation, move);
                }
                else
                {
                    // Board evaluation at current depth
                    var newBoard = BoardFactory.CreateFromMove(board, move);
                    var value = MiniMax.ToDepthWithTranspositions(newBoard, searchDepth, alpha, beta,
                        !isMaximizing);
                    result.Add(value, move);

                    // Add new transposition table
                    newBoard.Shared.Transpositions.Add(newBoard.BoardHash, searchDepth, value, NodeType.Exact, newBoard.Shared.GameTurnCount);

                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, value);
                    }
                    else
                    {
                        beta = Math.Min(beta, value);
                    }
                }
            }

            return result;
        }
    }
}
