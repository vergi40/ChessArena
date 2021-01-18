using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vergiBlue
{
    public class DiagnosticsData
    {
        public int EvaluationCount { get; set; } = 0;
        public int AlphaCutoffs { get; set; } = 0;
        public int BetaCutoffs { get; set; } = 0;
        public int CheckCount { get; set; } = 0;
        public int PriorityMovesFound { get; set; } = 0;
        public int TranspositionsFound { get; set; } = 0;


        public TimeSpan TimeElapsed = TimeSpan.Zero;

        public List<string> Messages = new List<string>();

        /// <summary>
        /// Call in end of each player turn
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = $"Board evaluations: {EvaluationCount}. ";
            if (CheckCount > 0) result += $"Check evaluations: {CheckCount}. ";
            if (AlphaCutoffs > 0) result += $"Alpha cutoffs: {AlphaCutoffs}. ";
            if (BetaCutoffs > 0) result += $"Beta cutoffs: {BetaCutoffs}. ";
            if (PriorityMovesFound > 0) result += $"Priority moves found: {PriorityMovesFound}. ";
            if (TranspositionsFound > 0) result += $"Transpositions used: {TranspositionsFound}. ";

            result += $"Time elapsed: {TimeElapsed.TotalMilliseconds:F0} ms. ";
            //result += $"Alphas: {AlphaCutoffs}, betas: {BetaCutoffs}. ";

            foreach (var message in Messages)
            {
                result += message;
            }

            return result;
        }

    }

    /// <summary>
    /// Collect all relevant data here on each player turn cycle.
    /// Call <see cref="StartMoveCalculations"/> in beginning of turn.
    /// Call <see cref="CollectAndClear"/> in end of turn to get the data.
    /// </summary>
    static class Diagnostics
    {
        private static DiagnosticsData _currentData = new DiagnosticsData();

        private static int _evaluationCount = 0;
        private static int _alphaCutoffs = 0;
        private static int _betaCutoffs = 0;
        private static int _checkCount = 0;
        private static int _priorityMovesFound = 0;
        private static int _transpositionsFound = 0;

        private static List<string> _messages = new List<string>();

        private static readonly object messageLock = new object();
        private static readonly Stopwatch _timeElapsed = new Stopwatch();

        /// <summary>
        /// Call in start of each player turn
        /// </summary>
        public static void StartMoveCalculations()
        {
            _currentData = new DiagnosticsData();
            _timeElapsed.Start();
        }

        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementEvalCount()
        {
            Interlocked.Increment(ref _evaluationCount);
        }
        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementAlpha()
        {
            Interlocked.Increment(ref _alphaCutoffs);
        }
        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementBeta()
        {
            Interlocked.Increment(ref _betaCutoffs);
        }

        public static void IncrementCheckCount()
        {
            Interlocked.Increment(ref _checkCount);
        }

        /// <summary>
        /// Found move that will be ordered to front of move-list to cause quick cutoffs.
        /// </summary>
        public static void IncrementPriorityMoves()
        {
            Interlocked.Increment(ref _priorityMovesFound);
        }

        /// <summary>
        /// Found move that will be ordered to front of move-list to cause quick cutoffs.
        /// </summary>
        public static void IncrementTranspositionsFound()
        {
            Interlocked.Increment(ref _transpositionsFound);
        }

        /// <summary>
        /// Thread-safe message operation. Slow
        /// </summary>
        public static void AddMessage(string message)
        {
            // TODO
            lock (messageLock)
            {
                _messages.Add(message);
            }
        }

        /// <summary>
        /// Call in end of each player turn
        /// </summary>
        /// <returns></returns>
        public static DiagnosticsData CollectAndClear()
        {
            lock (messageLock)
            {
                _currentData.EvaluationCount = _evaluationCount;
                _currentData.CheckCount = _checkCount;
                _currentData.AlphaCutoffs = _alphaCutoffs;
                _currentData.BetaCutoffs = _betaCutoffs;
                _currentData.PriorityMovesFound = _priorityMovesFound;
                _currentData.TranspositionsFound = _transpositionsFound;
                _currentData.Messages = _messages;

                _timeElapsed.Stop();
                _currentData.TimeElapsed = _timeElapsed.Elapsed;
                _timeElapsed.Reset();

                // Some overhead maybe?
                // Increment ref calls need to point to local property, so cant reference _currentData in those
                _evaluationCount = 0;
                _checkCount = 0;
                _alphaCutoffs = 0;
                _betaCutoffs = 0;
                _priorityMovesFound = 0;
                _transpositionsFound = 0;
                _messages = new List<string>();

                return _currentData;
            }
        }

    }
}
