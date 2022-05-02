using System.Collections.Generic;
using CommonNetStandard.Interface;
using vergiBlue.Algorithms;
using vergiBlue.Algorithms.Basic;
using vergiBlue.Algorithms.IterativeDeepening;
using vergiBlue.Algorithms.Parallel;
using vergiBlue.Algorithms.PreMove;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;

namespace vergiBlue.Logic
{
    interface IAlgorithm
    {
        SingleMove CalculateBestMove(BoardContext context);
    }

    /// <summary>
    /// Main entry point finding moves.
    /// "Context" in strategy-pattern
    /// Updates current algorithm strategy based on latest Update()-calls
    /// </summary>
    class AlgorithmController
    {
        /// <summary>
        /// TODO if want to support calculating also opponent, this needs to update every opponent turn
        /// Now only updated at <see cref="TurnStartUpdate"/>
        /// </summary>
        private bool _currentTurnIsWhite { get; set; }

        /// <summary>
        /// Current strategy used. Can change each turn
        /// </summary>
        private IAlgorithm _algorithm = new MiniMaxBasic();
        private List<SingleMove> _moveHistory { get; set; } = new List<SingleMove>();

        private OpeningLibrary _openings { get; } = new OpeningLibrary();
        private bool _openingPhase = true;

        private PreMoveAnalyzer _contextAnalyzer { get; set; } = new(false, null);

        private TurnStartInfo _turnInfo { get; set; } =
            new(false, new List<IMove>(), new LogicSettings(), new DiagnosticsData());

        // TODO quick hack
        private SearchParameters? _searchParameters { get; set; }

        // time limit
        // max depth
        // min depth
        public void Initialize(bool isWhite, int? overrideMaxDepth = null)
        {
            _contextAnalyzer = new PreMoveAnalyzer(isWhite, overrideMaxDepth);
        }

        public void TurnStartUpdate(TurnStartInfo turnInfo)
        {
            _turnInfo = turnInfo;
            UpdateGameHistory(_turnInfo.gameHistory);
            
            _currentTurnIsWhite = turnInfo.isWhiteTurn;
            _contextAnalyzer.TurnStartUpdate(turnInfo);
        }

        public void TurnStartUpdate(SearchParameters parameters)
        {
            _turnInfo = parameters.TurnStartInfo;
            UpdateGameHistory(_turnInfo.gameHistory);

            _currentTurnIsWhite = _turnInfo.isWhiteTurn;
            _contextAnalyzer.TurnStartUpdate(_turnInfo);

            _searchParameters = parameters;
        }

        private void UpdateGameHistory(IReadOnlyList<IMove> gameHistory)
        {
            var moves = new List<SingleMove>();
            foreach (var move in gameHistory)
            {
                moves.Add(new SingleMove(move));
            }

            _moveHistory = moves;
            // Note: could do some checks about opponent last move decisions
        }
        
        // Update target total time
        // Update game phase


        public SingleMove GetBestMove(IBoard board, IReadOnlyList<SingleMove> validMoves, bool skipOpeningChecks)
        {
            // If still at opening phase, skip all unnecessarities
            if (!skipOpeningChecks && _openingPhase)
            {
                var openingMove = _openings.NextMove(_moveHistory);
                if (openingMove != null)
                {
                    return openingMove;
                }
                else
                {
                    // No more opening sequences
                    _openingPhase = false;
                }
            }

            if (_searchParameters == null)
            {
                return GetBestMoveOld(board, validMoves);
            }
            else
            {
                return GetBestMoveUci(board, validMoves, _searchParameters);
            }
        }

        private SingleMove GetBestMoveOld(IBoard board, IReadOnlyList<SingleMove> validMoves)
        {
            // Select wanted algorithm and calculate
            var depthResult = _contextAnalyzer.DecideSearchDepth(validMoves, board);

            SetAlgorithm(depthResult.depth, depthResult.phase);

            var context = new BoardContext()
            {
                // Calculating for ai - so this should always match?
                // TODO what if we want to calculate for any player?
                IsWhiteTurn = _currentTurnIsWhite,
                CurrentBoard = board,
                ValidMoves = validMoves,
                NominalSearchDepth = depthResult.depth,
                MaxTimeMs = _turnInfo.settings.TimeLimitInMs
            };

            Collector.AddCustomMessage($"Algorithm: {_algorithm.GetType().Name}");
            return _algorithm.CalculateBestMove(context);
        }

        private SingleMove GetBestMoveUci(IBoard board, IReadOnlyList<SingleMove> validMoves, SearchParameters parameters)
        {
            // WIP if depth < 3?
            var algorithm = new IDWithUciParameters();
            
            // WIP 
            var maxDepth = 10;

            var context = new BoardContext()
            {
                // Calculating for ai - so this should always match?
                // TODO what if we want to calculate for any player?
                IsWhiteTurn = _currentTurnIsWhite,
                CurrentBoard = board,
                ValidMoves = validMoves,
                NominalSearchDepth = maxDepth,
                MaxTimeMs = _turnInfo.settings.TimeLimitInMs
            };

            Collector.AddCustomMessage($"Algorithm: {_algorithm.GetType().Name}");

            return algorithm.CalculateBestMove(context, parameters);
        }

        private void SetAlgorithm(int searchDepth, GamePhase gamePhase)
        {
            if (_turnInfo.settings.UseParallelComputation)
            {
                // At the moment overrides all else
                _algorithm = new ParallelBasic();
            }
            else if (_turnInfo.settings.UseTranspositionTables)
            {
                // Transpositions is still the most WIP of algorithms.
                // Seems like messes up particularly endgame calculations
                if (_turnInfo.settings.UseIterativeDeepening && searchDepth >= 3)
                {
                    _algorithm = new IDWithTranspositions();
                }
                else
                {
                    _algorithm = new MiniMaxWithTranspositions();
                }
            }
            else
            {
                if (_turnInfo.settings.UseIterativeDeepening && searchDepth >= 3)
                {
                    _algorithm = new IDBasic();
                }
                else
                {
                    _algorithm = new MiniMaxBasic();
                }
            }
        }
    }

    /// <summary>
    /// Info needed for algorithm
    /// </summary>
    public record BoardContext
    {
        public bool IsWhiteTurn { get; init; }
        public IBoard CurrentBoard { get; init; } = new Board();
        public IReadOnlyList<SingleMove> ValidMoves { get; init; } = new List<SingleMove>();

        public int MaxTimeMs { get; init; } = 5000;
        public int NominalSearchDepth { get; init; } = 5;
    }
}
