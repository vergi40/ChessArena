using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonNetStandard;
using CommonNetStandard.Interface;
using CommonNetStandard.Local_implementation;
using vergiBlue.Algorithms;
using vergiBlue.Pieces;

namespace vergiBlue
{
    public enum GamePhase
    {
        /// <summary>
        /// Openings and initial. Very slow evaluation calculation when all the pieces are out open
        /// </summary>
        Start,
        Middle,

        /// <summary>
        /// King might be in danger
        /// </summary>
        MidEndGame,

        /// <summary>
        /// King might be in danger
        /// </summary>
        EndGame
    }

    public class Logic : LogicBase
    {
        // Game strategic variables

        public GamePhase Phase { get; set; }
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

        private int _connectionTestIndex = 2;
        public IMove? LatestOpponentMove { get; set; }
        public IList<IMove> GameHistory { get; set; } = new List<IMove>();

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
        public Strategy Strategy { get; set; }

        /// <summary>
        /// For testing single next turn, overwrite this.
        /// </summary>
        public DiagnosticsData PreviousData { get; set; } = new DiagnosticsData();

        /// <summary>
        /// Use dummy moves to test connection with server
        /// </summary>
        private readonly bool _connectionTestOverride;

        /// <summary>
        /// For tests. Test environment will handle board initialization
        /// </summary>
        public Logic(bool isPlayerWhite, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            _connectionTestOverride = false;
            Strategy = new Strategy(isPlayerWhite, overrideMaxDepth);
        }

        public Logic(IGameStartInformation startInformation, bool connectionTesting, int? overrideMaxDepth = null, Board? overrideBoard = null) : base(startInformation.WhitePlayer)
        {
            _connectionTestOverride = connectionTesting;
            Strategy = new Strategy(startInformation.WhitePlayer, overrideMaxDepth);
            if(overrideBoard != null) Board = new Board(overrideBoard);
            else if (!connectionTesting) Board.InitializeEmptyBoard();
            
            // Opponent non-null only if player is black
            if (!IsPlayerWhite) ReceiveMove(startInformation.OpponentMove);
        }

        public override IPlayerMove CreateMove()
        {

            if (_connectionTestOverride)
            {
                var diagnostics = Diagnostics.CollectAndClear();
                // Dummy moves for connection testing
                var move = new PlayerMoveImplementation(
                    new MoveImplementation()
                    {
                        StartPosition = $"a{_connectionTestIndex--}",
                        EndPosition = $"a{_connectionTestIndex}",
                        PromotionResult = PromotionPieceType.NoPromotion
                    },
                    diagnostics.ToString());

                return move;
            }
            else
            {
                var isMaximizing = IsPlayerWhite;
                Diagnostics.StartMoveCalculations();

                // Get all available moves and do necessary filtering
                List<SingleMove> allMoves = Board.Moves(isMaximizing, true, true).ToList();
                if (allMoves.Count == 0)
                {
                    throw new ArgumentException($"No possible moves for player [isWhite={IsPlayerWhite}]. Game should have ended to draw (stalemate).");
                }

                // Reorder moves to improve alpha-beta cutoffs
                // allMoves = MoveResearch.OrderMoves(allMoves, Board, isMaximizing);

                if(MoveHistory.IsLeaningToDraw(GameHistory))
                {
                    var repetionMove = GameHistory[GameHistory.Count - 4];
                    allMoves.RemoveAll(m =>
                        m.PrevPos.ToAlgebraic() == repetionMove.StartPosition &&
                        m.NewPos.ToAlgebraic() == repetionMove.EndPosition);

                }
                Diagnostics.AddMessage($"Available moves found: {allMoves.Count}. ");

                Strategy.Update(PreviousData, TurnCount);
                var strategyResult = Strategy.DecideSearchDepth(PreviousData, allMoves, Board);
                SearchDepth = strategyResult.searchDepth;
                Phase = strategyResult.gamePhase;
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

                PreviousData = Diagnostics.CollectAndClear();

                var move = new PlayerMoveImplementation(
                    bestMove.ToInterfaceMove(castling, check),
                    PreviousData.ToString());
                GameHistory.Add(move.Move);
                return move;
            }
        }

        /// <summary>
        /// Player should at least:
        /// * Check if there is immediate checkmate available
        /// * Check if there is possible checkmate in two turns.
        /// 
        /// </summary>
        /// <param name="allMoves"></param>
        /// <returns></returns>
        private SingleMove? AnalyzeBestMove(IList<SingleMove> allMoves)
        {
            var isMaximizing = IsPlayerWhite;


            if (Phase == GamePhase.MidEndGame || Phase == GamePhase.EndGame)
            {
                var checkMate = MoveResearch.ImmediateCheckMateAvailable(allMoves, Board, isMaximizing);
                if (checkMate != null) return checkMate;

                var twoTurnCheckMates = MoveResearch.CheckMateInTwoTurns(allMoves, Board, isMaximizing);
                if (twoTurnCheckMates.Count > 1)
                {
                    var evaluatedCheckMates = MoveResearch.GetMoveScoreListParallel(twoTurnCheckMates, SearchDepth, Board, isMaximizing);
                    return MoveResearch.SelectBestMove(evaluatedCheckMates, isMaximizing);
                }
                else if (twoTurnCheckMates.Count > 0)
                {
                    return twoTurnCheckMates.First();
                }
            }

            // TODO separate logic to different layers. e.g. player depth at 2, 4 and when to use simple isCheckMate
            var evaluated = MoveResearch.GetMoveScoreListParallel(allMoves, SearchDepth, Board, isMaximizing);
            SingleMove? bestMove = MoveResearch.SelectBestMove(evaluated, isMaximizing);
            
            return bestMove;
        }


        public sealed override void ReceiveMove(IMove? opponentMove)
        {
            if(opponentMove == null)
            {
                throw new ArgumentException($"Received null move. Error or game has ended.");
            }

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
