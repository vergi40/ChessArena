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
        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.Rook(IsWhite, CurrentPosition);

        public Rook(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'R';
            RelativeStrength = PieceBaseStrength.Rook * Direction;
        }

        public Rook(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'R';
            RelativeStrength = PieceBaseStrength.Rook * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            // Start normal relative weighting after halfgame
            if (endGameWeight < 0.5) return PositionStrength;
            return RelativeStrength;
        }

        public override IEnumerable<SingleMove> Moves(BoardModel.IBoard board)
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
