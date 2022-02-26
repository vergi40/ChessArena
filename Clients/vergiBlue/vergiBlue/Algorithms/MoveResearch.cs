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

        /// <summary>
        /// Returns checkmate move or null.
        /// </summary>
        /// <returns></returns>
        public static SingleMove? ImmediateCheckMateAvailable(IList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Brute search checkmate
            foreach (var singleMove in moves)
            {
                var newBoard = BoardFactory.CreateFromMove(board, singleMove);
                if (newBoard.IsCheckMate(isMaximizing, false))
                {
                    singleMove.CheckMate = true;
                    return singleMove;
                }
            }

            return null;
        }

        public static IList<SingleMove> CheckMateInTwoTurns(IList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            IList<SingleMove> checkMates = new List<SingleMove>();
            foreach (var singleMove in moves)
            {
                var newBoard = BoardFactory.CreateFromMove(board, singleMove);
                if (CheckMate.InTwoTurns(newBoard, isMaximizing))
                {
                    // TODO collect all choices and choose best
                    // Game goes to draw loop otherwise
                    checkMates.Add(singleMove);
                }
            }

            return checkMates;
        }
    }
}
