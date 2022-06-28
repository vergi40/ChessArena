using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard.Client;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue.Algorithms;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;

namespace vergiBlue.Logic
{
    public class Logic : LogicBase
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<Logic>();

        // Game strategic variables
        public IMove? LatestOpponentMove { get; set; }

        /// <summary>
        /// Note: only accurate from start if board not generated from fen string
        /// </summary>
        public IList<IMove> GameHistory { get; set; } = new List<IMove>();

        // Just initialize to any - will be overridden
        public IBoard Board { get; set; } = new Board(false);

        /// <summary>
        /// For testing single next turn, overwrite this.
        /// </summary>
        public DiagnosticsData PreviousData { get; set; } = new DiagnosticsData();

        /// <summary>
        /// Set before starting logic, if e.g. want to try transposition tables or parallel
        /// search.
        /// </summary>
        public LogicSettings Settings { get; set; } = new LogicSettings();

        /// <summary>
        /// For testing. Don't want to use opening book for arbitrary test situations.
        /// False only when initializing default board
        /// </summary>
        public bool SkipOpeningChecks { get; set; } = true;

        private AlgorithmController _algorithmController { get; } = new AlgorithmController();

        /// <summary>
        /// Uci instance. Don't know which side yet
        /// </summary>
        public Logic() : base(true)
        {
            // TODO
        }

        /// <summary>
        /// For tests. Need to set board explicitly. Test environment handles initializations.
        /// </summary>
        [Obsolete("For tests, use constructor with Board parameter.")]
        public Logic(bool isPlayerWhite, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
            SkipOpeningChecks = true;
            _logger.LogInformation("Logic initialized");
        }

        /// <summary>
        /// For tests. Start board known. Test environment handles initializations.
        /// </summary>
        public Logic(bool isPlayerWhite, IBoard board, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
            Board = BoardFactory.CreateClone(board);
            Board.Shared.Testing = true;
            SkipOpeningChecks = true;
            _logger.LogInformation("Logic initialized");
        }

        public Logic(IGameStartInformation startInformation, int? overrideMaxDepth = null, IBoard? overrideBoard = null) : base(startInformation.WhitePlayer)
        {
            _algorithmController.Initialize(startInformation.WhitePlayer, overrideMaxDepth);
            if (overrideBoard != null)
            {
                Board = BoardFactory.CreateClone(overrideBoard);
                SkipOpeningChecks = true;
            }
            else
            {
                SetDefaultBoard();
            }

            _logger.LogInformation("Logic initialized");

            // Opponent non-null only if player is black
            if (!IsPlayerWhite) ReceiveMove(startInformation.OpponentMove);
        }

        /// <summary>
        /// Initialize hash tables, piece cache, move cache
        /// </summary>
        public void InitializeStaticSystems()
        {
            Board = BoardFactory.CreateEmptyBoard();
        }

        /// <summary>
        /// Clear all game-related
        /// </summary>
        public void NewGame()
        {
            Board = BoardFactory.CreateEmptyBoard();
            LatestOpponentMove = null;
            GameHistory.Clear();

            Collector.Instance.CollectAndClear();
        }

        /// <summary>
        /// Set board to default chess opening board
        /// </summary>
        public void SetDefaultBoard()
        {
            Board = BoardFactory.CreateDefault();
            SkipOpeningChecks = false;
        }

        public void SetBoard(string startPosOrFenBoard, List<string> moves)
        {
            bool isWhite;
            if (startPosOrFenBoard.Equals("startpos"))
            {
                // TODO separate static system init
                SetDefaultBoard();
                isWhite = true;
            }
            else
            {
                Board = BoardFactory.CreateFromFen(startPosOrFenBoard, out isWhite);
                IsPlayerWhite = isWhite;
                SkipOpeningChecks = true;
            }

            foreach (var move in moves)
            {
                var tempMove = SingleMoveFactory.Create(move);
                var fullMove = Board.CollectMoveProperties(tempMove);
                Board.ExecuteMove(fullMove);

                // Add to logic history - in case of opening book
                var interfaceMove = fullMove.ToInterfaceMove();
                GameHistory.Add(interfaceMove);

                isWhite = !isWhite;
            }

            IsPlayerWhite = isWhite;
            Board.InitializeSubSystems();
        }

        /// <summary>
        /// Create move from arbitral situation.
        /// If opening book should be checked, configure it with <see cref="SkipOpeningChecks"/>
        /// </summary>
        /// <param name="searchDepth"></param>
        /// <param name="previousMoveCount"></param>
        /// <returns></returns>
        public IPlayerMove CreateMoveWithDepth(int searchDepth, int previousMoveCount = 0)
        {
            Board.Shared.GameTurnCount = previousMoveCount;
            Board.Strategic.TurnCountInCurrentDepth = previousMoveCount;
            return CreateNewMove(searchDepth);
        }

        public override IPlayerMove CreateMove()
        {
            _logger.LogInformation("Starting create move operations...");
            var bestMove = CreateNewMove();
            var inner = bestMove.Move;
            _logger.LogInformation($"Created move {inner.StartPosition}{inner.EndPosition}{SingleMove.ConvertPromotion(inner.PromotionResult)}");
            
            return bestMove;
        }


