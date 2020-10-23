using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _lastTurnEvals { get; set; } = 0;
        private int _lastTurnCheckEvals { get; set; } = 0;


        private int _connectionTestIndex = 2;
        public Move LatestOpponentMove { get; set; }
        public IList<Move> GameHistory { get; set; } = new List<Move>();

        private bool _kingInDanger
        {
            get
            {
                if (LatestOpponentMove?.Check == true)
                {
                    return true;
                }

                return false;
            }
        }

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
                var diagnostics = Diagnostics.CollectAndClear(out TimeSpan timeElapsed, out int evals, out int checkEvals);
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

                // Start to validate indirect checkmates also. Filter out moves that result to check
                var allMoves = Board.Moves(isMaximizing, true).ToList();
                Diagnostics.AddMessage($"Available moves found: {allMoves.Count}. ");
                AnalyzeGamePhase(allMoves.Count);

                Diagnostics.StartMoveCalculations();
                var bestMove = AnalyzeBestMove(allMoves);

                if (bestMove == null) throw new ArgumentException($"Board didn't contain any possible move for player [isWhite={IsPlayerWhite}].");

                // Update local
                Board.ExecuteMove(bestMove);
                TurnCount++;

                // Endgame checks
                // TODO should be now read from singlemove
                var castling = false;
                var check = Board.IsCheck(IsPlayerWhite);
                //var checkMate = false;
                //if(check) checkMate = Board.IsCheckMate(IsPlayerWhite, true);
                if(bestMove.Promotion) Diagnostics.AddMessage($"Promotion occured at {bestMove.NewPos.ToAlgebraic()}. ");

                var diagnostics = Diagnostics.CollectAndClear(out TimeSpan timeElapsed, out int evals, out int checkEvals);
                _lastTurnElapsed = timeElapsed;
                _lastTurnEvals = evals;
                _lastTurnCheckEvals = checkEvals;

                var move = new PlayerMove()
                {
                    Move = bestMove.ToInterfaceMove(castling, check),
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
                        singleMove.CheckMate = true;
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
            // TODO as start parameters
            const int MaxDepth = 5;
            const int MinDepth = 2;

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
            else if ((_lastTurnElapsed.TotalMilliseconds > 1500 || _lastTurnEvals > 600000) && SearchDepth > MinDepth)
            {
                SearchDepth--;
                Diagnostics.AddMessage($"Decreased search depth to {SearchDepth}. ");
            }
            else if ((_lastTurnElapsed.TotalMilliseconds < 200 || _lastTurnEvals < 50000) && SearchDepth < MaxDepth)
            {
                // Only raise to max depth if possibilities are low enough
                if(SearchDepth < MaxDepth - 1 || movePossibilities < 21)
                {
                    SearchDepth++;
                    Diagnostics.AddMessage($"Increased search depth to {SearchDepth}. ");
                }
            }

            if(Phase == GamePhase.Middle && _lastTurnCheckEvals >= 800)
            {
                // Endgame - opponent has max 3 non-pawns left
                //var powerPieces = Board.PieceList.Count(p =>
                //    p.IsWhite != IsPlayerWhite && Math.Abs(p.RelativeStrength) > StrengthTable.Pawn);
                //if (powerPieces < 4)
                //{
                //    Phase = GamePhase.EndGame;
                //    Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. ");
                //}

                Phase = GamePhase.EndGame;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. ");
            }
            else if (Phase == GamePhase.EndGame && _lastTurnCheckEvals < 800)
            {
                Phase = GamePhase.Middle;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. ");
            }

        }

        public sealed override void ReceiveMove(Move opponentMove)
        {
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
}
