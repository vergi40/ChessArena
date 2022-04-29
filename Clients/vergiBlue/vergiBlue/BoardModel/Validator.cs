using System;
using System.Linq;
using CommonNetStandard.Common;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public static class Validator
    {
        /// <summary>
        /// Validate move and color before it's executed
        /// </summary>
        /// <exception cref="InvalidMoveException"></exception>
        public static void ValidateMoveAndColor(IBoard board, ISingleMove move, bool isWhiteturn)
        {
            ValidateMove(board, move);

            // Check that is really valid move for current player
            var piece = board.ValueAtDefinitely(move.PrevPos);
            if (piece.IsWhite != isWhiteturn)
            {
                throw new InvalidMoveException(
                    $"Invalid move. Piece isWhite={piece.IsWhite}, isWhiteTurn={isWhiteturn}");
            }

            var target = board.ValueAt(move.NewPos);
            if (target != null && piece.IsWhite == target.IsWhite)
            {
                throw new InvalidMoveException(
                    $"Invalid move. Can't move to square containing same color piece. Target square {move.NewPos.ToAlgebraic()} piece isWhite={piece.IsWhite}, target isWhite={target.IsWhite}");
            }
        }

        /// <summary>
        /// Validate any color move before it's executed
        /// </summary>
        /// <exception cref="InvalidMoveException"></exception>
        public static void ValidateMove(IBoard board, ISingleMove move)
        {
            if (move == null)
            {
                throw new ArgumentException($"Move can't be null.");
            }

            if (IsOutside(move.PrevPos))
            {
                throw new InvalidMoveException($"Invalid move. Start position outside of board {move.PrevPos}");
            }
            if (IsOutside(move.NewPos))
            {
                throw new InvalidMoveException($"Invalid move. End position outside of board {move.NewPos}");
            }
            if (board.ValueAt(move.PrevPos) == null)
            {
                throw new InvalidMoveException($"Invalid move. No piece at start pos {move.PrevPos.ToAlgebraic()}");
            }

            // Check that is really valid move for current player
            var piece = board.ValueAtDefinitely(move.PrevPos);
            var validMoves = board.MoveGenerator.ValidMovesQuick(piece.IsWhite);
            if (!validMoves.Any(m => m.EqualPositions(move)))
            {
                throw new InvalidMoveException(
                    $"Invalid move. Cannot move {piece.Identity} from {move.PrevPos} to {move.NewPos}. " +
                    $"Valid moves: {string.Join(", ", piece.Moves(board))}");
            }
        }

        public static bool IsOutside((int, int) target)
        {
            if (target.Item1 < 0 || target.Item1 > 7 || target.Item2 < 0 || target.Item2 > 7)
                return true;
            return false;
        }

        /// <summary>
        /// Prerequisite: King not in check
        /// </summary>
        public static bool IsLegalMove(SingleMove move, IBoard board, IPiece piece, (int column, int row) kingLocation)
        {
            // https://chess.stackexchange.com/a/16901
            // Simple and neat steps

            var forWhite = piece.IsWhite;
            
            // TODO boardfactory light
            var newBoard = BoardFactory.CreateFromMove(board, move);

            // Modern engines
            // en passant tricky. test whether king is attacked after move made
            if (move.EnPassant)
            {
                if (newBoard.MoveGenerator.IsSquareCurrentlyAttacked(!forWhite, kingLocation))
                {
                    return false;
                }

                return true;
            }
            
            // king: test if king is attacked after move is made
            if (piece.Identity == 'K')
            {
                if (move.Castling) throw new ArgumentException("Logical error: Castling moves are generated elsewhere");
                if (newBoard.MoveGenerator.IsSquareCurrentlyAttacked(!forWhite, move.NewPos))
                {
                    return false;
                }

                return true;
            }

            // others: if not pinned -> move ok. if pinned -> if it's moving along the attack ray, ok
            foreach (var slider in board.MoveGenerator.GetOrCreateSliders(!forWhite))
            {
                if (slider.AttackLine.Contains(move.PrevPos))
                {
                    // pinned found
                    if (!slider.Pin.Equals(move.PrevPos))
                    {
                        // Assert
                        throw new ArgumentException(
                            $"Logical error: slider had wrong pinned piece. " +
                            $"Slider pin at {slider.Pin.ToAlgebraic()}. Move prevpos at {move.PrevPos.ToAlgebraic()}");
                    }

                    // Ok only if moving along slider or capturing attacker
                    if (!slider.AttackLine.Contains(move.NewPos) && move.NewPos != slider.Attacker)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
