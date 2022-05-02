using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    /// <summary>
    /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ... 
    /// </summary>
    public class IDBasic : IAlgorithm
    {
        public SingleMove CalculateBestMove(BoardContext context)
        {
            //
            return IterativeDeepeningBasic(context.ValidMoves, context.NominalSearchDepth, context.CurrentBoard,
                context.IsWhiteTurn, context.MaxTimeMs);
        }

        /// <summary>
        /// Iterative deepening sub-method.
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ... 
        /// </summary>
        private SingleMove IterativeDeepeningBasic(IReadOnlyList<SingleMove> allMoves, int searchDepth, IBoard board, bool isMaximizing, int timeLimitInMs = 5000)
        {
            // 
            var minimumSearchPercentForHigherDepthUse = 1 / (double)3;
            var timeUp = false;
            int depthUsed = 0;

            var midResult = new List<(double weight, SingleMove move)>();
            var currentIterationMoves = new List<SingleMove>(allMoves);
            (double eval, SingleMove move) previousIterationBest = new(0.0, new SingleMove((-1, -1), (-1, -1)));
            var timer = SearchTimer.Start(timeLimitInMs);

            // Why this works for black start, but not white?
            //var alpha = -1000000.0;
            //var beta = 1000000.0;

            // Initial depth 2
            for (int i = 2; i <= searchDepth; i++)
            {
                var alpha = MiniMaxGeneral.DefaultAlpha;
                var beta = MiniMaxGeneral.DefaultBeta;
                depthUsed = i;
                midResult.Clear();

                foreach (var move in currentIterationMoves)
                {
                    var newBoard = BoardFactory.CreateFromMove(board, move);
                    var evaluation = MiniMax.ToDepth(newBoard, i, alpha, beta, !isMaximizing, timer);
                    midResult.Add((evaluation, move));

                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, evaluation);
                    }
                    else
                    {
                        beta = Math.Min(beta, evaluation);
                    }

                    if (timer.Exceeded())
                    {
                        timeUp = true;
                        break;
                    }
                }


                // Full search finished for depth
                midResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();

                // Found checkmate
                //if (isMaximizing && midResult.First().weight > PieceBaseStrength.CheckMateThreshold
                //    || !isMaximizing && midResult.First().weight < -PieceBaseStrength.CheckMateThreshold)
                //{
                //    // TODO This might result in stupid movements, if opponent doesn't do the exact move AI thinks is best for it

                //    Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed}. Check mate found.");
                //    Diagnostics.AddMessage($" Move evaluation: {midResult.First().weight}.");
                //    return midResult.First().move;
                //}

                if (timeUp) break;

                currentIterationMoves = midResult.Select(item => item.Item2).ToList();
                previousIterationBest = midResult.First();
            }

            // midResult is either partial or full. Just sort and return first.

            // If too small percent was searched for new depth, use prevous results
            // E.g. out of 8 possible moves, only 2 were searched
            if (midResult.Count / (double)allMoves.Count < minimumSearchPercentForHigherDepthUse)
            {
                var result = previousIterationBest;
                Common.AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, result.eval, result.move, board);
                Common.DebugPrintWeighedMoves(midResult);
                return result.move;
            }

            var finalResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
            Common.AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, finalResult.First().weight, finalResult.First().move, board);
            Common.DebugPrintWeighedMoves(finalResult);
            return finalResult.First().move;
        }

    }
}
