using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace vergiBlue
{
    class Logic : LogicBase
    {
        private int _index = 2;
        public Move LatestOpponentMove { get; set; }

        public Logic(GameStartInformation startInformation)
        {

        }

        public override PlayerMove CreateMove()
        {
            // TODO testing
            var move = new PlayerMove()
            {
                Move = new Move()
                {
                    StartPosition = $"a{_index--}",
                    EndPosition = $"a{_index}",
                    PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                },
                Diagnostics = "Search depth = 0."
            };

            return move;
        }

        public override void ReceiveMove(Move opponentMove)
        {
            // TODO testing
            LatestOpponentMove = opponentMove;
        }

        public static bool IsOutside((int, int) target)
        {
            if (target.Item1 < 0 || target.Item1 > 7 || target.Item2 < 0 || target.Item2 > 7)
                return true;
            return false;
        }

        /// <summary>
        /// Returns (column, row) format as e.g. 'a1'
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string ToAlgebraic((int column, int row) position)
        {
            const int intToAlphabet = 65;
            var move = $"{((char) (position.column + intToAlphabet)).ToString().ToLower()}{position.row + 1}";
            return move;
        }
    }

    

    static class Diagnostics
    {
        private static int EvaluationCount = 0;
        private static List<string> Messages = new List<string>();
        private static readonly object messageLock = new object();

        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementEvalCount()
        {
            Interlocked.Increment(ref EvaluationCount);
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
    }
}
