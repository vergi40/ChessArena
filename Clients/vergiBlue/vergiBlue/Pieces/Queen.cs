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
            var moves = BishopMoves(board);
            return moves.Concat(RookMoves(board));
        }

        public override bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            if (TryCreateBishopSliderAttack(board, opponentKing, out sliderAttack))
            {
                return true;
            }
            if (TryCreateRookSliderAttack(board, opponentKing, out sliderAttack))
            {
                return true;
            }

            return false;
        }

        public override bool CanAttackQuick((int column, int row) target, IBoard board)
        {
            if (TryCreateRookDirectionUInitVector(CurrentPosition, target, out var unitDirectionR))
            {
                foreach (var next in board.Shared.RawMoves.RookRawMovesToDirection(CurrentPosition, unitDirectionR))
                {
                    if (target.Equals(next)) return true;
                    if (board.ValueAt(next) != null) return false;
                }
            }
            if (TryCreateBishopDirectionUnitVector(CurrentPosition, target, out var unitDirectionB))
            {
                foreach (var next in board.Shared.RawMoves.BishopRawMovesToDirection(CurrentPosition, unitDirectionB))
                {
                    if (target.Equals(next)) return true;
                    if (board.ValueAt(next) != null) return false;
                }
            }

            return false;
        }
    }
}
