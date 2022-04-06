using System;
using System.Collections.Generic;
using System.Linq;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.PreMove
{
    class DepthController
    {
        private const int HistoryCount = 5;

        /// <summary>
        /// (depth, time lower bound, correction, move count)
        /// </summary>
        private List<(double, int, double, int)> _previous { get; } = new();

        public DepthController()
        {

        }

        /// <summary>
        /// Returns depth as double. Eg. 5.5 means estimate for time limit is between 5 and 6
        /// </summary>
        public double GetDepthEstimate(IReadOnlyList<SingleMove> allMoves, IBoard board, TurnStartInfo turnInfo, int maxDepth, int previousDepth)
        {
            CleanHistory();
            var timeLimit = turnInfo.settings.TimeLimitInMs;
            var previousTimeLimitExceeded = PreviousTurnExceededTimeLimit(turnInfo.previousMoveData, timeLimit);

            var previousEstimate = 0;
            var resultDepth = 0.0;
            var useMax = true;
            for (int i = 1; i <= maxDepth; i++)
            {
                var estimation = AssessTimeForMiniMaxDepth(i, allMoves, board, turnInfo.previousMoveData);

                // Use 50% tolerance for target time
                if (estimation > timeLimit)
                {
                    resultDepth = i - 1;

                    // e.g. previous 4000 (5), next 8000 (6), target 5000 (5.25) -> 
                    // 
                    var a = timeLimit - previousEstimate;
                    var b = estimation - timeLimit;

                    resultDepth += a / (double)(a + b);
                    useMax = false;
                }
                else
                {
                    previousEstimate = estimation;
                }
            }

            if (useMax) resultDepth = maxDepth;

            // Compare to previous with feedback
            var correction = GetCorrection(turnInfo.previousMoveData);

            _previous.Add((resultDepth, previousEstimate, correction, allMoves.Count));

            if (!previousTimeLimitExceeded)
            {
                // as previous round was ok, previous depth should be bare minimum
                return Math.Max(resultDepth + correction, previousDepth);
            }
            return resultDepth + correction;
        }



        /// <summary>
        /// Compare to previous findings and decrease or increase depth. Cap to -1, 1
        /// </summary>
        private double GetCorrection(DiagnosticsData previousData)
        {
            if (!_previous.Any()) return 0.0;

            // e.g. estimate 3000, elapsed 1000 -> increase
            var elapsed = previousData.TimeElapsed.TotalMilliseconds;
            var previousTimeEstimate = _previous.Last().Item2;

            var delta = (previousTimeEstimate - elapsed) * 0.001;
            delta = Math.Max(delta, -1.0);
            delta = Math.Min(delta, 1.0);
            return delta;
        }

        private int AssessTimeForMiniMaxDepth(int depth, IReadOnlyList<SingleMove> availableMoves, IBoard board,
            DiagnosticsData previousData)
        {
            var previousTime = previousData.TimeElapsed.TotalMilliseconds;
            var prevousEvalCount = previousData.EvaluationCount + previousData.CheckCount;

            // First calculation or testing
            previousTime = Math.Max(1000, previousTime);
            prevousEvalCount = Math.Max(100_000, prevousEvalCount);

            // Need a equation to model evalcount <-> time
            // movecount ^ depth = evalcount
            // evalcount correlates to time

            // Estimated evaluation speed. moves per millisecond
            // Speed = count / time
            var previousEvalSpeed = prevousEvalCount / previousTime;

            var evalCount = Math.Pow(Math.Max(availableMoves.Count, 6), depth);
            // lets say 20 ^ 5 = 3 200 000

            // Estimated total time with previous speed
            // Time = count /  speed
            var timeEstimate = evalCount / previousEvalSpeed;

            // Increase estimate proportionally depending of piece count
            // All pieces -> use as is
            // 1 piece -> 1/16 of the time
            var factor = board.GetPowerPiecePercent();
            //var factor = 0.5 + (double)powerPieces / 32;

            return (int)(timeEstimate * factor);
        }

        private bool PreviousTurnExceededTimeLimit(DiagnosticsData previousData, int currentTimeLimit)
        {
            var tolerance = 0.10;
            return previousData.TimeElapsed.TotalMilliseconds > currentTimeLimit * (1 - tolerance);
        }

        private void CleanHistory()
        {
            while (_previous.Count > HistoryCount)
            {
                _previous.RemoveAt(0);
            }
        }
    }
}
