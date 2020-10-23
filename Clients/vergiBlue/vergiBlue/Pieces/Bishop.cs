using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Bishop : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }
        
        public Bishop(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'B';
            RelativeStrength = StrengthTable.Bishop * Direction;
        }

        public Bishop(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'B';
            RelativeStrength = StrengthTable.Pawn * Direction;
        }

        public override IEnumerable<SingleMove> Moves(Board board)
        {
            return BishopMoves(board);
        }
        
        public override PieceBase CreateCopy()
        {
            return new Bishop(IsWhite, CurrentPosition);
        }
    }
}
