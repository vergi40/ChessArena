using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;

namespace vergiBlue.Pieces
{
    public class Knight : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }

        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.Knight(IsWhite, CurrentPosition);

        public Knight(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'N';
            RelativeStrength = PieceBaseStrength.Knight * Direction;
        }

        public Knight(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'N';
            RelativeStrength = PieceBaseStrength.Knight * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            return PositionStrength;
        }

        public override IEnumerable<SingleMove> Moves(IBoard board)
        {
            foreach (var newPosition in board.Shared.RawMoves.Knight[CurrentPosition.To1DimensionArray()])
            {
                var validMove = CanMoveTo(newPosition, board, false);
                if (validMove != null) yield return validMove;
            }
        }

        public IEnumerable<SingleMove> MovesValidated(IBoard board)
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

        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            return Moves(board);
        }

        public override bool CanAttackQuick((int column, int row) target, IBoard board)
        {
            if (target.Equals((-1, -1))) throw new ArgumentException("Knight used out-of-board target");
            var dirAndDistance = GetTransformation(CurrentPosition, target);
            // TODO max 2
            if (Math.Abs(dirAndDistance.x) + Math.Abs(dirAndDistance.y) == 3 && Math.Abs(dirAndDistance.x) * Math.Abs(dirAndDistance.y) == 2)
            {
                // Don't really care of positions, just see if manhattan distance is 3
                return true;
            }

            return false;
        }
    }
}
