using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using vergiBlue.Algorithms;
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

        private TimeSpan _lastTurnElapsed { get; set; } = TimeSpan.Zero;



        private int _connectionTestIndex = 2;
        public Move LatestOpponentMove { get; set; }
        public IList<Move> GameHistory { get; set; } = new List<Move>();

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
                var diagnostics = Diagnostics.CollectAndClear(out TimeSpan timeElapsed);
                _lastTurnElapsed = timeElapsed;
                // Dummy moves for connection testing
                var move = new PlayerMove()
                {
                    Move = new Move()
                    {
                        StartPosition = $"a{_connectionTestIndex--}",
                        EndPosition = $"a{_connectionTestIndex}",
                        PromotionResult = Move.Types.PromotionPieceType.NoPromotion
                    },
                    Diagnostics = diagnostics
                };

                return move;
            }
            else
            {
                var isMaximizing = IsPlayerWhite;

                var allMoves = Board.Moves(isMaximizing).ToList();
                AnalyzeGamePhase(allMoves.Count);

                Diagnostics.StartMoveCalculations();
                var bestMove = AnalyzeBestMove(allMoves);

                if (bestMove == null) throw new ArgumentException($"Board didn't contain any possible move for player [isWhite={IsPlayerWhite}].");

                // Update local
                Board.ExecuteMove(bestMove);
                TurnCount++;

                // Endgame checks
                var castling = false;
                var check = Board.IsCheck(IsPlayerWhite);
                var checkMate = false;
                if(check) checkMate = Board.IsCheckMate(IsPlayerWhite, true);

                var diagnostics = Diagnostics.CollectAndClear(out TimeSpan timeElapsed);
                _lastTurnElapsed = timeElapsed;
                var move = new PlayerMove()
                {
                    Move = bestMove.ToInterfaceMove(castling, check, checkMate),
                    Diagnostics = diagnostics
                };
                GameHistory.Add(move.Move);
                return move;
            }
        }

        private SingleMove AnalyzeBestMove(IList<SingleMove> allMoves)
        {
            var isMaximizing = IsPlayerWhite;
            if (Phase == GamePhase.EndGame)
            {
                // Brute search checkmate
                foreach (var singleMove in allMoves)
                {
                    var newBoard = new Board(Board, singleMove);
                    if (newBoard.IsCheckMate(isMaximizing, false))
                    {
                        return singleMove;
                    }
                }
                foreach (var singleMove in allMoves)
                {
                    var newBoard = new Board(Board, singleMove);
                    if (CheckMate.InTwoTurns(newBoard, isMaximizing))
                    {
                        return singleMove;
                    }
                }
            }

            // TODO separate logic to different layers. e.g. player depth at 2, 4 and when to use simple isCheckMate
            var bestValue = WorstValue(IsPlayerWhite);
            SingleMove bestMove = null;
            foreach (var singleMove in allMoves)
            {
                var newBoard = new Board(Board, singleMove);
                var value = MiniMax.ToDepth(newBoard, SearchDepth, -100000, 100000, !isMaximizing);
                
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

            return bestMove;
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
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. Search depth {SearchDepth}. ");
            }
            else if (_lastTurnElapsed.TotalMilliseconds > 2000 && SearchDepth > 2)
            {
                SearchDepth--;
                Diagnostics.AddMessage($"Decreased search depth to {SearchDepth}. ");
            }
            else if (_lastTurnElapsed.TotalMilliseconds < 200 && SearchDepth < 5)
            {
                SearchDepth++;
                Diagnostics.AddMessage($"Increased search depth to {SearchDepth}. ");
            }

            if(Phase != GamePhase.Start && Phase != GamePhase.EndGame)
            {
                // Endgame - opponent has max 3 non-pawns left
                var powerPieces = Board.PieceList.Count(p =>
                    p.IsWhite != IsPlayerWhite && Math.Abs(p.RelativeStrength) > StrengthTable.Pawn);
                if (powerPieces < 4)
                {
                    Phase = GamePhase.EndGame;
                    Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. ");
                }
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
                GameHistory.Add(opponentMove);
                TurnCount++;
            }
        }

        private double BestValue(bool isMaximizing)
        {
            if (isMaximizing) return 1000000;
            else return -1000000;
        }

        private double WorstValue(bool isMaximizing)
        {
            if (isMaximizing) return -1000000;
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
        public static string CollectAndClear(out TimeSpan timeElapsed)
        {
            lock(messageLock)
            {
                var result = $"Board evaluations: {EvaluationCount}. ";

                _timeElapsed.Stop();
                timeElapsed = _timeElapsed.Elapsed;
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
