using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Pawn : PieceBase
    {
        public override double RelativeStrength { get; }

        public Pawn(bool isOpponent, bool isWhite, Board boardReference) : base(isOpponent, isWhite, boardReference)
        {
            RelativeStrength = StrengthTable.Pawn * Direction;
        }

        protected override SingleMove CanMoveTo((int, int) target, bool validateBorders = false)
        {
            if (validateBorders && Logic.IsOutside(target)) return null;

            if (Board.ValueAt(target) == null)
            {
                var promotion = target.Item2 == 0 || target.Item2 == 7;
                return new SingleMove(CurrentPosition, target, false, promotion);
            }
            return null;
        }

        private SingleMove CanCapture((int, int) target)
        {
            if(Logic.IsOutside(target)) return null;

            // Normal
            var diagonal = Board.ValueAt(target);
            if (diagonal != null && diagonal.IsOpponent)
            {
                return new SingleMove(CurrentPosition, diagonal.CurrentPosition, true);
            }

            // En passant - opponent on side
            // Need to check that there is pawn next to and opponent has done double move from start to that pawn last round 
            // TODO


            return null;
        }

        /// <summary>
        /// List all allowed
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<SingleMove> Moves()
        {
            var cur = CurrentPosition;

            // Basic
            var move = CanMoveTo((cur.column, cur.row + Direction), true);
            if (move != null) yield return move;

            // Start possibility
            if (cur.row == 1 || cur.row == 6)
            {
                move = CanMoveTo((cur.column, cur.row + (Direction * 2)), true);
                if (move != null) yield return move;
            }

            move = CanCapture((cur.column - 1, cur.row + Direction));
            if (move != null) yield return move;
            move = CanCapture((cur.column + 1, cur.row + Direction));
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy(Board newBoard)
        {
            var piece = new Pawn(IsOpponent, IsWhite, newBoard);
            piece.CurrentPosition = CurrentPosition;
            return piece;
        }
    }
}
