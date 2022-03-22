using System;
using System.Collections.Generic;

namespace vergiBlue.Analytics
{
    /// <summary>
    /// Data from previous turn that can be utilized to guide next turn analysis
    /// </summary>
    public class DiagnosticsData
    {
        public uint EvaluationCount { get; set; } = 0;
        public int CheckCount { get; set; } = 0;


        public TimeSpan TimeElapsed { get; set; } = TimeSpan.Zero;
    }
}