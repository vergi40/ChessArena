using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Client;
using CommonNetStandard.Interface;
using vergiBlue.Algorithms;

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

    public class LogicSettings
    {
        // Config bools
        public bool UseTranspositionTables { get; set; } = false;
        public bool UseParallelComputation { get; set; } = true;
        public bool UseIterativeDeepening { get; set; } = false;

        /// <summary>
        /// Log more data, like alpha-betas
        /// </summary>
        public bool UseFullDiagnostics { get; set; } = false;


        /// <summary>
        /// For tests. Keep parameters intact. After logic constructor, this initialization can be used to set any logical aspect.
        /// LTS - Long Time Support. Parameters will be kept the same.
        /// </summary>
        /// <param name="useParallelComputation"></param>
        /// <param name="useTranspositionTables"></param>
        /// <param name="useIterativeDeepening"></param>
        public void SetConfigLTS(bool? useParallelComputation = null, bool? useTranspositionTables = null, bool? useIterativeDeepening = null, bool? useFullDiagnostics = null)
        {
            if (useParallelComputation != null) UseParallelComputation = useParallelComputation.Value;
            if (useTranspositionTables != null) UseTranspositionTables = useTranspositionTables.Value;
            if (useIterativeDeepening != null) UseIterativeDeepening = useIterativeDeepening.Value;
            if (useFullDiagnostics != null) UseFullDiagnostics = useFullDiagnostics.Value;
        }
    }

    public class Logic : LogicBase
    {
        // Game strategic variables

        public int SearchDepth { get; set; } = 4;
        public Strategy Strategy { get; set; }

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

        public IMove? LatestOpponentMove { get; set; }
        public IList<IMove> GameHistory { get; set; } = new List<IMove>();
        
        public Board Board { get; set; } = new Board();
        private OpeningLibrary Openings { get; } = new OpeningLibrary();

        /// <summary>
        /// For testing single next turn, overwrite this.
        /// </summary>
        public DiagnosticsData PreviousData { get; set; } = new DiagnosticsData();


        public LogicSettings Settings { get; set; } = new LogicSettings();


        /// <summary>
        /// For tests. Need to set board explicitly. Test environment handles initializations.
        /// Custom set <see cref="UseParallelComputation"/>.
        /// Custom set <see cref="UseTranspositionTables"/>.
        /// Custom set <see cref="UseIterativeDeepening"/>.
        /// </summary>
        [Obsolete("For tests, use constructor with Board parameter.")]
        public Logic(bool isPlayerWhite, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            Strategy = new Strategy(isPlayerWhite, overrideMaxDepth, Settings.UseTranspositionTables);
        }

        /// <summary>
        /// For tests. Start board known. Test environment handles initializations.
        /// Custom set <see cref="UseParallelComputation"/>.
        /// Custom set <see cref="UseTranspositionTables"/>.
        /// Custom set <see cref="UseIterativeDeepening"/>.
        /// </summary>
        public Logic(bool isPlayerWhite, Board board, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            Strategy = new Strategy(isPlayerWhite, overrideMaxDepth, Settings.UseTranspositionTables);
            Board = new Board(board);
        }

        public Logic(IGameStartInformation startInformation, int? overrideMaxDepth = null, Board? overrideBoard = null) : base(startInformation.WhitePlayer)
        {
            Strategy = new Strategy(startInformation.WhitePlayer, overrideMaxDepth, Settings.UseTranspositionTables);
            if(overrideBoard != null) Board = new Board(overrideBoard);
            else Board.InitializeEmptyBoard();
            
            // Opponent non-null only if player is black
            if (!IsPlayerWhite) ReceiveMove(startInformation.OpponentMove);
        }

        /// <summary>
        /// Create move from arbitral situation
        /// </summary>
        /// <param name="searchDepth"></param>
        /// <param name="checkOpenings"></param>
        /// <param name="previousMoveCount"></param>
        /// <returns></returns>
        public IPlayerMove CreateMoveWithDepth(int searchDepth, bool checkOpenings = false, int previousMoveCount = 0)
        {
            TurnCount = previousMoveCount;
            return CreateNewMove(checkOpenings, searchDepth);
        }


        public override IPlayerMove CreateMove()
        {
            return CreateNewMove(true);
        }

        private IPlayerMove CreateNewMove(bool checkOpenings, int? overrideSearchDepth = null)
        {
            var isMaximizing = IsPlayerWhite;
            Diagnostics.StartMoveCalculations();

            // Opening
            if (GameHistory.Count < 10 && checkOpenings)
            {
                var previousMoves = GetPreviousMoves();
                var openingMove = Openings.NextMove(previousMoves);

                if (openingMove != null)
                {
                    Board.ExecuteMove(openingMove);
                    TurnCount++;
                    PreviousData = Diagnostics.CollectAndClear();

                    var result = new PlayerMoveImplementation(
                        openingMove.ToInterfaceMove(false, false), PreviousData.ToString());
                    GameHistory.Add(result.Move);
                    return result;
                }
            }

            // Get all available moves and do necessary filtering
            List<SingleMove> allMoves = Board.Moves(isMaximizing, true, true).ToList();
            if (allMoves.Count == 0)
            {
                // Game ended to stalemate
                
                throw new ArgumentException(
                    $"No possible moves for player [isWhite={IsPlayerWhite}]. Game should have ended to draw (stalemate).");
            }

            if (MoveHistory.IsLeaningToDraw(GameHistory))
            {
                // Take 4th from the end of list
                var repetionMove = GameHistory[^4];
                allMoves.RemoveAll(m =>
                    m.PrevPos.ToAlgebraic() == repetionMove.StartPosition &&
                    m.NewPos.ToAlgebraic() == repetionMove.EndPosition);
            }

            Diagnostics.AddMessage($"Available moves found: {allMoves.Count}. ");

            // 
            if(overrideSearchDepth == null)
            {
                Strategy.Update(PreviousData, TurnCount);
                SearchDepth = Strategy.DecideSearchDepth(PreviousData, allMoves, Board);
            }
            else
            {
                SearchDepth = overrideSearchDepth.Value;
            }
            
            var bestMove = AnalyzeBestMove(allMoves);

            if (bestMove == null)
                throw new ArgumentException(
                    $"Board didn't contain any possible move for player [isWhite={IsPlayerWhite}].");

            // Update local
            Board.ExecuteMove(bestMove);
            TurnCount++;

            // Endgame checks
            var castling = false;
            var check = Board.IsCheck(IsPlayerWhite);
            //var checkMate = false;
            //if(check) checkMate = Board.IsCheckMate(IsPlayerWhite, true);
            if (bestMove.Promotion) Diagnostics.AddMessage($"Promotion occured at {bestMove.NewPos.ToAlgebraic()}. ");

            PreviousData = Diagnostics.CollectAndClear(Settings.UseFullDiagnostics);

            var move = new PlayerMoveImplementation(
                bestMove.ToInterfaceMove(castling, check),
                PreviousData.ToString());
            GameHistory.Add(move.Move);
            return move;
        }

        private IList<SingleMove> GetPreviousMoves()
        {
            var moves = new List<SingleMove>();
            foreach (var move in GameHistory)
            {
                // TODO optimal case would have capture moves tagged also
                moves.Add(new SingleMove(move));
            }

            return moves;
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

            if (Strategy.Phase == GamePhase.MidEndGame || Strategy.Phase == GamePhase.EndGame)
            {
                var checkMate = MoveResearch.ImmediateCheckMateAvailable(allMoves, Board, isMaximizing);
                if (checkMate != null) return checkMate;

                var twoTurnCheckMates = MoveResearch.CheckMateInTwoTurns(allMoves, Board, isMaximizing);
                if (twoTurnCheckMates.Count > 1)
                {
                    var evaluatedCheckMates = MoveResearch.GetMoveScoreListParallel(twoTurnCheckMates, SearchDepth, Board, isMaximizing);
                    return MoveResearch.SelectBestMove(evaluatedCheckMates, isMaximizing, true);
                }
                else if (twoTurnCheckMates.Count > 0)
                {
                    return twoTurnCheckMates.First();
                }
            }
            
            if(Settings.UseIterativeDeepening)
            {
                return MoveResearch.SelectBestWithIterativeDeepening(allMoves, SearchDepth, Board, isMaximizing,
                    Settings.UseTranspositionTables);
            }

            EvaluationResult evaluated;
            if (Settings.UseParallelComputation)
            {
                // CPU profiler - first breakpoint here
                evaluated = MoveResearch
                    .GetMoveScoreListParallel(allMoves, SearchDepth, Board, isMaximizing);
                
                if(evaluated.Empty)
                {
                    throw new ArgumentException($"Logical error - parallel computing lost moves during evaluation.");
                }
            }
            else
            {
                evaluated = MoveResearch.GetMoveScoreList(allMoves, SearchDepth, Board, isMaximizing, Settings.UseTranspositionTables);
                
                if(Board.SharedData.Transpositions.Tables.Count > 0)
                    Diagnostics.AddMessage($"Transposition tables saved: {Board.SharedData.Transpositions.Tables.Count}");
            }
            
            return MoveResearch.SelectBestMove(evaluated, isMaximizing, true);
            // CPU profiler - second breakpoint here
        }


        public sealed override void ReceiveMove(IMove? opponentMove)
        {
            LatestOpponentMove = opponentMove ?? throw new ArgumentException($"Received null move. Error or game has ended.");

            // Basic validation
            var move = new SingleMove(opponentMove);
            if (Board.ValueAt(move.PrevPos) == null)
            {
                throw new ArgumentException(
                    $"Player [isWhite={!IsPlayerWhite}] Tried to move a from position that is empty");
            }

            var from = Board.ValueAt(move.PrevPos);
            if (from?.IsWhite == IsPlayerWhite)
            {
                throw new ArgumentException($"Opponent tried to move player piece");
            }

            // TODO intelligent analyzing what actually happened

            var targetPosition = Board.ValueAt(move.NewPos);
            if (targetPosition != null)
            {
                if (targetPosition.IsWhite == IsPlayerWhite)
                {
                    // Opponent captures player targetpiece
                    move.Capture = true;
                }
                else
                {
                    throw new ArgumentException("Opponent tried to capture own piece.");
                }
            }

            Board.ExecuteMove(move);
            GameHistory.Add(opponentMove);
            TurnCount++;
        }

        public static bool IsOutside((int, int) target)
        {
            if (target.Item1 < 0 || target.Item1 > 7 || target.Item2 < 0 || target.Item2 > 7)
                return true;
            return false;
        }

        /// <summary>
        /// For tests. Keep parameters intact. After logic constructor, this initialization can be used to set any logical aspect.
        /// LTS - Long Time Support. Parameters will be kept the same.
        /// </summary>
        /// <param name="useParallelComputation"></param>
        /// <param name="useTranspositionTables"></param>
        /// <param name="useIterativeDeepening"></param>
        public void SetConfigLTS(bool? useParallelComputation = null, bool? useTranspositionTables = null, bool? useIterativeDeepening = null)
        {
            if (useParallelComputation != null) Settings.UseParallelComputation = useParallelComputation.Value;
            if (useTranspositionTables != null) Settings.UseTranspositionTables = useTranspositionTables.Value;
            if (useIterativeDeepening != null) Settings.UseIterativeDeepening = useIterativeDeepening.Value;
        }
    }
}
