using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Client;
using CommonNetStandard.Interface;
using vergiBlue.Algorithms;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;

namespace vergiBlue.Logic
{
    public class Logic : LogicBase
    {
        // Game strategic variables
        public IMove? LatestOpponentMove { get; set; }
        public IList<IMove> GameHistory { get; set; } = new List<IMove>();
        
        public IBoard Board { get; set; } = BoardFactory.CreateEmptyBoard();

        /// <summary>
        /// For testing single next turn, overwrite this.
        /// </summary>
        public DiagnosticsData PreviousData { get; set; } = new DiagnosticsData();

        /// <summary>
        /// Set before starting logic, if e.g. want to try transposition tables or parallel
        /// search.
        /// </summary>
        public LogicSettings Settings { get; set; } = new LogicSettings();

        private AlgorithmController _algorithmController { get; } = new AlgorithmController();

        /// <summary>
        /// For tests. Need to set board explicitly. Test environment handles initializations.
        /// </summary>
        [Obsolete("For tests, use constructor with Board parameter.")]
        public Logic(bool isPlayerWhite, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
        }

        /// <summary>
        /// For tests. Start board known. Test environment handles initializations.
        /// </summary>
        public Logic(bool isPlayerWhite, IBoard board, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
            Board = BoardFactory.CreateClone(board);
            Board.Shared.Testing = true;
        }

        public Logic(IGameStartInformation startInformation, int? overrideMaxDepth = null, IBoard? overrideBoard = null) : base(startInformation.WhitePlayer)
        {
            _algorithmController.Initialize(startInformation.WhitePlayer, overrideMaxDepth);
            if (overrideBoard != null) Board = BoardFactory.CreateClone(overrideBoard);
            else Board.InitializeDefaultBoard();
            
            // Opponent non-null only if player is black
            if (!IsPlayerWhite) ReceiveMove(startInformation.OpponentMove);
        }

        /// <summary>
        /// Create move from arbitral situation
        /// </summary>
        /// <param name="searchDepth"></param>
        /// <param name="checkOpenings">For testing. Don't want to use opening book for arbitrary test situations. </param>
        /// <param name="previousMoveCount"></param>
        /// <returns></returns>
        public IPlayerMove CreateMoveWithDepth(int searchDepth, bool checkOpenings = false, int previousMoveCount = 0)
        {
            Board.Shared.GameTurnCount = previousMoveCount;
            Board.Strategic.TurnCountInCurrentDepth = previousMoveCount;
            Board.Strategic.SkipOpeningChecks = !checkOpenings;
            return CreateNewMove(searchDepth);
        }

        public override IPlayerMove CreateMove()
        {
            return CreateNewMove();
        }


        private IPlayerMove CreateNewMove(int? overrideSearchDepth = null)
        {
            var isMaximizing = IsPlayerWhite;
            Collector.Instance.StartMoveCalculationTimer();

            var startInfo = new TurnStartInfo(isMaximizing, GameHistory.ToList(), Settings, PreviousData,
                overrideSearchDepth);
            _algorithmController.TurnStartUpdate(startInfo);

            // Common start measures - WIP
            if (Settings.UseTranspositionTables)
            {
                // Delete old entries from tables
                var transpositions = Board.Shared.Transpositions.Tables;
                if (transpositions.Any())
                {
                    var toBeDeleted = new List<ulong>();
                    var currentTurnCount = Board.Shared.GameTurnCount;

                    foreach (var transposition in transpositions)
                    {
                        // If transposition.turn 20 < current 25 - 4
                        if (transposition.Value.GameTurnCount <
                            currentTurnCount - Settings.ClearSavedTranspositionsAfterTurnsPassed)
                        {
                            toBeDeleted.Add(transposition.Key);
                        }
                    }

                    foreach (var hash in toBeDeleted)
                    {
                        transpositions.Remove(hash);
                    }
                    
                    if(Settings.UseFullDiagnostics)
                    {
                        if(toBeDeleted.Any()) Collector.AddCustomMessage($"Deleted {toBeDeleted.Count} old transposition entries.");
                        Collector.AddCustomMessage($"Total transpositions: {transpositions.Count}.");
                    }
                }
            }

            // Opening -- done

            // Get all available moves and do necessary filtering
            List<SingleMove> validMoves = Board.MoveGenerator.MovesWithOrdering(isMaximizing, true, true).ToList();
            
            if (MoveHistory.IsLeaningToDraw(GameHistory))
            {
                // Repetition
                // Take 4th from the end of list
                var repetionMove = GameHistory[^4];
                validMoves.RemoveAll(m =>
                    m.PrevPos.ToAlgebraic() == repetionMove.StartPosition &&
                    m.NewPos.ToAlgebraic() == repetionMove.EndPosition);
            }

            if (validMoves.Count == 0)
            {
                // Game ended to stalemate
                throw new ArgumentException(
                    $"No possible moves for player [isWhite={IsPlayerWhite}]. Game should have ended to draw (stalemate).");
            }

            Collector.AddCustomMessage($"Available moves found: {validMoves.Count}. ");
            
            // Use controller - WIP
            var aiMove = _algorithmController.GetBestMove(Board, validMoves);

            if (aiMove == null)
                throw new ArgumentException(
                    $"Board didn't contain any possible move for player [isWhite={IsPlayerWhite}].");

            if (Board.Shared.Transpositions.Tables.Count > 0)
                Collector.AddCustomMessage($"Transposition tables saved: {Board.Shared.Transpositions.Tables.Count}");

            // Update local
            var moveWithData = Board.CollectMoveProperties(aiMove);
            Board.ExecuteMoveWithValidation(moveWithData);
            Board.Shared.GameTurnCount++;
            
            var (analyticsOutput, previousData) = Collector.Instance.CollectAndClear(Settings.UseFullDiagnostics);
            PreviousData = previousData;

            var move = new PlayerMoveImplementation(moveWithData.ToInterfaceMove(),
                analyticsOutput);
            GameHistory.Add(move.Move);
            return move;
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

            // Interface misses properties like capture, enpassant
            move = Board.CollectMoveProperties(move);

            Board.ExecuteMoveWithValidation(move);
            GameHistory.Add(opponentMove);
            Board.Shared.GameTurnCount++;
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
