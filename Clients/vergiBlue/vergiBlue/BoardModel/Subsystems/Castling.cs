using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems
{
    public static class Castling
    {
        public static (bool leftOk, bool rightOk) PreValidation(IBoard board, PieceBase king)
        {
            var left = false;
            var right = false;
            var (leftDone, rightDone) = CastlingDoneOrMissed(board.Strategic, king.IsWhite);

            if (!leftDone)
            {
                left = PreValidationLeft(board, king);
            }

            if (!rightDone)
            {
                right = PreValidationRight(board, king);
            }

            return (left, right);
        }

        private static (bool leftDone, bool rightDone) CastlingDoneOrMissed(StrategicData strategic, bool forWhite)
        {
            var left = false;
            var right = false;
            if (forWhite)
            {
                if (!strategic.WhiteLeftCastlingValid)
                {
                    left = true;
                }
                if (!strategic.WhiteRightCastlingValid)
                {
                    right = true;
                }
            }
            else
            {
                if (!strategic.BlackLeftCastlingValid)
                {
                    left = true;
                }
                if (!strategic.BlackRightCastlingValid)
                {
                    right = true;
                }
            }

            return (left, right);
        }

        public static int GetRow(bool isWhite)
        {
            return isWhite ? 0 : 7;
        }

        private static bool PreValidationLeft(IBoard board, PieceBase king)
        {
            var row = GetRow(king.IsWhite);

            // Castling pieces are intact
            if(king.CurrentPosition != (4, row))
            {
                return false;
            }
            var rook = board.ValueAt((0, row));
            if (rook == null || rook.Identity != 'R') return false;

            // No other pieces on the way
            if (board.ValueAt((1, row)) != null) return false;
            if (board.ValueAt((2, row)) != null) return false;
            if (board.ValueAt((3, row)) != null) return false;
            
            return true;
        }

        private static bool PreValidationRight(IBoard board, PieceBase king)
        {
            var row = GetRow(king.IsWhite);

            // Castling pieces are intact
            if (king.CurrentPosition != (4, row))
            {
                return false;
            }
            var rook = board.ValueAt((7, row));
            if (rook == null || rook.Identity != 'R') return false;

            // No other pieces on the way
            if (board.ValueAt((5, row)) != null) return false;
            if (board.ValueAt((6, row)) != null) return false;

            return true;
        }

        public static bool TryCreateLeftCastling(PieceBase king, HashSet<(int column, int row)> attackSquares, out SingleMove move)
        {
            var row = GetRow(king.IsWhite);
            move = SingleMoveFactory.CreateCastling((4, row), (2, row));

            foreach (var neededSquare in new[] { (2, row), (3, row), (4, row) })
            {
                if (attackSquares.Contains(neededSquare)) return false;
            }

            return true;
        }

        public static bool TryCreateRightCastling(PieceBase king, HashSet<(int column, int row)> attackSquares, out SingleMove move)
        {
            var row = GetRow(king.IsWhite);
            move = SingleMoveFactory.CreateCastling((4, row), (6, row));

            foreach (var neededSquare in new[] { (4, row), (5, row), (6, row) })
            {
                if (attackSquares.Contains(neededSquare)) return false;
            }

            return true;
        }

        /// <summary>
        /// Do during each move execution.
        /// Check if castling pieces are still in place
        /// </summary>
        public static void UpdateStatusForNonCastling(IBoard board, PieceBase pieceMoving, in ISingleMove move)
        {
            // If moving rook or king, update strategic
            // If capturing opponent rook, update opponent
            if (pieceMoving.Identity != 'K' && pieceMoving.Identity != 'R' && !move.Capture)
            {
                // Quick return
                return;
            }

            var isWhite = pieceMoving.IsWhite;
            var strategic = board.Strategic;

            var (leftDone, rightDone) = CastlingDoneOrMissed(strategic, isWhite);
            if (leftDone && rightDone)
            {
                // Quick return
                return;
            }

            if (pieceMoving.Identity == 'K')
            {
                // Revoke all
                strategic.RevokeCastlingFor(isWhite, true, true);
                return;
            }
            if (pieceMoving.Identity == 'R')
            {
                var row = GetRow(isWhite);
                if (move.PrevPos == (0, row))
                {
                    strategic.RevokeCastlingFor(isWhite, true, false);
                    return;
                }
                if (move.PrevPos == (7, row))
                {
                    strategic.RevokeCastlingFor(isWhite, false, true);
                    return;
                }
            }

            // TODO is it even necessary to update opponent castling?
            if (move.Capture)
            {
                var row = GetRow(!isWhite);
                if (move.NewPos == (0, row))
                {
                    strategic.RevokeCastlingFor(!isWhite, true, false);
                    return;
                }
                if (move.NewPos == (7, row))
                {
                    strategic.RevokeCastlingFor(!isWhite, false, true);
                }
            }
        }
    }
}
