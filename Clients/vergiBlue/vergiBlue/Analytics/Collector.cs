using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Analytics
{
    public sealed class Collector
    {
        private static readonly Collector instance = new Collector();
        public static Collector Instance => instance;

        // We know how many items we want to insert into the ConcurrentDictionary.
        // So set the initial capacity to some prime number above that, to ensure that
        // the ConcurrentDictionary does not need to be resized while initializing it.
        // https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=net-6.0
        private const int DictInitialCapacity = 21;

        private ConcurrentDictionary<string, uint> OperationsDictionary { get; }
        private ConcurrentBag<string> CustomMessages { get; } = new();

        private EndTurnOutput Outputter { get; } = new EndTurnOutput();

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

        /// <summary>
        /// Collect descriptive operations that can be counted. 
        /// </summary>
        /// <param name="operationKey">Use <see cref="OperationsKeys"/></param>
        public void IncreaseOperationCount(string operationKey)
        {
            OperationsDictionary.AddOrUpdate(operationKey, 1, (k, v) => v + 1);
        }

        /// <summary>
        /// Collect descriptive messages
        /// </summary>
        /// <param name="message"></param>
        public void AddCustomMessage(string message)
        {
            // Helpers to keep full result message nice.
            message = message.Trim();
            if (!message.EndsWith('.')) message += ".";
            message += " ";

            CustomMessages.Add(message);
        }

        public string CollectAndClear(bool fullDiagnostics = false, bool lineBreaks = false)
        {
            var builder = new StringBuilder();
            foreach (var (key, value) in OperationsDictionary)
            {
                if (!fullDiagnostics)
                {
                    if (key.Equals(OperationsKeys.Alpha)) continue;
                    if (key.Equals(OperationsKeys.Beta)) continue;
                }

                builder.Append(Outputter.Format(key, value));

                if (lineBreaks)
                {
                    builder.Append(Environment.NewLine);
                }
            }

            if (lineBreaks)
            {
                builder.Append(Environment.NewLine);
            }

            foreach (var message in CustomMessages)
            {
                builder.Append(message);
                if (lineBreaks)
                {
                    builder.Append(Environment.NewLine);
                }
            }

            return builder.ToString();
        }
    }
}
