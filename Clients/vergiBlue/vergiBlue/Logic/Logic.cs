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
using vergiBlue.Database;

namespace vergiBlue.Logic
{
    public class Logic : IAiClient, IUciClient
    {
        private readonly ILoggerFactory _loggerFactory = ApplicationLogging.LoggerFactory;
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
        /// TODO WIP. Read implementation from settings
        /// </summary>
        private IReplayPersistor _replayPersistor { get; } = new ReplayPersistor(new EFDatabase());

        private DataFactory _dataFactory { get; } = new DataFactory();

        /// <summary>
        /// Client is white player and starts the game
        /// </summary>
        public bool IsPlayerWhite { get; set; }

        /// <summary>
        /// Uci instance. Don't know which side yet
        /// </summary>
        public Logic()
        {
            // TODO
        }

        /// <summary>
        /// For tests. Need to set board explicitly. Test environment handles initializations.
        /// </summary>
        [Obsolete("For tests, use constructor with Board parameter.")]
        public Logic(bool isPlayerWhite, int? overrideMaxDepth = null)
        {
            IsPlayerWhite = isPlayerWhite;

            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
            SkipOpeningChecks = true;
            _logger.LogDebug("Logic initialized");
            _replayPersistor = new DebugReplay();
            _replayPersistor.InitializeNewGame(isPlayerWhite, Board);
        }

        /// <summary>
        /// For tests. Start board known. Test environment handles initializations.
        /// </summary>
        public Logic(bool isPlayerWhite, IBoard board, int? overrideMaxDepth = null)
        {
            IsPlayerWhite = isPlayerWhite;

            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
            Board = BoardFactory.CreateClone(board);
            Board.Shared.Testing = true;
            SkipOpeningChecks = true;
            _logger.LogDebug("Logic initialized");
            _replayPersistor.InitializeNewGame(isPlayerWhite, Board);
        }

        public Logic(IGameStartInformation startInformation, int? overrideMaxDepth = null, IBoard? overrideBoard = null)
        {
            IsPlayerWhite = startInformation.WhitePlayer;

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

            _logger.LogDebug("Logic initialized");
            _replayPersistor.InitializeNewGame(startInformation.WhitePlayer, Board);

            // Opponent non-null only if player is black
            if (!startInformation.WhitePlayer) ReceiveMove(startInformation.OpponentMove);
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

        public IPlayerMove CreateMove()
        {
            var bestMove = CreateNewMove();
            return bestMove;
        }
        
        private IPlayerMove CreateNewMove(int? overrideSearchDepth = null)
        {
            _logger.LogDebug("Starting create move operations...");
            Collector.Instance.StartMoveCalculationTimer();
            if (Settings.UseTranspositionTables)
            {
                RefreshTranspositions();
            }

            // Common start measures
            RefreshAlgorithm(overrideSearchDepth);

            var moveBuilder = new MoveBuilder(_loggerFactory);
            moveBuilder.SetBoardAndSide(Board, IsPlayerWhite);
            moveBuilder.SetAlgorithmControl(_algorithmController);
            moveBuilder.SetSkipOpeningChecks(SkipOpeningChecks);
            moveBuilder.SetValidMoves(GameHistory);
            
            // Best move
            var aiMove = moveBuilder.BuildBestMove();

            if (Board.Shared.Transpositions.Tables.Count > 0)
                Collector.AddCustomMessage($"Transposition tables saved: {Board.Shared.Transpositions.Tables.Count}");

            // Update local
            var moveWithData = Board.CollectMoveProperties(aiMove);
            Board.ExecuteMove(moveWithData);
            Board.Shared.GameTurnCount++;
            
            var (analyticsOutput, diagnosticsData) = Collector.Instance.CollectAndClear(Settings.UseFullDiagnostics);
            PreviousData = diagnosticsData;

            var move = new PlayerMoveImplementation(moveWithData.ToInterfaceMove(),
                analyticsOutput);
            GameHistory.Add(move.Move);

            var moveData = _dataFactory.CreateDescriptive(Board, IsPlayerWhite, diagnosticsData);
            _replayPersistor.SaveMoveWithAnalytics(moveData);

            var inner = move.Move;
            _logger.LogDebug($"Created move {inner.StartPosition}{inner.EndPosition}{SingleMove.ConvertPromotion(inner.PromotionResult)}");
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
        
        public void ReceiveMove(IMove? opponentMove)
        {
            LatestOpponentMove = opponentMove ?? throw new ArgumentException($"Received null move. Error or game has ended.");
            _logger.LogDebug(
                $"Received move {opponentMove.StartPosition}{opponentMove.EndPosition}{SingleMove.ConvertPromotion(opponentMove.PromotionResult)}");

            // Basic validation
            var move = new SingleMove(opponentMove);
            Validator.ValidateMoveAndColor(Board, move, !IsPlayerWhite);

            // Interface misses properties like capture, enpassant
            move = Board.CollectMoveProperties(move);
            Board.ExecuteMove(move);
            Board.Shared.GameTurnCount++;

            GameHistory.Add(opponentMove);
            var moveData = _dataFactory.CreateMinimal(Board, !IsPlayerWhite);
            _replayPersistor.SaveMove(moveData);
        }

        /// <summary>
        /// Create search task with cancellation support
        /// </summary>
        public Task<string> CreateSearchTask(UciGoParameters parameters, Action<string> searchInfoUpdate, CancellationToken ct)
        {
            var searchParameters = new SearchParameters(parameters, searchInfoUpdate, ct);
            
            // Do merge class of parameters and action infoupdate
            var move = CreateNewMoveUci(searchParameters);

            // WriteLine($"bestmove {asd.Result.BestMove.ToCompactString()}");
            return Task.FromResult(move.ToCompactString());
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

            var moveBuilder = new MoveBuilder(_loggerFactory);
            moveBuilder.SetBoardAndSide(Board, IsPlayerWhite);
            moveBuilder.SetAlgorithmControl(_algorithmController);
            moveBuilder.SetSkipOpeningChecks(SkipOpeningChecks);
            moveBuilder.SetValidMoves(GameHistory);

            // Best move
            var aiMove = moveBuilder.BuildBestMove();

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
}
