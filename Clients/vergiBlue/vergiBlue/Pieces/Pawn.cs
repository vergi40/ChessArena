using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Pieces
{
    public class Pawn : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }
        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.Pawn(IsWhite, CurrentPosition);

        public Pawn(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'P';
            RelativeStrength = PieceBaseStrength.Pawn * Direction;
        }

        public Pawn(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'P';
            RelativeStrength = PieceBaseStrength.Pawn * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            return PositionStrength;
        }

        protected override SingleMove? CanMoveTo((int, int) target, Board board, bool validateBorders = false)
        {
            if (validateBorders && Logic.Logic.IsOutside(target)) return null;

            if (board.ValueAt(target) == null)
            {
                var promotion = target.Item2 == 0 || target.Item2 == 7;
                return new SingleMove(CurrentPosition, target, false, promotion);
            }
            return null;
        }

        private SingleMove? CanCapture((int, int) target, Board board)
        {
            if(Logic.Logic.IsOutside(target)) return null;

            // Normal
            var diagonalPiece = board.ValueAt(target);
            if (diagonalPiece != null && diagonalPiece.IsWhite != IsWhite)
            {
                var promotion = target.Item2 == 0 || target.Item2 == 7;
                return new SingleMove(CurrentPosition, diagonalPiece.CurrentPosition, true, promotion);
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
        public override IEnumerable<SingleMove> Moves(Board board)
        {
            var (column, row) = CurrentPosition;

            // Basic
            var move = CanMoveTo((column, row + Direction), board, true);
            if (move != null) yield return move;

            // Start possibility
            if (move != null && (row == 1 || row == 6))
            {
                move = CanMoveTo((column, row + (Direction * 2)), board, true);
                if (move != null) yield return move;
            }

            move = CanCapture((column - 1, row + Direction), board);
            if (move != null) yield return move;
            move = CanCapture((column + 1, row + Direction), board);
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy()
        {
            var piece = new Pawn(IsWhite, CurrentPosition);
            return piece;
        }
    }
}
