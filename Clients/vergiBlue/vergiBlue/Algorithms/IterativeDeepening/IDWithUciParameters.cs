using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    // TODO prototype class is messy
    internal class IDWithUciParameters
    {
        // TODO either add all as privates or use all as arguments
        private Action<string> _writeOutputAction { get; set; } = delegate(string s) {  };

        public SingleMove CalculateBestMove(BoardContext context, SearchParameters parameters)
        {
            //
            _writeOutputAction = parameters.WriteToOutputAction;

            var (maxDepth, timeLimit) = DefineDepthAndTime(context, parameters);

            return IterativeDeepeningUciPrototype(context.ValidMoves, maxDepth, context.CurrentBoard,
                context.IsWhiteTurn, timeLimit, parameters.StopSearchToken);
        }


        private (int maxDepth, int timeLimit) DefineDepthAndTime(BoardContext context, SearchParameters parameters)
        {
            var uciParameters = parameters.UciParameters;
            var limits = uciParameters.SearchLimits;

            // Infinite -> use really large depth. Not set -> use some default
            var infinite = uciParameters.Infinite;
            int maxDepth = 10;
            if (infinite) maxDepth = 100;
            else if (limits.Depth != 0) maxDepth = limits.Depth;

            // Infinite -> use really large time limit. If not set -> use settings time limit
            var timeLimit = context.MaxTimeMs;
            if (infinite) timeLimit = int.MaxValue;
            else if (limits.Time != 0) timeLimit = limits.Time;

            return (maxDepth, timeLimit);
        }


        private SingleMove IterativeDeepeningUciPrototype(IReadOnlyList<SingleMove> allMoves, int searchDepth, IBoard board, bool isMaximizing, int timeLimitInMs, CancellationToken stopSearchToken)
        {
            // Only use deeped depth stopped search results, if this percent of moves were evaluated 
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
                    var evaluation = MiniMax.ToDepthUciPrototype(newBoard, i, alpha, beta, !isMaximizing, stopControl);

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

            // If too small percent was searched for new depth, use prevous results
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
