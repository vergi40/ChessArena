using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Rook : PieceBase
    {
        public override double RelativeStrength { get; }
        
        public Rook(bool isWhite) : base(isWhite)
        {
            RelativeStrength = StrengthTable.Rook * Direction;
        }

        public Rook(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            RelativeStrength = StrengthTable.Rook * Direction;
        }

        public Rook(bool isWhite, string position) : base(isWhite, position)
        {
            RelativeStrength = StrengthTable.Rook * Direction;
        }

        private SingleMove CanMoveTo((int, int) target, Board board, bool validateBorders = false)
        {
            if (board.ValueAt(target) is PieceBase piece)
            {
                if (piece.IsWhite != IsWhite) return new SingleMove(CurrentPosition, target, true);
                else return null;
            }
            else return new SingleMove(CurrentPosition, target);
        }

        public override IEnumerable<SingleMove> Moves(Board board)
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

        public override PieceBase CreateCopy()
        {
            var piece = new Rook(IsWhite);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }
    }
}
