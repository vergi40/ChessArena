using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using vergiBlue.Pieces;

namespace vergiBlue
{
    public enum GamePhase
    {
        /// <summary>
        /// Openings and initial
        /// </summary>
        Start,
        Middle,
        EndGame
    }

    public class Logic : LogicBase
    {
        // Game strategic variables

        public GamePhase Phase { get; set; } = GamePhase.Start;
        public int SearchDepth { get; set; } = 4;

        /// <summary>
        /// Total game turn count
        /// </summary>
        public int TurnCount { get; set; } = 0;

        /// <summary>
        /// Starts from 0
        /// </summary>
        public int PlayerTurnCount
        {
            get
            {
                if (IsPlayerWhite) return TurnCount / 2;
                return (TurnCount - 1) / 2;
            }
        }

        public static TimeSpan LastTurnElapsed { get; set; } = TimeSpan.Zero;



        private int _connectionTestIndex = 2;
        public Move LatestOpponentMove { get; set; }

        public Board Board { get; set; } = new Board();

        /// <summary>
        /// Use dummy moves to test connection with server
        /// </summary>
        private readonly bool _connectionTestOverride;

        /// <summary>
        /// For tests. Test environment will handle board initialization
        /// </summary>
        public Logic(bool isPlayerWhite) : base(isPlayerWhite)
        {
            _connectionTestOverride = false;
        }

        public Logic(GameStartInformation startInformation, bool connectionTesting) : base(startInformation.WhitePlayer)
        {
            _connectionTestOverride = connectionTesting;
            if (!connectionTesting) Board.InitializeEmptyBoard();
            if (!IsPlayerWhite) ReceiveMove(startInformation.OpponentMove);
        }

        public override PlayerMove CreateMove()
        {

            if (_connectionTestOverride)
            {
                // Dummy moves for connection testing
                var move = new PlayerMove()
                {
                    Move = new Move()
                    {
                        StartPosition = $"a{_connectionTestIndex--}",
                        EndPosition = $"a{_connectionTestIndex}",
                        PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                    },
                    Diagnostics = Diagnostics.CollectAndClear()
                };

                return move;
            }
            else
            {
                Diagnostics.StartMoveCalculations();
                var bestValue = WorstValue();
                SingleMove bestMove = null;
                var isMaximizing = IsPlayerWhite;

                var allMoves = Board.Moves(isMaximizing).ToList();
                AnalyzeGamePhase(allMoves.Count);

                foreach (var singleMove in allMoves)
                {
                    var newBoard = new Board(Board, singleMove);
                    var value = MiniMax(newBoard, 3, -100000, 100000, !isMaximizing);
                    if (isMaximizing)
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

                if(bestMove == null) throw new ArgumentException($"Board didn't contain any possible move for player [isWhite={IsPlayerWhite}].");

                // Update local
                Board.ExecuteMove(bestMove);
                TurnCount++;

                var move = new PlayerMove()
                {
                    Move = bestMove.ToInterfaceMove(),
                    Diagnostics = Diagnostics.CollectAndClear()
                };
                return move;
            }
        }

        private void AnalyzeGamePhase(int movePossibilities)
        {
            if (PlayerTurnCount == 0)
            {
                Phase = GamePhase.Start;
                SearchDepth = 4;
            }
            else if(PlayerTurnCount == 4)
            {
                Phase = GamePhase.Middle;
                SearchDepth = 3;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}");
            }
            else if (LastTurnElapsed.TotalMilliseconds > 2000)
            {
                SearchDepth--;
                Diagnostics.AddMessage($"Decreased search depth to {SearchDepth}");
            }
            else if (LastTurnElapsed.TotalMilliseconds < 200)
            {
                SearchDepth++;
                Diagnostics.AddMessage($"Increased search depth to {SearchDepth}");
            }

        }

