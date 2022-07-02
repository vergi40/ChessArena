using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    /// <summary>
    /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ... 
    /// </summary>
    public class IDBasic : IAlgorithm
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<IDBasic>();

        /// <summary>
        /// Overridden to write in UCI console, if UCI search
        /// </summary>
        private Action<string> _writeOutputAction { get; set; } = delegate (string s)
        {
            _logger.LogInformation(s);
        };

        public SingleMove CalculateBestMove(BoardContext context, SearchParameters? searchParameters = null)
        {
            if (searchParameters != null)
            {
                // UCI search
                _writeOutputAction = searchParameters.WriteToOutputAction;

                var (maxDepth, timeLimit) = Common.DefineDepthAndTime(context, searchParameters);

                return IterativeDeepeningBasic(context.ValidMoves, maxDepth, context.CurrentBoard,
                    context.IsWhiteTurn, timeLimit, searchParameters.StopSearchToken);
            }
            else
            {
                // Desktop / test search
                return IterativeDeepeningBasic(context.ValidMoves, context.NominalSearchDepth,
                    context.CurrentBoard,
                    context.IsWhiteTurn, context.MaxTimeMs, CancellationToken.None);
            }
        }

        /// <summary>
        /// Iterative deepening sub-method.
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ...
        /// </summary>
        private SingleMove IterativeDeepeningBasic(IReadOnlyList<SingleMove> allMoves, int searchDepth,
            IBoard board, bool isMaximizing, int timeLimitInMs, CancellationToken stopSearchToken)
        {
            // Only use deeper depth stopped search results, if this percent of moves were evaluated 
            var minimumSearchPercentForHigherDepthUse = 0.49;
            var timeUp = false;
            int depthUsed = 0;

            // Results for depth n. Updated in each iteration
            var searchResults = new List<(double weight, SingleMove move)>();
            var allMovesSorted = new List<SingleMove>(allMoves);

            // Mostly for debug
            var previousDepthResults = new List<(double weight, SingleMove move)>();
            var timer = SearchTimer.Start(timeLimitInMs);
            var stopControl = new SearchStopControl(timer, stopSearchToken);

            if (searchDepth == 0)
            {
                throw new ArgumentException("Can't search with search depth limited to 0");
            }

            // In case depth = 1
            var initialSearchDepth = Math.Min(2, searchDepth);

            // Initial depth 2
            for (int i = initialSearchDepth; i <= searchDepth; i++)
            {
                var alpha = MiniMaxGeneral.DefaultAlpha;
                var beta = MiniMaxGeneral.DefaultBeta;
                depthUsed = i;
                searchResults.Clear();

                foreach (var move in allMovesSorted)
                {
                    var newBoard = BoardFactory.CreateFromMove(board, move);
                    var evaluation = MiniMax.ToDepth(newBoard, i, alpha, beta, !isMaximizing, stopControl);

                    if (stopControl.StopSearch())
                    {
                        // Don't use results if search was stopped
                        timeUp = true;
                        break;
                    }

                    searchResults.Add((evaluation, move));

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
                searchResults = MoveOrdering.SortWeightedMovesWithSort(searchResults, isMaximizing).ToList();

                if (timeUp) break;
                // info depth 4 score cp -30 time 55 nodes 1292 nps 25606 pv d7d5 e2e3 e7e6 g1f3
                var infoPrint =
                    $"info depth {i} score cp {searchResults.First().weight} " +
                    $"time {timer.CurrentElapsed()} nodes {Collector.CurrentEvalCount()}";
                _writeOutputAction(infoPrint);

                allMovesSorted = searchResults.Select(item => item.Item2).ToList();
                previousDepthResults = searchResults.ToList();
            }

            // searchResults is either partial or full
            // If too small percent was searched for new depth, use previous results
            // E.g. out of 8 possible moves, only 2 were searched
            List<(double weight, SingleMove move)> finalResults;
            if (searchResults.Count / (double)allMoves.Count < minimumSearchPercentForHigherDepthUse)
            {
                // Plain sort by value here - not trying to optimize minimax alpha-betas anymore
                finalResults = MoveOrdering.SortWeightedMovesWithSort(previousDepthResults, isMaximizing).ToList();
            }
            else
            {
                // Plain sort by value here - not trying to optimize minimax alpha-betas anymore
                finalResults = MoveOrdering.SortWeightedMovesWithSort(searchResults, isMaximizing).ToList();
            }

            var bestMove = finalResults.First();
            Common.AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, searchResults.Count, bestMove.weight, bestMove.move, board);
            Common.DebugPrintWeighedMoves(finalResults);
            return bestMove.move;
        }

    }
}
