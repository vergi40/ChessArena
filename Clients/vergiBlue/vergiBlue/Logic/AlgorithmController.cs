using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CommonNetStandard.Client;
using CommonNetStandard.Interface;
using vergiBlue.Algorithms;
using vergiBlue.Algorithms.Basic;
using vergiBlue.Algorithms.IterativeDeepening;
using vergiBlue.Algorithms.Parallel;
using vergiBlue.Algorithms.PreMove;
using vergiBlue.BoardModel;

namespace vergiBlue.Logic
{
    interface IAlgorithm
    {
        SingleMove CalculateBestMove(BoardContext context);
    }

    /// <summary>
    /// Pre-algorithm infos
    /// </summary>
    public record TurnStartInfo(bool isWhiteTurn, IReadOnlyList<IMove> gameHistory, LogicSettings settings,
        DiagnosticsData previousMoveData, int? overrideSearchDepth = null)
    {
        public bool IsSearchDepthFixed => overrideSearchDepth != null;

        public int SearchDepthFixed
        {
            get
            {
                if (overrideSearchDepth != null)
                {
                    return overrideSearchDepth.Value;
                }

                return -1;
            }
        }
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


        public SingleMove GetBestMove(IBoard board, IReadOnlyList<SingleMove> validMoves)
        {
            // If still at opening phase, skip all unnecessarities
            if (_openingPhase && !board.Strategic.SkipOpeningChecks)
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

            // Select wanted algorithm and calculate
            

            var depthResult = _contextAnalyzer.DecideSearchDepth(validMoves, board);

            SetAlgorithm(depthResult.depth, depthResult.phase);
            Diagnostics.AddMessage($"Algo: {_algorithm.GetType().Name}");

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

            // Next should check it there is easy check mate in horizon
            var checkMateMove = FindCheckMate(depthResult.phase, context);
            if (checkMateMove != null) return checkMateMove;

            return _algorithm.CalculateBestMove(context);
        }

        private void SetAlgorithm(int searchDepth, GamePhase gamePhase)
        {
            if (_turnInfo.settings.UseParallelComputation)
            {
                // At the moment overrides all else
                _algorithm = new ParallelBasic();
            }
            else if (_turnInfo.settings.UseTranspositionTables && gamePhase != GamePhase.EndGame)
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

        private SingleMove? FindCheckMate(GamePhase gamePhase, BoardContext context)
        {
            var validMoves = context.ValidMoves;
            var board = context.CurrentBoard;

            // TODO BoardContext parameter
            if (gamePhase == GamePhase.MidEndGame || gamePhase == GamePhase.EndGame)
            {
                var isMaximizing = _contextAnalyzer.IsWhite;
                var checkMate = MoveResearch.ImmediateCheckMateAvailable(validMoves.ToList(), board, isMaximizing);
                if (checkMate != null) return checkMate;

                var twoTurnCheckMates = MoveResearch.CheckMateInTwoTurns(validMoves.ToList(), board, isMaximizing);
                if (twoTurnCheckMates.Count > 1)
                {
                    var newContext = context with { ValidMoves = twoTurnCheckMates.ToList() };
                    return _algorithm.CalculateBestMove(newContext);
                }
                else if (twoTurnCheckMates.Count > 0)
                {
                    return twoTurnCheckMates.First();
                }
            }

            return null;
        }

        /// <summary>
        /// For tests, visualization etc.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="validMoves"></param>
        /// <returns></returns>
        public EvaluationResult GetEvalForEachMove(IBoard board, IReadOnlyList<SingleMove> validMoves)
        {
            // TODO
            // Could be cool to give player hints about evaluations for next move
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Info needed for algorithm
    /// </summary>
    record BoardContext
    {
        public bool IsWhiteTurn { get; init; }
        public IBoard CurrentBoard { get; init; } = new Board();
        public IReadOnlyList<SingleMove> ValidMoves { get; init; } = new List<SingleMove>();

        public int MaxTimeMs { get; init; } = 5000;
        public int NominalSearchDepth { get; init; } = 5;
    }
}
