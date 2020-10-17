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
        public bool IsWhite { get; }

        private int _index = 2;
        public Move LatestOpponentMove { get; set; }

        public Board Board { get; set; }

        /// <summary>
        /// Let test class handle initialization and board
        /// </summary>
        private readonly bool _testOverride;

        /// <summary>
        /// For tests
        /// </summary>
        public Logic(bool isWhite)
        {
            IsWhite = isWhite;
            _testOverride = true;
        }

        public Logic(GameStartInformation startInformation)
        {
            _testOverride = false;
        }

        public override PlayerMove CreateMove()
        {
            if (_testOverride)
            {
                var bestValue = WorstValue();
                SingleMove bestMove = null;
                var isMaximizing = IsWhite;

                // Evaluate each move and select best
                foreach (var piece in Board.PieceList.Where(p => !p.IsOpponent))
                {
                    foreach (var singleMove in piece.Moves())
                    {
                        var newBoard = new Board(Board, singleMove);
                        var value = newBoard.Evaluate();
                        if(isMaximizing)
                        {
                            if (value > bestValue)
                            {
                                bestValue = value;
                                bestMove = singleMove;
                            }
                        }
                        else
                        {
                            if (value < bestValue)
                            {
                                bestValue = value;
                                bestMove = singleMove;
                            }
                        }
                    }
                }

                if(bestMove == null) throw new ArgumentException($"Board didn't contain any possible move for player [isWhite={IsWhite}].");

                var move = new PlayerMove()
                {
                    Move = bestMove.ToInterfaceMove(),
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

            if (_testOverride)
            {
                var move = new SingleMove(opponentMove);

                // TODO intelligent analyzing what actually happened

                if (Board.ValueAt(move.NewPos) is Piece targetPiece)
                {
                    // Should be done elsewhere
                    if (!targetPiece.IsOpponent) move.Capture = true;
                    else throw new ArgumentException("Opponent captured own piece.");
                }

                Board.ExecuteMove(move);
            }
        }

        private double BestValue()
        {
            if (IsWhite) return 1000000;
            else return -1000000;
        }

        private double WorstValue()
        {
            if (IsWhite) return -1000000;
            else return 1000000;
        }

        public static bool IsOutside((int, int) target)
        {
            if (target.Item1 < 0 || target.Item1 > 7 || target.Item2 < 0 || target.Item2 > 7)
                return true;
            return false;
        }

        private const int _intToAlphabet = 65;

        /// <summary>
        /// Transforms (column, row) format to e.g. 'a1'
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string ToAlgebraic((int column, int row) position)
        {
            var move = $"{((char) (position.column + _intToAlphabet)).ToString().ToLower()}{position.row + 1}";
            return move;
        }

        /// <summary>
        /// Transforms algebraic format e.g. 'a1' to (column, row) format
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static (int column, int row) ToTuple(string position)
        {
            char columnChar = char.ToUpper(position[0]);
            var column = columnChar - _intToAlphabet;
            var row = int.Parse(position[1].ToString());
            return (column, row - 1);
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
