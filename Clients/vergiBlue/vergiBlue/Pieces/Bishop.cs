using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems.Attacking;


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
            return BishopMoves(board, true);
        }

        public override bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            sliderAttack = new SliderAttack();
            if (TryCreateBishopDirectionVector(CurrentPosition, opponentKing, out var direction))
            {
                sliderAttack.Attacker = CurrentPosition;
                sliderAttack.WhiteAttacking = IsWhite;
                sliderAttack.King = opponentKing;
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * direction.x;
                    var nextY = CurrentPosition.row + i * direction.y;
                    sliderAttack.AttackLine.Add((nextX, nextY));
                    if (opponentKing.Equals((nextX, nextY))) break;
                }
                return true;
            }
            return false;
        }
    }
}
