using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard.Client;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using log4net;
using vergiBlue.Algorithms;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;

namespace vergiBlue.Logic
{
    public class Logic : LogicBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Logic));

        // Game strategic variables
        public IMove? LatestOpponentMove { get; set; }
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
            _logger.Info("Logic initialized");
        }

        /// <summary>
        /// For tests. Start board known. Test environment handles initializations.
        /// </summary>
        public Logic(bool isPlayerWhite, IBoard board, int? overrideMaxDepth = null) : base(isPlayerWhite)
        {
            _algorithmController.Initialize(isPlayerWhite, overrideMaxDepth);
            Board = BoardFactory.CreateClone(board);
            Board.Shared.Testing = true;
            _logger.Info("Logic initialized");
        }

        public Logic(IGameStartInformation startInformation, int? overrideMaxDepth = null, IBoard? overrideBoard = null) : base(startInformation.WhitePlayer)
        {
            _algorithmController.Initialize(startInformation.WhitePlayer, overrideMaxDepth);
            if (overrideBoard != null)
            {
                Board = BoardFactory.CreateClone(overrideBoard);
            }
            else
            {
                Board = BoardFactory.CreateDefault();
            }

            _logger.Info("Logic initialized");

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

        }


        public void SetBoard(string startPosOrFenBoard, List<string> moves)
        {
            bool isWhite;
            if (startPosOrFenBoard.Equals("startpos"))
            {
                // TODO separate static system init
                Board = BoardFactory.CreateDefault();
                isWhite = true;
            }
            else
            {
                Board = BoardFactory.CreateFromFen(startPosOrFenBoard, out isWhite);
                IsPlayerWhite = isWhite;
            }

            Board.Strategic.SkipOpeningChecks = true;

            foreach (var move in moves)
            {
                var tempMove = SingleMoveFactory.Create(move);
                var fullMove = Board.CollectMoveProperties(tempMove);
                Board.ExecuteMove(fullMove);
                isWhite = !isWhite;
            }

            IsPlayerWhite = isWhite;
            Board.InitializeSubSystems();
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
            _logger.Info("Starting create move operations...");
            var bestMove = CreateNewMove();
            var inner = bestMove.Move;
            _logger.Info($"Created move {inner.StartPosition}{inner.EndPosition}{SingleMove.ConvertPromotion(inner.PromotionResult)}");
            
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
            var aiMove = _algorithmController.GetBestMove(Board, validMoves);
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

            _logger.Info($"{validMoves.Count} valid moves found: {string.Join(", ", validMoves)}.");
            Collector.AddCustomMessage($"{validMoves.Count} valid moves found.");
            return validMoves;
        }

        public sealed override void ReceiveMove(IMove? opponentMove)
        {
            LatestOpponentMove = opponentMove ?? throw new ArgumentException($"Received null move. Error or game has ended.");
            _logger.Info(
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

        /// <summary>
        /// Create search task with cancellation support
        /// </summary>
        public Task<SearchResult> CreateSearchTask(UciGoParameters parameters, Action<string> searchInfoUpdate, CancellationToken ct)
        {
            searchInfoUpdate("test");

            var searchParameters = new SearchParameters()
            {
                UciParameters = parameters,
                WriteToOutputAction = searchInfoUpdate,
                StopSearchToken = ct
            };

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
            var aiMove = _algorithmController.GetBestMove(Board, validMoves);
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

            var historyMove = new PlayerMoveImplementation(moveWithData.ToInterfaceMove(),
                analyticsOutput);
            GameHistory.Add(historyMove.Move);
            return moveWithData;
        }
    }

    public class SearchResult
    {
        public ISingleMove BestMove { get; }

        public SearchResult(ISingleMove bestMove)
        {
            BestMove = bestMove;
        }
    }

    public class SearchParameters
    {
        public UciGoParameters UciParameters { get; set; }
        public Action<string> WriteToOutputAction { get; set; }
        public TurnStartInfo TurnStartInfo { get; set; }

        public CancellationToken StopSearchToken { get; set; }
    }
}
