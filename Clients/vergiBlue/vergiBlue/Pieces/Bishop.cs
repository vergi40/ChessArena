using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;


namespace vergiBlue.Pieces
{
    public class Bishop : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }
        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.Bishop(IsWhite, CurrentPosition);

        public Bishop(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'B';
            RelativeStrength = PieceBaseStrength.Bishop * Direction;
        }

        public Bishop(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'B';
            RelativeStrength = PieceBaseStrength.Bishop * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            return PositionStrength;
        }

        public override IEnumerable<SingleMove> Moves(IBoard board, bool returnSoftTargets = false)
        {
            return BishopMoves(board, returnSoftTargets);
        }
        
        public override PieceBase CreateCopy()
        {
            return new Bishop(IsWhite, CurrentPosition);
        }

        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            return BishopMoves(board, true);
        }
    }
}
