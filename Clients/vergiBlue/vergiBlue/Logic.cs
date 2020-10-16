using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace vergiBlue
{
    public class Logic : LogicBase
    {
        private int _index = 2;
        public Move LatestOpponentMove { get; set; }

        public Board Board { get; set; }

        /// <summary>
        /// Let test class handle initialization and board
        /// </summary>
        private bool _testOverride = false;

        /// <summary>
        /// For tests
        /// </summary>
        public Logic() {  _testOverride = true;}

        public Logic(GameStartInformation startInformation)
        {
            
        }

        public override PlayerMove CreateMove()
        {
            if (_testOverride)
            {
                var bestValue = -1000000.0;
                SingleMove bestMove = null;

                // Evaluate each move and select best
                foreach (var piece in Board.PieceList.Where(p => !p.IsOpponent))
                {
                    foreach (var singleMove in piece.Moves())
                    {
                        var newBoard = new Board(Board, singleMove);
                        var value = newBoard.Evaluate();
                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestMove = singleMove;
                        }
                    }
                }

                var move = new PlayerMove()
                {
                    Move = new Move()
                    {
                        StartPosition = ToAlgebraic(bestMove.PrevPos),
                        EndPosition = ToAlgebraic(bestMove.NewPos),
                        PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                    },
                    Diagnostics = Diagnostics.CollectAndClear()
                };
                return move;
            }
            else
            {
                // Dummy moves for testserver
                var move = new PlayerMove()
                {
                    Move = new Move()
                    {
                        StartPosition = $"a{_index--}",
                        EndPosition = $"a{_index}",
                        PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                    },
                    Diagnostics = Diagnostics.CollectAndClear()
                };

                return move;
            }
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

        public static string CollectAndClear()
        {
            lock(messageLock)
            {
                var result = $"Board evaluations: {EvaluationCount}";
                foreach (var message in Messages)
                {
                    result += message;
                }

                EvaluationCount = 0;
                Messages = new List<string>();
                return result;
            }
        }
    }
}
