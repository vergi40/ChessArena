using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class King : PieceBase
    {
        public override double RelativeStrength { get; }
        public King(bool isWhite) : base(isWhite)
        {
            RelativeStrength = StrengthTable.King * Direction;
        }

        public King(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            RelativeStrength = StrengthTable.King * Direction;
        }

        public King(bool isWhite, string position) : base(isWhite, position)
        {
            RelativeStrength = StrengthTable.King * Direction;
        }

        private SingleMove CanMoveTo((int, int) target, Board board, bool validateBorders = false)
        {
            if (validateBorders && Logic.IsOutside(target)) return null;

            if (board.ValueAt(target) == null)
            {
                return new SingleMove(CurrentPosition, target);
            }
            else if (board.ValueAt(target).IsWhite != IsWhite)
            {
                return new SingleMove(CurrentPosition, target, true);
            }
            return null;
        }

        public override IEnumerable<SingleMove> Moves(Board board)
        {
            var cur = CurrentPosition;

            var move = CanMoveTo((cur.column + 1, cur.row), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row - 1), board, true);
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy()
        {
            var piece = new King(IsWhite);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }

        public King CreateKingCopy()
        {
            var piece = new King(IsWhite);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }
    }
}
