using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    public class Strategy
    {
        public bool IsPlayerWhite { get; }
        private DiagnosticsData _previous { get; set; }
        public GamePhase Phase { get; set; }
        public int SearchDepth { get; set; }

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

        // TODO as start parameters
        public int MaxDepth { get; set; } = 5;
        const int MinDepth = 2;

        public Strategy(bool isWhite, int? overrideMaxDepth)
        {
            IsPlayerWhite = isWhite;
            SearchDepth = 3;

            if (overrideMaxDepth != null) MaxDepth = overrideMaxDepth.Value;
        }

        /// <summary>
        /// Refresh data in beginning of each turn
        /// </summary>
        /// <param name="data"></param>
        /// <param name="turnCount"></param>
        public void Update(DiagnosticsData data, int turnCount)
        {
            _previous = data;
            TurnCount = turnCount;
        }

        public (int searchDepth, GamePhase gamePhase) DecideSearchDepth(DiagnosticsData previous, List<SingleMove> allMoves, Board board)
        {
            // Testing
            if (previous.OverrideSearchDepth != null) return (previous.OverrideSearchDepth.Value, previous.OverrideGamePhase);

            _previous = previous;

            AnalyzeGamePhase(allMoves.Count, board);
            return (SearchDepth, Phase);
        }


        private void AnalyzeGamePhase(int movePossibilities, Board board)
        {
            var powerPieces = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > StrengthTable.Pawn);

            if (powerPieces > 10)
            {
                AnalyzeHighPieceCountPhase(movePossibilities, board);
            }
            else if (powerPieces > 7)
            {
                AnalyzeMediumPieceCountPhase(movePossibilities, board);
            }
            else
            {
                AnalyzeLowPieceCountPhase(movePossibilities, board);
            }
        }

        /// <summary>
        /// Cap the search depth lower
        /// </summary>
        /// <param name="movePossibilities"></param>
        /// <param name="board"></param>
        private void AnalyzeHighPieceCountPhase(int movePossibilities, Board board)
        {
            // Game start
            if (_previous.TimeElapsed.Equals(TimeSpan.Zero))
            {
                SearchDepth = Math.Min(MaxDepth, 3);
                Phase = GamePhase.Start;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. Search depth {SearchDepth}. ");
                return;
            }
            
            const int criticalEvalCount = 300000;
            var approximateEvalCount = Math.Pow(movePossibilities, SearchDepth);
            Phase = GamePhase.Start;

            

            if ((_previous.TimeElapsed.TotalMilliseconds > 1500 || Math.Max(approximateEvalCount, _previous.EvaluationCount) > criticalEvalCount) && SearchDepth > MinDepth)
            {
                SearchDepth--;
                Diagnostics.AddMessage($"Decreased search depth to {SearchDepth}. ");
            }
            else if ((_previous.TimeElapsed.TotalMilliseconds < 200 || Math.Max(_previous.EvaluationCount, approximateEvalCount) < 50000) && SearchDepth < MaxDepth - 1)
            {
                // Only raise to max depth if possibilities are low enough
                SearchDepth++;
                Diagnostics.AddMessage($"Increased search depth to {SearchDepth}. ");
            }
        }

        private void AnalyzeMediumPieceCountPhase(int movePossibilities, Board board)
        {
            // 
            if (Phase != GamePhase.Middle && Phase != GamePhase.MidEndGame)
            {
                SearchDepth = Math.Min(MaxDepth, 4);
                Phase = GamePhase.Middle;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. Search depth {SearchDepth}. ");
                return;
            }
            const int criticalEvalCount = 400000;
            const int criticalCheckCount = 1000;

            var approximateEvalCount = Math.Pow(movePossibilities, SearchDepth);
            
            if ((_previous.TimeElapsed.TotalMilliseconds > 1500 || Math.Max(approximateEvalCount, _previous.EvaluationCount) > criticalEvalCount) && SearchDepth > MinDepth)
            {
                SearchDepth--;
                Diagnostics.AddMessage($"Decreased search depth to {SearchDepth}. ");
            }
            else if ((_previous.TimeElapsed.TotalMilliseconds < 200 || Math.Max(_previous.EvaluationCount, approximateEvalCount) < 50000) && SearchDepth < MaxDepth)
            {
                // Only raise to max depth if possibilities are low enough
                SearchDepth++;
                Diagnostics.AddMessage($"Increased search depth to {SearchDepth}. ");
            }

            if (Phase == GamePhase.Middle && _previous.CheckCount >= criticalCheckCount)
            {
                Phase = GamePhase.MidEndGame;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. ");
            }
            else if (Phase == GamePhase.MidEndGame && _previous.CheckCount < criticalCheckCount)
            {
                Phase = GamePhase.Middle;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. ");
            }
        }

        private void AnalyzeLowPieceCountPhase(int movePossibilities, Board board)
        {
            // 
            if (Phase != GamePhase.EndGame)
            {
                SearchDepth = Math.Min(MaxDepth, 5);
                Phase = GamePhase.EndGame;
                Diagnostics.AddMessage($"Game phase changed to {Phase.ToString()}. Search depth {SearchDepth}. ");
                return;
            }
            const int criticalHighEvalCount = 1000000;
            const int criticalLowEvalCount = 150000;
            //const int criticalCheckCount = 1000;

            var approximateEvalCount = Math.Pow(movePossibilities, SearchDepth);

            if ((_previous.TimeElapsed.TotalMilliseconds > 1500 || Math.Max(approximateEvalCount, _previous.EvaluationCount) > criticalHighEvalCount) && SearchDepth > MinDepth + 1)
            {
                SearchDepth--;
                Diagnostics.AddMessage($"Decreased search depth to {SearchDepth}. ");
            }
            else if ((_previous.TimeElapsed.TotalMilliseconds < 400 || Math.Max(_previous.EvaluationCount, approximateEvalCount) < criticalLowEvalCount) && SearchDepth < MaxDepth + 2)
            {
                // Only raise to max depth if possibilities are low enough
                SearchDepth++;
                Diagnostics.AddMessage($"Increased search depth to {SearchDepth}. ");
            }
        }
    }
}
