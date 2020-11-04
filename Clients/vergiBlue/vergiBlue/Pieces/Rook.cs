using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Rook : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }

        public Rook(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'R';
            RelativeStrength = StrengthTable.Rook * Direction;
        }

        public Rook(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'R';
            RelativeStrength = StrengthTable.Rook * Direction;
        }

        public override IEnumerable<SingleMove> Moves(Board board)
        {
            return RookMoves(board);
        }

        public override PieceBase CreateCopy()
        {
            var piece = new Rook(IsWhite, CurrentPosition);
            return piece;
        }
    }
}
