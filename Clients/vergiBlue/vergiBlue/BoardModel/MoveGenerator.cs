﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Algorithms;

namespace vergiBlue.BoardModel
{
    public class MoveGenerator
    {
        private IBoard _board { get; }

        public MoveGenerator(IBoard board)
        {
            _board = board;
        }

        /// <summary>
        /// Find every possible move for every piece for given color.
        /// </summary>
        public IList<SingleMove> Moves(bool forWhite, bool orderMoves, bool kingInDanger = false)
        {
            IList<SingleMove> list = new List<SingleMove>();

            // In tests king might not exist
            var king = _board.KingLocation(forWhite);
            if (king != null)
            {
                foreach (var castling in king.CastlingMoves(_board))
                {
                    list.Add(castling);
                }
            }

            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    list.Add(singleMove);
                }
            }

            // TODO modify to enumerable and do these higher
            if (orderMoves) return MoveOrdering.SortMovesByEvaluation(list, _board, forWhite);
            else return MoveOrdering.SortMovesByGuessWeight(list, _board, forWhite);
        }

        /// <summary>
        /// No sorting
        /// </summary>
        public IEnumerable<SingleMove> MovesWithoutCastling(bool forWhite, bool kingInDanger = false)
        {
            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    yield return singleMove;
                }
            }
        }

        public IList<SingleMove> MovesWithTranspositionOrder(bool forWhite, bool kingInDanger = false)
        {
            // Priority moves like known cutoffs
            var priorityList = new List<SingleMove>();
            var otherList = new List<SingleMove>();
            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    // Check if move has transposition data
                    // Maximizing player needs lower bound moves
                    // Minimizing player needs upper bound moves
                    var transposition = _board.Shared.Transpositions.GetTranspositionForMove(_board, singleMove);
                    if (transposition != null)
                    {
                        if ((forWhite && transposition.Type == NodeType.LowerBound) ||
                            (!forWhite && transposition.Type == NodeType.UpperBound))
                        {
                            Diagnostics.IncrementPriorityMoves();
                            priorityList.Add(singleMove);
                            continue;
                        }
                    }
                    otherList.Add(singleMove);
                }
            }

            priorityList.AddRange(MoveOrdering.SortMovesByGuessWeight(otherList, _board, forWhite));
            return priorityList;
        }
    }
}
