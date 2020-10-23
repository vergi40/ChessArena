using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class King : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }

        public King(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'K';
            RelativeStrength = StrengthTable.King * Direction;
        }

        public King(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'K';
            RelativeStrength = StrengthTable.King * Direction;
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
            return new King(IsWhite, CurrentPosition);
        }

        public King CreateKingCopy()
        {
            return new King(IsWhite, CurrentPosition);
        }
    }
}
