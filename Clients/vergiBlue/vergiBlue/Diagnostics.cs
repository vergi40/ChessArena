using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vergiBlue.Analytics;

namespace vergiBlue
{
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
            // Helpers to keep full result message nice.
            message = message.Trim();
            if (!message.EndsWith('.')) message += ".";
            message = message + " ";

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
        public static DiagnosticsData CollectAndClear(bool fullDiagnostics = false)
        {
            lock (messageLock)
            {
                _currentData.EvaluationCount = _evaluationCount;
                _currentData.CheckCount = _checkCount;
                
                if(fullDiagnostics)
                {
                    _currentData.AlphaCutoffs = _alphaCutoffs;
                    _currentData.BetaCutoffs = _betaCutoffs;
                }
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
