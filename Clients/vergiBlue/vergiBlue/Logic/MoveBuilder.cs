using CommonNetStandard.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using vergiBlue.Algorithms;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;

namespace vergiBlue.Logic
{
    internal class MoveBuilder
    {
        private readonly ILogger _logger;

        private bool IsPlayerWhite { get; set; }
        private IBoard Board { get; set; } = BoardFactory.CreateEmptyBoard();
        private bool SkipOpeningChecks { get; set; }

        private List<SingleMove> ValidMoves { get; set; } = new();
        private AlgorithmController _algorithmController { get; set; } = new AlgorithmController();

        public MoveBuilder(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MoveBuilder>();
        }

        public SingleMove BuildBestMove()
        {
            var move = _algorithmController.GetBestMove(Board, ValidMoves, SkipOpeningChecks);
            Validator.ValidateMoveAndColor(Board, move, IsPlayerWhite);
            return move;
        }


        public void SetBoardAndSide(IBoard board, bool isPlayerWhite)
        {
            Board = board;
            IsPlayerWhite = isPlayerWhite;
        }

        public void SetAlgorithmControl(AlgorithmController algorithmController)
        {
            _algorithmController = algorithmController;

        }

        public void SetSkipOpeningChecks(bool skipOpeningChecks) { SkipOpeningChecks = skipOpeningChecks; }

        /// <summary>
        /// Get all available moves and do necessary ordering & filtering
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void SetValidMoves(IList<IMove> gameHistory)
        {
            var isMaximizing = IsPlayerWhite;
            var validMoves = Board.MoveGenerator.MovesWithOrdering(isMaximizing, true, true).ToList();

            if (MoveHistory.IsLeaningToDraw(gameHistory))
            {
                // Repetition
                // Take 4th from the end of list
                var repetionMove = gameHistory[^4];
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

            _logger.LogDebug($"{validMoves.Count} valid moves found: {string.Join(", ", movesSorted)}.");
            Collector.AddCustomMessage($"{validMoves.Count} valid moves found.");
            ValidMoves = validMoves;
        }
    }
}
