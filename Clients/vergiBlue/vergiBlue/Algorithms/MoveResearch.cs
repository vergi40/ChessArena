using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using vergiBlue.BoardModel;

namespace vergiBlue.Algorithms
{
    public static class MoveResearch
    {
        public static SingleMove SelectBestMove(EvaluationResult evaluated, bool isMaximizing, bool prioritizeCaptures)
        {
            if (isMaximizing) return evaluated.MaxMove;
            return evaluated.MinMove;
        }
        
        private static double BestValue(bool isMaximizing)
        {
            if (isMaximizing) return 1000000;
            return -1000000;
        }

        private static double WorstValue(bool isMaximizing)
        {
            if (isMaximizing) return -1000000;
            return 1000000;
        }
    }
}
