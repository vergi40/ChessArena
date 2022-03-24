using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using Math = System.Math;


namespace vergiBlue.Pieces
{
    public class King : PieceBase
    {
        public override char Identity { get; }
        public override double RelativeStrength { get; }

        public override double PositionStrength =>
            RelativeStrength + vergiBlue.PositionStrength.KingStartToMiddleGame(IsWhite, CurrentPosition);

        private double PositionStrengthInEnd =>
            RelativeStrength + vergiBlue.PositionStrength.KingEndGame(IsWhite, CurrentPosition);

        public King(bool isWhite, (int column, int row) position) : base(isWhite, position)
        {
            Identity = 'K';
            RelativeStrength = PieceBaseStrength.King * Direction;
        }

        public King(bool isWhite, string position) : base(isWhite, position)
        {
            Identity = 'K';
            RelativeStrength = PieceBaseStrength.King * Direction;
        }

        public override double GetEvaluationStrength(double endGameWeight = 0)
        {
            // TODO Debug hack
            if (endGameWeight < 0) return PositionStrengthInEnd;
            
            // Linear weighting to endgame strength 
            //return PositionStrength * (1 - endGameWeight) + PositionStrengthInEnd * endGameWeight;

            // Start normal relative weighting after halfgame
            // TODO testing
            if (endGameWeight < 0.5) return PositionStrength;
            return RelativeStrength;
        }

        public override IEnumerable<SingleMove> Moves(IBoard board)
        {
            foreach (var newPosition in board.Shared.RawMoves.King[CurrentPosition.To1DimensionArray()])
            {
                var validMove = CanMoveTo(newPosition, board, false);
                if (validMove != null) yield return validMove;
            }
        }

        public IEnumerable<SingleMove> MovesValidated(IBoard board)
        {
            var cur = CurrentPosition;

            var move = CanMoveTo((cur.column + 1, cur.row), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row - 1), board, true);
            if (move != null) yield return move;
        }

        public override PieceBase CreateCopy()
        {
            return new King(IsWhite, CurrentPosition);
        }

        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            return Moves(board);
        }

        public override bool CanAttackQuick((int column, int row) target, IBoard board)
        {
            // Target is in vicinity
            if (Math.Abs(target.row - CurrentPosition.row) <= 1 && Math.Abs(target.column - CurrentPosition.column) <= 1)
            {
                return true;
            }

            return false;
        }
    }
}
