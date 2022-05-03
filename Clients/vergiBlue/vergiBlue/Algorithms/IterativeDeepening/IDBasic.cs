using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
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
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IDWithTranspositions));

        /// <summary>
        /// Overridden to write in UCI console, if UCI search
        /// </summary>
        private Action<string> _writeOutputAction { get; set; } = delegate (string s)
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
        private SingleMove IterativeDeepeningBasic(IReadOnlyList<SingleMove> allMoves, int searchDepth, IBoard board,
            bool isMaximizing, int timeLimitInMs, CancellationToken stopSearchToken)
        {
            // 
            var minimumSearchPercentForHigherDepthUse = 1 / (double)3;
            var timeUp = false;
            int depthUsed = 0;

            var midResult = new List<(double weight, SingleMove move)>();
            var currentIterationMoves = new List<SingleMove>(allMoves);
            (double eval, SingleMove move) previousIterationBest = new(0.0, new SingleMove((-1, -1), (-1, -1)));
            
            // Mostly for debug
            var previousIterationAll = new List<(double weight, SingleMove move)>();
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
                midResult.Clear();

                foreach (var move in currentIterationMoves)
                {
                    var newBoard = BoardFactory.CreateFromMove(board, move);
                    var evaluation = MiniMax.ToDepth(newBoard, i, alpha, beta, !isMaximizing, stopControl);
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

                if (timeUp) break;
                // info depth 4 score cp -30 time 55 nodes 1292 nps 25606 pv d7d5 e2e3 e7e6 g1f3
                var infoPrint =
                    $"info depth {i} score cp {midResult.First().weight} " +
                    $"time {timer.CurrentElapsed()} nodes {Collector.CurrentEvalCount()} ";
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
                Common.DebugPrintWeighedMoves(previousIterationAll);
                return result.move;
            }

            var finalResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
            Common.AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, finalResult.First().weight, finalResult.First().move, board);
            Common.DebugPrintWeighedMoves(finalResult);
            return finalResult.First().move;
        }

    }
}
