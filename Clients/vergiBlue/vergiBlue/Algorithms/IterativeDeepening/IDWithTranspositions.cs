using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using log4net;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    /// <summary>
    /// De facto search algorithm containing all available features.
    /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ... 
    /// </summary>
    internal class IDWithTranspositions : IAlgorithm
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IDWithTranspositions));

        /// <summary>
        /// Overridden to write in UCI console, if UCI search
        /// </summary>
        private Action<string> _writeOutputAction { get; set; } = delegate(string s)
        {
            _logger.Info(s);
        };

        public SingleMove CalculateBestMove(BoardContext context, SearchParameters? searchParameters = null)
        {
            if (searchParameters != null)
            {
                // UCI search
                _writeOutputAction = searchParameters.WriteToOutputAction;

                var (maxDepth, timeLimit) = Common.DefineDepthAndTime(context, searchParameters);

                return IterativeDeepeningWithTT(context.ValidMoves, maxDepth, context.CurrentBoard,
                    context.IsWhiteTurn, timeLimit, searchParameters.StopSearchToken);
            }
            else
            {
                // Desktop / test search
                return IterativeDeepeningWithTT(context.ValidMoves, context.NominalSearchDepth,
                    context.CurrentBoard,
                    context.IsWhiteTurn, context.MaxTimeMs, CancellationToken.None);
            }
        }
        
        /// <summary>
        /// Iterative deepening sub-method.
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ...
        /// </summary>
        private SingleMove IterativeDeepeningWithTT(IReadOnlyList<SingleMove> allMoves, int searchDepth,
            IBoard board, bool isMaximizing, int timeLimitInMs, CancellationToken stopSearchToken)
        {
            // Only use deeper depth stopped search results, if this percent of moves were evaluated 
            var minimumSearchPercentForHigherDepthUse = 0.49;
            var timeUp = false;
            int depthUsed = 0;

            var midResult = new List<(double weight, SingleMove move)>();
            var currentIterationMoves = new List<SingleMove>(allMoves);
            (double eval, SingleMove move) previousIterationBest = new(0.0, new SingleMove((-1, -1), (-1, -1)));

            // Mostly for debug
            var previousIterationAll = new List<(double weight, SingleMove move)>();
            var timer = SearchTimer.Start(timeLimitInMs);
            var stopControl = new SearchStopControl(timer, stopSearchToken);

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
                    var evaluation = MiniMax.ToDepthWithTT(newBoard, i, alpha, beta, !isMaximizing, stopControl);

                    if (stopControl.StopSearch())
                    {
                        // Don't use results if search was stopped
                        timeUp = true;
                        break;
                    }

                    midResult.Add((evaluation, move));

                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, evaluation);
                        if (alpha >= beta) { /* */ }
                    }
                    else
                    {
                        beta = Math.Min(beta, evaluation);
                        if (beta <= alpha) { /* */ }
                    }
                }

                // Full search finished for depth
                midResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();

                if (timeUp) break;
                var pvString = Common.GetPrincipalVariationAsString(searchDepth, board, midResult.First().move, isMaximizing);
                // info depth 4 score cp -30 time 55 nodes 1292 nps 25606 pv d7d5 e2e3 e7e6 g1f3
                var infoPrint =
                    $"info depth {i} score cp {midResult.First().weight} " +
                    $"time {timer.CurrentElapsed()} nodes {Collector.CurrentEvalCount()} " +
                    $"pv { pvString}";
                _writeOutputAction(infoPrint);

                currentIterationMoves = midResult.Select(item => item.Item2).ToList();
                previousIterationBest = midResult.First();
                previousIterationAll = midResult.ToList();
            }

            // midResult is either partial or full. Just sort and return first.

            // If too small percent was searched for new depth, use previous results
            // E.g. out of 8 possible moves, only 2 were searched
            if (midResult.Count / (double)allMoves.Count < minimumSearchPercentForHigherDepthUse)
            {
                var result = previousIterationBest;
                Common.AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, result.eval, result.move, board);
                Common.AddPVDiagnostics(depthUsed, board, result.move, isMaximizing);
                Common.DebugPrintWeighedMoves(previousIterationAll);
                return result.move;
            }

            var finalResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
            Common.AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, finalResult.First().weight, finalResult.First().move, board);
            Common.AddPVDiagnostics(depthUsed, board, finalResult.First().move, isMaximizing);
            Common.DebugPrintWeighedMoves(finalResult);
            return finalResult.First().move;
        }
    }
}
