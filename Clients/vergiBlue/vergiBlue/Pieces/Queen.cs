using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Queen : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }
        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.Queen(IsWhite, CurrentPosition);

        public Queen(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'Q';
            RelativeStrength = PieceBaseStrength.Queen * Direction;
        }

        public Queen(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'Q';
            RelativeStrength = PieceBaseStrength.Queen * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            return PositionStrength;
        }

        public override IEnumerable<SingleMove> Moves(Board board)
        {
            var moves = BishopMoves(board);
            return moves.Concat(RookMoves(board));
        }

        public override PieceBase CreateCopy()
        {
            return new Queen(IsWhite, CurrentPosition);
        }
    }
}
