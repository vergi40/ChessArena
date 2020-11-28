using System;
using System.Collections.Generic;
using CommonNetStandard.Interface;

namespace vergiBlue.Pieces
{
    public abstract class PieceBase : IPiece
    {
        public bool IsWhite { get; }

        /// <summary>
        /// Upper case K, Q, R, N, B, P
        /// </summary>
        public abstract char Identity { get; }
        public abstract double RelativeStrength { get; }

        /// <summary>
        /// Sign of general direction. Can also be used to classify white as positive and black as negative value.
        /// </summary>
        public int Direction
        {
            get
            {
                if (IsWhite) return 1;
                return -1;
            }
        }

        public (int column, int row) CurrentPosition { get; set; }

        /// <summary>
        /// If using this, need to set position explicitly
        /// </summary>
        /// <param name="isWhite"></param>
        [Obsolete("Use constructor with position instead")]
        protected PieceBase(bool isWhite)
        {
            IsWhite = isWhite;
        }

        protected PieceBase(bool isWhite, (int column, int row) position)
        {
            IsWhite = isWhite;
            CurrentPosition = position;
        }

        protected PieceBase(bool isWhite, string position)
        {
            IsWhite = isWhite;
            CurrentPosition = position.ToTuple();
        }

        /// <summary>
        /// If target position is empty or has opponent piece, return SingleMove. If own piece or outside board, return null.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="board"></param>
        /// <param name="validateBorders"></param>
        /// <returns></returns>
        protected virtual SingleMove? CanMoveTo((int, int) target, Board board, bool validateBorders = false)
        {
            if (validateBorders && Logic.IsOutside(target)) return null;

            var valueAt = board.ValueAt(target);
            if (valueAt == null)
            {
                return new SingleMove(CurrentPosition, target);
            }
            else if (valueAt.IsWhite != IsWhite)
            {
                return new SingleMove(CurrentPosition, target, true);
            }
            return null;
        }

        /// <summary>
        /// Each move the piece can make in current board setting
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SingleMove> Moves(Board board);

        /// <summary>
        /// Copy needs to be made with the derived class constructor so type matches
        /// </summary>
        /// <returns></returns>
        public abstract PieceBase CreateCopy();

        protected IEnumerable<SingleMove> RookMoves(Board board)
        {
            var column = CurrentPosition.column;
            var row = CurrentPosition.row;

            // Up
            for (int i = row + 1; i < 8; i++)
            {
                var move = CanMoveTo((column, i), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // Down
            for (int i = row - 1; i >= 0; i--)
            {
                var move = CanMoveTo((column, i), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // Right
            for (int i = column + 1; i < 8; i++)
            {
                var move = CanMoveTo((i, row), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // Left
            for (int i = column - 1; i >= 0; i--)
            {
                var move = CanMoveTo((i, row), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
        }

        protected IEnumerable<SingleMove> BishopMoves(Board board)
        {
            var column = CurrentPosition.column;
            var row = CurrentPosition.row;

            // NE
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column + i, row + i), board, true);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // SE
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column + i, row - i), board, true);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // SW
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column - i, row - i), board, true);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // NW
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column - i, row + i), board, true);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
        }
    }
}
