using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard.Common;

namespace vergiBlue.BoardModel
{
    public static class Validator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidMoveException"></exception>
        public static void ValidateMove(IBoard board, SingleMove move)
        {
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
            var validMoves = board.MoveGenerator.MovesQuick(piece.IsWhite, true);
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
    }
}
