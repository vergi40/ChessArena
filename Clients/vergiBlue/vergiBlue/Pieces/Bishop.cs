using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;


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

        public override IEnumerable<SingleMove> Moves(IBoard board)
        {
            return BishopMoves(board);
        }
        
        public override PieceBase CreateCopy()
        {
            return new Bishop(IsWhite, CurrentPosition);
        }

        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            return BishopMoves(board);
        }

        public override bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            if (TryCreateBishopSliderAttack(board, opponentKing, out sliderAttack))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// If target is found first in direction, true. If some other piece or nothing, false
        /// </summary>
        public override bool CanAttackQuick((int column, int row) target, IBoard board)
        {
            if (TryCreateBishopDirectionVector(CurrentPosition, target, out var unitDirection))
            {
                foreach (var next in board.Shared.RawMoves.BishopRawMovesToDirection(CurrentPosition, unitDirection))
                {
                    if (target.Equals(next)) return true;
                    if (board.ValueAt(next) != null) return false;
                }
            }

            return false;
        }
    }
}
