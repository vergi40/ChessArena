﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    public class Strategy
    {
        /// <summary>
        /// Try to do calculations in same time scale as target. Milliseconds.
        /// Can change for each turn, based on how much total time there is left. 
        /// </summary>
        public int TargetTime { get; set; } = 5000;
        
        public bool IsPlayerWhite { get; }
        private DiagnosticsData _previous { get; set; } = new DiagnosticsData();
        public GamePhase Phase { get; set; }
        public int SearchDepth { get; set; }

        private bool _useTranspositionTables { get; } = false;

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

        public Strategy(bool isWhite, int? overrideMaxDepth, bool useTranspositionTables)
        {
            IsPlayerWhite = isWhite;
            SearchDepth = 5;

            if (useTranspositionTables)
            {
                _useTranspositionTables = true;
                //SearchDepth = 6;
                SearchDepth = 5;
            }

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

        public int DecideSearchDepth(DiagnosticsData previous, List<SingleMove> allMoves, Board board)
        {
            _previous = previous;
            var previousDepth = SearchDepth;

            // Previous was opening move from database
            if (previous.EvaluationCount == 0 && previous.CheckCount == 0)
            {
                Diagnostics.AddMessage($"Using search depth {SearchDepth}.");
                return SearchDepth;
            }

            // Logic needs to be rewritten when using transposition tables, because of how much they affect speed.
            if (_useTranspositionTables)
            {
                SearchDepth = GetMaxDepthForCurrentBoardWithTranspositions(board);
                Diagnostics.AddMessage($"Using search depth {SearchDepth}.");
                return SearchDepth;
            }

            var maxDepth = GetMaxDepthForCurrentBoard(board);
            SearchDepth = maxDepth;
            // TODO Skip until correlated to new speeds
            Diagnostics.AddMessage($"Using search depth {SearchDepth}.");
            return maxDepth;

            var previousEstimate = 0;
            for (int i = 3; i <= maxDepth; i++)
            {
                var estimation = AssessTimeForMiniMaxDepth(i, allMoves, board, previousDepth, previous);
                
                // Use 50% tolerance for target time
                if (estimation > TargetTime * 1.50)
                {
                    SearchDepth = i - 1;
                    Diagnostics.AddMessage($"Using search depth {SearchDepth}. Time estimation was {previousEstimate} ms.");
                    return SearchDepth;
                }
                else previousEstimate = estimation;
            }

            SearchDepth = maxDepth;
            Diagnostics.AddMessage($"Failed to assess - using search depth {SearchDepth}. ");
            return maxDepth;
            //AnalyzeGamePhase(allMoves.Count, board);
            //return SearchDepth;
        }

        private int AssessTimeForMiniMaxDepth(int depth, IList<SingleMove> availableMoves, Board board,
            int previousDepth, DiagnosticsData previousData)
        {
            var previousTime = previousData.TimeElapsed.TotalMilliseconds;
            var prevousEvalCount = previousData.EvaluationCount + previousData.CheckCount;
            
            // Need a equation to model evalcount <-> time
            // movecount ^ depth = evalcount
            // evalcount correlates to time

            // Estimated evaluation speed. moves per millisecond
            // Speed = count / time
            var previousEvalSpeed = prevousEvalCount / previousTime;
            
            var evalCount = Math.Pow(Math.Max(availableMoves.Count, 6), depth);
            // lets say 20 ^ 5 = 3 200 000
            
            // Estimated total time with previous speed
            // Time = count /  speed
            var timeEstimate = evalCount / previousEvalSpeed;

            // Increase estimate proportionally depending of piece count
            // All pieces -> use as is
            // 1 piece -> 1/16 of the time
            var powerPieces = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);
            var factor = (double)powerPieces / 16;
            //var factor = 0.5 + (double)powerPieces / 32;

            return (int) (timeEstimate * factor);
        }
        private int GetMaxDepthForCurrentBoard(Board board)
        {
            var tempOffset = -1;

            var powerPieces = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);
            if (powerPieces > 9) return 6 + tempOffset;
            if (powerPieces > 7) return 7 + tempOffset;
            if (powerPieces > 6) return 8 + tempOffset;
            if (powerPieces > 4) return 9 + tempOffset;
            return 10 + tempOffset;
        }

        private int GetMaxDepthForCurrentBoardWithTranspositions(Board board)
        {
            var tempOffset = -1;
            
            var powerPieces = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);
            if (powerPieces > 9) return 6 + tempOffset;
            if (powerPieces > 7) return 7 + tempOffset;
            if (powerPieces > 6) return 8 + tempOffset;
            if (powerPieces > 4) return 9 + tempOffset;
            return 10 + tempOffset;
        }


        private void AnalyzeGamePhase(int movePossibilities, Board board)
        {
            var powerPieces = board.PieceList.Count(p => Math.Abs(p.RelativeStrength) > PieceBaseStrength.Pawn);

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
