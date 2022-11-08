using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vergiBlue.Analytics
{
    /// <summary>
    /// Analytics data collector
    /// Call <see cref="StartMoveCalculationTimer"/> in beginning of turn.
    /// Call <see cref="CollectAndClear"/> in end of turn to get the data.
    /// </summary>
    public sealed class Collector
    {
        private static readonly Collector _instance = new Collector();
        public static Collector Instance => _instance;

        // We know how many items we want to insert into the ConcurrentDictionary.
        // So set the initial capacity to some prime number above that, to ensure that
        // the ConcurrentDictionary does not need to be resized while initializing it.
        // https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=net-6.0
        private const int DictInitialCapacity = 21;

        private ConcurrentDictionary<string, uint> OperationsDictionary { get; }
        private ConcurrentBag<string> CustomMessages { get; } = new();

        private EndTurnOutput Outputter { get; } = new EndTurnOutput();

        private Stopwatch MoveCalculationTimer { get; } = new Stopwatch();

        private MoveEvaluationData? EvalData { get; set; }

        private uint _evaluationCount = 0;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Collector(){}

        private Collector()
        {
            // The higher the concurrencyLevel, the higher the theoretical number of operations
            // that could be performed concurrently on the ConcurrentDictionary.  However, global
            // operations like resizing the dictionary take longer as the concurrencyLevel rises.
            // For the purposes of this example, we'll compromise at numCores * 2.
            var cores = Environment.ProcessorCount;
            OperationsDictionary = new(cores * 2, DictInitialCapacity);
        }

        public void StartMoveCalculationTimer()
        {
            MoveCalculationTimer.Start();
        }

        /// <summary>
        /// Atomic increment operation. Evaluation has dedicated backing since it's called often
        /// </summary>
        public static void IncrementEvaluationCount()
        {
            Interlocked.Increment(ref Instance._evaluationCount);
        }

        /// <summary>
        /// Collect descriptive operations that can be counted. 
        /// </summary>
        /// <param name="operationKey">Use <see cref="OperationsKeys"/></param>
        public static void IncreaseOperationCount(string operationKey) =>
            Instance.IncreaseOperationCountInternal(operationKey);

        /// <summary>
        /// Collect descriptive operations that can be counted. 
        /// </summary>
        /// <param name="operationKey">Use <see cref="OperationsKeys"/></param>
        public void IncreaseOperationCountInternal(string operationKey)
        {
            OperationsDictionary.AddOrUpdate(operationKey, 1, (_, v) => v + 1);
        }

        /// <summary>
        /// Collect descriptive messages
        /// </summary>
        public static void AddCustomMessage(string message) => Instance.AddCustomMessageInternal(message);

        public static void AddEvaluationData(MoveEvaluationData data) => Instance.EvalData = data;

        public MoveEvaluationData CollectEvalData()
        {
            var evalData = Instance.EvalData;
            if (evalData == null) return new MoveEvaluationData();
            
            Instance.EvalData = null;
            return evalData;
        }

        /// <summary>
        /// Collect descriptive messages
        /// </summary>
        private void AddCustomMessageInternal(string message)
        {
            // Helpers to keep full result message nice.
            message = message.Trim();
            if (!message.EndsWith('.')) message += ".";
            message += " ";

            CustomMessages.Add(message);
        }

        public void AddElapsedTime(TimeSpan span)
        {
            // TODO
        }

        public static uint CurrentEvalCount()
        {
            return _instance._evaluationCount;
        }

        public (string output, DiagnosticsData data) CollectAndClear(bool fullDiagnostics = false, bool lineBreaks = false)
        {
            // Custom: time elapsed
            var timeElapsed = MoveCalculationTimer.Elapsed;
            OperationsDictionary.AddOrUpdate(OperationsKeys.TimeElapsedMs, (uint)timeElapsed.TotalMilliseconds,
                (_,_) => (uint)timeElapsed.TotalMilliseconds);
            
            // Custom: eval count
            OperationsDictionary.AddOrUpdate(OperationsKeys.EvaluationDone, _evaluationCount, (_, v) => v + 1);


            var output = CollectOutput(fullDiagnostics, lineBreaks);
            var data = CollectData(timeElapsed);
            

            // Clear
            OperationsDictionary.Clear();
            CustomMessages.Clear();
            MoveCalculationTimer.Reset();
            _evaluationCount = 0;
            return (output, data);
        }

        private string CollectOutput(bool fullDiagnostics = false, bool lineBreaks = false)
        {
            var builder = new StringBuilder();
            foreach (var (key, value) in OperationsDictionary)
            {
                if (value == 0) continue;

                if (!fullDiagnostics)
                {
                    if (key.Equals(OperationsKeys.Alpha)) continue;
                    if (key.Equals(OperationsKeys.Beta)) continue;
                }

                builder.Append(Outputter.Format(key, value));
                if (lineBreaks) builder.Append(Environment.NewLine);
            }

            if (lineBreaks) builder.Append(Environment.NewLine);

            foreach (var message in CustomMessages)
            {
                builder.Append(message);
                if (lineBreaks) builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        private DiagnosticsData CollectData(TimeSpan timeElapsed)
        {
            var data = new DiagnosticsData();
            if (OperationsDictionary.TryGetValue(OperationsKeys.CheckEvaluationDone, out var checkEvals))
            {
                data.CheckCount = (int)checkEvals;
            }
            if (OperationsDictionary.TryGetValue(OperationsKeys.EvaluationDone, out var totalEvals))
            {
                data.EvaluationCount = totalEvals;
            }

            data.TimeElapsed = timeElapsed;
            return data;
        }
    }
}
