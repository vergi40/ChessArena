using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Knight : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }
        
        public Knight(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'N';
            RelativeStrength = StrengthTable.Knight * Direction;
        }

        public Knight(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'N';
            RelativeStrength = StrengthTable.Knight * Direction;
        }
        
        public override IEnumerable<SingleMove> Moves(Board board)
        {
            var cur = CurrentPosition;

            var move = CanMoveTo((cur.column - 1, cur.row + 2), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row + 2), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 2, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 2, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row - 2), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row - 2), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 2, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 2, cur.row + 1), board, true);
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy()
        {
            return new Knight(IsWhite, CurrentPosition);
        }
    }
}
