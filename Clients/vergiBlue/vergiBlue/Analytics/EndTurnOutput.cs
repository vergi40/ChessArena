using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Analytics
{
    internal class EndTurnOutput
    {
        public string Format(string key, uint value)
        {
            if (Predefined.TryGetValue(key, out var formatted))
            {
                // E.g. "Board evaluations: 500. "
                return $"{formatted}: {value}. ";
            }

            return $"{key} count: {value}. ";
        }


        private static Dictionary<string, string> Predefined { get; } = new()
        {
            { OperationsKeys.EvaluationDone, "Board evaluations: " },
            { OperationsKeys.CheckEvaluationDone, "Check evaluations: " },
            { OperationsKeys.Alpha, "Alpha cutoffs: " },
            { OperationsKeys.Beta, "Beta cutoffs: " },
            { OperationsKeys.PriorityMoveFound, "Priority moves found: " },
            { OperationsKeys.TranspositionUsed, "Transpositions used: " },
            { OperationsKeys.CacheCheckUtilized, "Chech cache utilized count: " },
            { OperationsKeys.TimeElapsed, "Time elapsed: " },
        };
    }
}