        /// <summary>
        /// Main game decision feature. Calculate player and opponent moves to certain depth. When
        /// maximizing, return best move evaluation value for white player. When minimizing return best value for black.
        /// </summary>
        /// <param name="newBoard">Board setup to be evaluated</param>
        /// <param name="depth">How many player and opponent moves by turns are calculated</param>
        /// <param name="alpha">The highest known value at previous recursion level</param>
        /// <param name="beta">The lowest known value at previous recursion level</param>
        /// <param name="maximizingPlayer">Maximizing = white, minimizing = black</param>
        /// <returns></returns>
        private double MiniMax(Board newBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            var allMoves = newBoard.Moves(maximizingPlayer).ToList();

            if (depth == 0 || !allMoves.Any()) return newBoard.Evaluate();
            if (maximizingPlayer)
            {
                var value = -100000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = new Board(newBoard, move);
                    value = Math.Max(value, MiniMax(nextBoard, depth - 1, alpha, beta, false));
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Saved some time by noticing this branch is a dead end
                        Diagnostics.IncrementAlpha();
                        break;
                    }
                }
                return value;
            }
            else
            {
                var value = 100000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = new Board(newBoard, move);
                    value = Math.Min(value, MiniMax(nextBoard, depth - 1, alpha, beta, true));
                    beta = Math.Min(beta, value);
                    if (beta < alpha)
                    {
                        // Saved some time by noticing this branch is a dead end
                        Diagnostics.IncrementBeta();
                        break;
                    }
                }
                return value;
            }
        }

        public sealed override void ReceiveMove(Move opponentMove)
        {
            TurnCount++;
            LatestOpponentMove = opponentMove;

            if (!_connectionTestOverride)
            {
                // Basic validation
                var move = new SingleMove(opponentMove);
                if (Board.ValueAt(move.PrevPos) == null)
                {
                    throw new ArgumentException($"Player [isWhite={!IsPlayerWhite}] Tried to move a from position that is empty");
                }

                if (Board.ValueAt(move.PrevPos) is PieceBase opponentPiece)
                {
                    if (opponentPiece.IsWhite == IsPlayerWhite)
                    {
                        throw new ArgumentException($"Opponent tried to move player piece");
                    }
                }

                // TODO intelligent analyzing what actually happened

                if (Board.ValueAt(move.NewPos) is PieceBase playerPiece)
                {
                    // Opponent captures player targetpiece
                    if (playerPiece.IsWhite == IsPlayerWhite) move.Capture = true;
                    else throw new ArgumentException("Opponent tried to capture own piece.");
                }

                Board.ExecuteMove(move);
                TurnCount++;
            }
        }

        private double BestValue()
        {
            if (IsPlayerWhite) return 1000000;
            else return -1000000;
        }

        private double WorstValue()
        {
            if (IsPlayerWhite) return -1000000;
            else return 1000000;
        }

        public static bool IsOutside((int, int) target)
        {
            if (target.Item1 < 0 || target.Item1 > 7 || target.Item2 < 0 || target.Item2 > 7)
                return true;
            return false;
        }
    }
    
    

    static class Diagnostics
    {
        private static int EvaluationCount = 0;
        private static int AlphaCutoffs = 0;
        private static int BetaCutoffs = 0;

        private static List<string> Messages = new List<string>();
        private static readonly object messageLock = new object();
        private static readonly Stopwatch _timeElapsed = new Stopwatch();
        
        /// <summary>
        /// Call in start of each player turn
        /// </summary>
        public static void StartMoveCalculations()
        {
            _timeElapsed.Start();
        }

        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementEvalCount()
        {
            Interlocked.Increment(ref EvaluationCount);
        }
        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementAlpha()
        {
            Interlocked.Increment(ref AlphaCutoffs);
        }
        /// <summary>
        /// Atomic increment operation
        /// </summary>
        public static void IncrementBeta()
        {
            Interlocked.Increment(ref BetaCutoffs);
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

        /// <summary>
        /// Call in end of each player turn
        /// </summary>
        /// <returns></returns>
        public static string CollectAndClear()
        {
            lock(messageLock)
            {
                var result = $"Board evaluations: {EvaluationCount}. ";

                _timeElapsed.Stop();
                Logic.LastTurnElapsed = _timeElapsed.Elapsed;
                result += $"Time elapsed: {_timeElapsed.ElapsedMilliseconds} ms. ";
                result += $"Alphas: {AlphaCutoffs}, betas: {BetaCutoffs}. ";
                _timeElapsed.Reset();

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
