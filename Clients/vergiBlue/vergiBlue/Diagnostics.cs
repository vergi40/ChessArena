using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vergiBlue
{
    static class Diagnostics
    {
        private static int EvaluationCount = 0;
        private static int AlphaCutoffs = 0;
        private static int BetaCutoffs = 0;
        private static int CheckCount = 0;

        private static List<string> Messages = new List<string>();
        private static readonly object messageLock = new object();
        private static readonly Stopwatch _timeElapsed = new Stopwatch();

        /// <summary>
        /// Call in start of each player turn
        /// </summary>
        public static void StartMoveCalculations()
        {
            _timeElapsed.Start();
        }

        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementEvalCount()
        {
            Interlocked.Increment(ref EvaluationCount);
        }
        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementAlpha()
        {
            Interlocked.Increment(ref AlphaCutoffs);
        }
        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementBeta()
        {
            Interlocked.Increment(ref BetaCutoffs);
        }

        public static void IncrementCheckCount()
        {
            Interlocked.Increment(ref CheckCount);
        }

        /// <summary>
        /// Thread-safe message operation. Slow
        /// </summary>
        public static void AddMessage(string message)
        {
            // TODO
            lock (messageLock)
            {
                Messages.Add(message);
            }
        }

        /// <summary>
        /// Call in end of each player turn
        /// </summary>
        /// <returns></returns>
        public static string CollectAndClear(out TimeSpan timeElapsed, out int evals, out int checkEvals)
        {
            lock (messageLock)
            {
                var result = $"Board evaluations: {EvaluationCount}. ";
                if (CheckCount > 0) result += $"Check evaluations: {CheckCount}. ";

                _timeElapsed.Stop();
                timeElapsed = _timeElapsed.Elapsed;
                evals = EvaluationCount;
                checkEvals = CheckCount;
                result += $"Time elapsed: {_timeElapsed.ElapsedMilliseconds} ms. ";
                //result += $"Alphas: {AlphaCutoffs}, betas: {BetaCutoffs}. ";
                _timeElapsed.Reset();

                foreach (var message in Messages)
                {
                    result += message;
                }

                EvaluationCount = 0;
                AlphaCutoffs = 0;
                BetaCutoffs = 0;
                CheckCount = 0;
                Messages = new List<string>();
                return result;
            }
        }

    }
}
