using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Linear weighting to endgame strength 
            //return PositionStrength * (1 - endGameWeight) + PositionStrengthInEnd * endGameWeight;

            // Start normal relative weighting after halfgame
            // TODO testing
            if (endGameWeight < 0.5) return PositionStrength;
            return RelativeStrength;
        }

        public override IEnumerable<SingleMove> Moves(Board board)
        {
            var cur = CurrentPosition;

            var moveRight = CanMoveTo((cur.column + 1, cur.row), board, true);
            if (moveRight != null) yield return moveRight;

            var move = CanMoveTo((cur.column + 1, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row + 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column - 1, cur.row + 1), board, true);
            if (move != null) yield return move;

            var moveLeft = CanMoveTo((cur.column - 1, cur.row), board, true);
            if (moveLeft != null) yield return moveLeft;

            move = CanMoveTo((cur.column - 1, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column, cur.row - 1), board, true);
            if (move != null) yield return move;

            move = CanMoveTo((cur.column + 1, cur.row - 1), board, true);
            if (move != null) yield return move;

            // Castling
            // Only check if not done yet
            if (IsWhite)
            {
                if (board.Strategic.WhiteLeftCastlingValid && board.CanCastleToLeft(true))
                {
                    yield return new SingleMove(CurrentPosition, (2, 0));
                }

                if (board.Strategic.WhiteRightCastlingValid && board.CanCastleToRight(true))
                {
                    yield return new SingleMove(CurrentPosition, (6, 0));
                }
            }
            else
            {
                if (board.Strategic.BlackLeftCastlingValid && board.CanCastleToLeft(false))
                {
                    yield return new SingleMove(CurrentPosition, (2, 7));
                }

                if (board.Strategic.BlackRightCastlingValid && board.CanCastleToRight(false))
                {
                    yield return new SingleMove(CurrentPosition, (6, 7));
                }
            }
        }

        public override PieceBase CreateCopy()
        {
            return new King(IsWhite, CurrentPosition);
        }
    }
}
