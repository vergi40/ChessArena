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

        public override IEnumerable<SingleMove> Moves(IBoard board, bool returnSoftTargets)
        {
            var cur = CurrentPosition;

            var move = CanMoveTo((cur.column - 1, cur.row + 2), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row + 2), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 2, cur.row - 1), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 2, cur.row + 1), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row - 2), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row - 2), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 2, cur.row - 1), board, true, returnSoftTargets);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 2, cur.row + 1), board, true, returnSoftTargets);
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy()
        {
            return new Knight(IsWhite, CurrentPosition);
        }

        [Obsolete("Directly implemented to pseudo moves")]
        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            return Moves(board, true);
        }
    }
}
