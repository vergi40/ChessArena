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
        public King(bool isWhite, Board boardReference) : base(isWhite, boardReference)
        {
            RelativeStrength = StrengthTable.King * Direction;
        }

        protected override SingleMove CanMoveTo((int, int) target, bool validateBorders = false)
        {
            if (validateBorders && Logic.IsOutside(target)) return null;

            if (Board.ValueAt(target) == null)
            {
                return new SingleMove(CurrentPosition, target);
            }
            else if (Board.ValueAt(target).IsWhite != IsWhite)
            {
                return new SingleMove(CurrentPosition, target, true);
            }
            return null;
        }

        public override IEnumerable<SingleMove> Moves()
        {
            var cur = CurrentPosition;

            var move = CanMoveTo((cur.column + 1, cur.row), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row + 1), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row + 1), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row + 1), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row - 1), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row - 1), true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row - 1), true);
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy(Board newBoard)
        {
            var piece = new King(IsWhite, newBoard);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }

        public King CreateKingCopy(Board newBoard)
        {
            var piece = new King(IsWhite, newBoard);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }
    }
}