        private IPlayerMove CreateNewMove(int? overrideSearchDepth = null)
        {
            Collector.Instance.StartMoveCalculationTimer();

            // Common start measures
            RefreshAlgorithm(overrideSearchDepth);

            if (Settings.UseTranspositionTables)
            {
                RefreshTranspositions();
            }

            // Get all available moves and do necessary ordering & filtering
            List<SingleMove> validMoves = GetValidMoves();
            
            // Best move
            var aiMove = _algorithmController.GetBestMove(Board, validMoves, SkipOpeningChecks);
            Validator.ValidateMoveAndColor(Board, aiMove, IsPlayerWhite);

            if (Board.Shared.Transpositions.Tables.Count > 0)
                Collector.AddCustomMessage($"Transposition tables saved: {Board.Shared.Transpositions.Tables.Count}");

            // Update local
            var moveWithData = Board.CollectMoveProperties(aiMove);
            Board.ExecuteMove(moveWithData);
            Board.Shared.GameTurnCount++;
            
            var (analyticsOutput, previousData) = Collector.Instance.CollectAndClear(Settings.UseFullDiagnostics);
            PreviousData = previousData;

            var move = new PlayerMoveImplementation(moveWithData.ToInterfaceMove(),
                analyticsOutput);
            GameHistory.Add(move.Move);
            return move;
        }

        private void RefreshAlgorithm(int? overrideSearchDepth)
        {
            var isMaximizing = IsPlayerWhite;
            var startInfo = new TurnStartInfo(isMaximizing, GameHistory.ToList(), Settings, PreviousData,
                overrideSearchDepth);
            _algorithmController.TurnStartUpdate(startInfo);
        }

        private void RefreshAlgorithm(SearchParameters parameters)
        {
            var isMaximizing = IsPlayerWhite;
            var startInfo = new TurnStartInfo(isMaximizing, GameHistory.ToList(), Settings, PreviousData, null);
            parameters.TurnStartInfo = startInfo;
            _algorithmController.TurnStartUpdate(parameters);
        }

        private void RefreshTranspositions()
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

                if (Settings.UseFullDiagnostics)
                {
                    if (toBeDeleted.Any()) Collector.AddCustomMessage($"Deleted {toBeDeleted.Count} old transposition entries.");
                    Collector.AddCustomMessage($"Total transpositions: {transpositions.Count}.");
                }
            }
        }

        private List<SingleMove> GetValidMoves()
        {
            var isMaximizing = IsPlayerWhite;
            var validMoves = Board.MoveGenerator.MovesWithOrdering(isMaximizing, true, true).ToList();

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

            var movesSorted = validMoves.Select(m => m.ToCompactString()).OrderBy(m => m);
            
            _logger.LogInformation($"{validMoves.Count} valid moves found: {string.Join(", ", movesSorted)}.");
            Collector.AddCustomMessage($"{validMoves.Count} valid moves found.");
            return validMoves;
        }

        public sealed override void ReceiveMove(IMove? opponentMove)
        {
            LatestOpponentMove = opponentMove ?? throw new ArgumentException($"Received null move. Error or game has ended.");
            _logger.LogInformation(
                $"Received move {opponentMove.StartPosition}{opponentMove.EndPosition}{SingleMove.ConvertPromotion(opponentMove.PromotionResult)}");

            // Basic validation
            var move = new SingleMove(opponentMove);
            Validator.ValidateMoveAndColor(Board, move, !IsPlayerWhite);

            // Interface misses properties like capture, enpassant
            move = Board.CollectMoveProperties(move);
            Board.ExecuteMove(move);
            Board.Shared.GameTurnCount++;

            GameHistory.Add(opponentMove);
        }

        /// <summary>
        /// Create search task with cancellation support
        /// </summary>
        public Task<SearchResult> CreateSearchTask(UciGoParameters parameters, Action<string> searchInfoUpdate, CancellationToken ct)
        {
            var searchParameters = new SearchParameters(parameters, searchInfoUpdate, ct);
            
            // Do merge class of parameters and action infoupdate
            var move = CreateNewMoveUci(searchParameters);

            return Task.FromResult(new SearchResult(move));
        }


        private ISingleMove CreateNewMoveUci(SearchParameters parameters)
        {
            Collector.Instance.StartMoveCalculationTimer();

            // Common start measures
            RefreshAlgorithm(parameters);

            if (Settings.UseTranspositionTables)
            {
                RefreshTranspositions();
            }

            // Get all available moves and do necessary ordering & filtering
            List<SingleMove> validMoves = GetValidMoves();

            // Best move
            var aiMove = _algorithmController.GetBestMove(Board, validMoves, SkipOpeningChecks);
            Validator.ValidateMoveAndColor(Board, aiMove, IsPlayerWhite);

            if (Board.Shared.Transpositions.Tables.Count > 0)
                Collector.AddCustomMessage($"Transposition tables saved: {Board.Shared.Transpositions.Tables.Count}");

            // Update local
            var moveWithData = Board.CollectMoveProperties(aiMove);

            // NOTE in uci commands the engine never executes move itself
            //Board.ExecuteMove(moveWithData);
            //Board.Shared.GameTurnCount++;

            var (analyticsOutput, previousData) = Collector.Instance.CollectAndClear(Settings.UseFullDiagnostics);
            PreviousData = previousData;

            // NOTE: never add to move history, since uci game is not played continuously to same board
            return moveWithData;
        }
    }

    // Note: as class instead of just move string. Room to extend with e.g. ponder return values
    public class SearchResult
    {
        public ISingleMove BestMove { get; }

        public SearchResult(ISingleMove bestMove)
        {
            BestMove = bestMove;
        }
    }
}
