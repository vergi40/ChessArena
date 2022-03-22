using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Interface;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.PreMove
{
    /// <summary>
    /// Analyze next move context information
    /// </summary>
    internal class PreMoveAnalyzer
    {
        public bool IsWhite { get; }
        
        public GamePhase Phase { get; set; }
        
        private int _previousDepth { get; set; } = 5;
        private GamePhase _previousPhase { get; set; } = GamePhase.Start;

        private DepthController _depthController { get; } = new DepthController();

        private TurnStartInfo _turnInfo { get; set; } =
            new(false, new List<IMove>(), new LogicSettings(), new DiagnosticsData());

        private int? _overrideGameMaxDepth { get; }


        public PreMoveAnalyzer(bool isWhite, int? overrideGameMaxDepth)
        {
            IsWhite = isWhite;
            _overrideGameMaxDepth = overrideGameMaxDepth;
        }

        /// <summary>
        /// Refresh data in beginning of each turn
        /// </summary>
        public void TurnStartUpdate(TurnStartInfo turnInfo)
        {
            _turnInfo = turnInfo;
        }

        public (int depth, GamePhase phase) DecideSearchDepth(IReadOnlyList<SingleMove> allMoves, IBoard board)
        {
            var gamePhase = DecideGamePhaseTemp(allMoves.Count, board);

            if (_turnInfo.IsSearchDepthFixed)
            {
                _previousDepth = _turnInfo.SearchDepthFixed;
                return (_turnInfo.SearchDepthFixed, gamePhase);
            }
            // Track previous depth and time. Only +1 or -1 if enough points tick the box
            // 
            // Decide game phase
            // Min&max depth from settings (algorithm type) and game phase - update each turn
            // Estimate time by amount of possible moves to board
            // Compare estimate time to previous time and possibilities

            // Clamp()
            // PrintChanges(newDepth) - if changes
            var (minDepth, maxDepth) = GetDepthMinMax(_turnInfo, board, gamePhase);
            if (_overrideGameMaxDepth != null)
            {
                maxDepth = _overrideGameMaxDepth.Value;
                minDepth = Math.Min(minDepth, _overrideGameMaxDepth.Value);
            }

            var depthEstimate = _depthController.GetDepthEstimate(allMoves, board, _turnInfo, maxDepth);

            var resultDepth = (int)Math.Round(depthEstimate);
            resultDepth = Math.Clamp(resultDepth, minDepth, maxDepth);

            PrintChanges(resultDepth, depthEstimate, Phase);
            _previousDepth = resultDepth;
            _previousPhase = Phase;

            return (resultDepth, gamePhase);
        }

        private void PrintChanges(int depth, double estimate, GamePhase phase)
        {
            if (depth != _previousDepth)
            {
                Diagnostics.AddMessage($"Search depth update {_previousDepth} -> {depth} (estimate {estimate:F2}");
            }

            if (phase != _previousPhase)
            {
                Diagnostics.AddMessage($"Game phase update {_previousPhase} -> {phase}");
            }
        }

        private (int minDepth, int maxDepth) GetDepthMinMax(TurnStartInfo turnInfo, IBoard board, GamePhase phase)
        {
            // NOTE: take phase into account
            var settings = turnInfo.settings;

            if (settings.UseTranspositionTables && settings.UseIterativeDeepening)
            {
                var tempOffset = -1;
                
                // Cool new switch structure
                var powerPieceCount = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);
                var max = powerPieceCount switch
                {
                    > 7 => 7 + tempOffset,
                    > 6 => 8 + tempOffset,
                    > 4 => 9 + tempOffset,
                    _ => 10 + tempOffset
                };

                return (3, max);
            }

            if (settings.UseTranspositionTables || settings.UseIterativeDeepening)
            {
                var tempOffset = -2;

                var powerPieceCount = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);
                var max = powerPieceCount switch
                {
                    > 9 => 7 + tempOffset,
                    > 6 => 8 + tempOffset,
                    > 4 => 9 + tempOffset,
                    _ => 10 + tempOffset
                };

                return (3, max);
            }

            if (settings.UseParallelComputation)
            {
                return (3, 6);
            }

            return (3, 5);
        }

        /// <summary>
        /// TODO Refactor out side effects (publis Phase)
        /// </summary>
        /// <param name="movePossibilities"></param>
        /// <param name="board"></param>
        /// <returns></returns>
        public GamePhase DecideGamePhaseTemp(int movePossibilities, IBoard board)
        {
            var powerPieces = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);

            if (powerPieces > 10)
            {
                Phase = GamePhase.Start;
            }
            else if (powerPieces > 7)
            {
                AnalyzeMediumPieceCountPhaseTemp();
            }
            else
            {
                AnalyzeLowPieceCountPhaseTemp();
            }

            return Phase;
        }
        
        private void AnalyzeMediumPieceCountPhaseTemp()
        {
            // 
            if (Phase != GamePhase.Middle && Phase != GamePhase.MidEndGame)
            {
                Phase = GamePhase.Middle;
                return;
            }
            const int criticalEvalCount = 400000;
            const int criticalCheckCount = 1000;
            var previous = _turnInfo.previousMoveData;

            if (Phase == GamePhase.Middle && previous.CheckCount >= criticalCheckCount)
            {
                Phase = GamePhase.MidEndGame;
            }
            else if (Phase == GamePhase.MidEndGame && previous.CheckCount < criticalCheckCount)
            {
                Phase = GamePhase.Middle;
            }
        }

        private void AnalyzeLowPieceCountPhaseTemp()
        {
            // 
            if (Phase != GamePhase.EndGame)
            {
                Phase = GamePhase.EndGame;
            }
        }
    }
}
