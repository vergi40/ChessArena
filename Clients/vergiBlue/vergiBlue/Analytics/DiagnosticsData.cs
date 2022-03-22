using System;
using System.Collections.Generic;

namespace vergiBlue.Analytics
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
}