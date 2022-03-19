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

        public override IEnumerable<SingleMove> Moves(IBoard board)
        {
            var moves = BishopMoves(board);
            return moves.Concat(RookMoves(board));
        }

        public override PieceBase CreateCopy()
        {
            return new Queen(IsWhite, CurrentPosition);
        }

        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            var moves = BishopMoves(board, true);
            return moves.Concat(RookMoves(board));
        }

        public override bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            sliderAttack = new SliderAttack();
            if (TryCreateBishopDirectionVector(CurrentPosition, opponentKing, out var bDir))
            {
                sliderAttack.Attacker = CurrentPosition;
                sliderAttack.WhiteAttacking = IsWhite;
                sliderAttack.King = opponentKing;
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * bDir.x;
                    var nextY = CurrentPosition.row + i * bDir.y;
                    sliderAttack.AttackLine.Add((nextX, nextY));
                    if (opponentKing.Equals((nextX, nextY))) break;
                }
                return true;
            }
            if (TryCreateRookDirectionVector(CurrentPosition, opponentKing, out var rDir))
            {
                sliderAttack.Attacker = CurrentPosition;
                sliderAttack.WhiteAttacking = IsWhite;
                sliderAttack.King = opponentKing;
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * rDir.x;
                    var nextY = CurrentPosition.row + i * rDir.y;
                    sliderAttack.AttackLine.Add((nextX, nextY));
                    if (opponentKing.Equals((nextX, nextY))) break;
                }
                return true;
            }
            return false;
        }

        public override bool CanAttackQuick((int column, int row) target, IBoard board)
        {
            if (TryCreateBishopDirectionVector(CurrentPosition, target, out var bDir))
            {
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * bDir.x;
                    var nextY = CurrentPosition.row + i * bDir.y;
                    if (target.Equals((nextX, nextY))) return true;
                    if (board.ValueAt((nextX, nextY)) != null) return false;
                }
            }
            if (TryCreateRookDirectionVector(CurrentPosition, target, out var rDir))
            {
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * rDir.x;
                    var nextY = CurrentPosition.row + i * rDir.y;
                    if (target.Equals((nextX, nextY))) return true;
                    if (board.ValueAt((nextX, nextY)) != null) return false;
                }
            }

            return false;
        }
    }
}
