using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Interface;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;


namespace vergiBlue.Pieces
{
    public abstract class PieceBase : IPiece
    {
        public bool IsWhite { get; }

        /// <summary>
        /// Upper case K, Q, R, N, B, P
        /// </summary>
        public abstract char Identity { get; }
        
        /// <summary>
        /// Static strength for piece type. White positive, black negative.
        /// </summary>
        public abstract double RelativeStrength { get; }
        
        /// <summary>
        /// Static strenght for combination of piece type and position.
        /// </summary>
        public abstract double PositionStrength { get; }

        /// <summary>
        /// Sign of general direction. Can also be used to classify white as positive and black as negative value.
        /// </summary>
        public int Direction
        {
            get
            {
                if (IsWhite) return 1;
                return -1;
            }
        }

        public (int column, int row) CurrentPosition { get; set; }
        
        protected PieceBase(bool isWhite, (int column, int row) position)
        {
            IsWhite = isWhite;
            CurrentPosition = position;
        }

        protected PieceBase(bool isWhite, string position)
        {
            IsWhite = isWhite;
            CurrentPosition = position.ToTuple();
        }

        public abstract double GetEvaluationStrength(double endGameWeight = 0);
        
        

        /// <summary>
        /// If target position is empty or has opponent piece, return SingleMove. If own piece or outside board, return null.
        /// </summary>
        protected virtual SingleMove? CanMoveTo((int, int) target, IBoard board, bool validateBorders = false, bool returnSoftTargets = false)
        {
            if (validateBorders && Validator.IsOutside(target)) return null;

            var valueAt = board.ValueAt(target);
            if (valueAt == null)
            {
                return new SingleMove(CurrentPosition, target);
            }
            else if (valueAt.IsWhite != IsWhite)
            {
                return new SingleMove(CurrentPosition, target, true);
            }
            else if (returnSoftTargets && valueAt.IsWhite == IsWhite)
            {
                return SingleMoveFactory.CreateSoftTarget(CurrentPosition, target);
            }
            return null;
        }

        /// <summary>
        /// Each move the piece can make in current board setting
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SingleMove> Moves(IBoard board);

        /// <summary>
        /// Copy needs to be made with the derived class constructor so type matches
        /// </summary>
        /// <returns></returns>
        public abstract PieceBase CreateCopy();

        protected IEnumerable<SingleMove> RookMoves(IBoard board)
        {
            foreach(var rawMove in board.Shared.RawMoves.RookRawMovesToDirection(CurrentPosition, Directions.N))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
            foreach (var rawMove in board.Shared.RawMoves.RookRawMovesToDirection(CurrentPosition, Directions.E))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
            foreach (var rawMove in board.Shared.RawMoves.RookRawMovesToDirection(CurrentPosition, Directions.S))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
            foreach (var rawMove in board.Shared.RawMoves.RookRawMovesToDirection(CurrentPosition, Directions.W))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
        }

        protected IEnumerable<SingleMove> BishopMoves(IBoard board)
        {
            foreach (var rawMove in board.Shared.RawMoves.BishopRawMovesToDirection(CurrentPosition, Directions.NE))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
            foreach (var rawMove in board.Shared.RawMoves.BishopRawMovesToDirection(CurrentPosition, Directions.SE))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
            foreach (var rawMove in board.Shared.RawMoves.BishopRawMovesToDirection(CurrentPosition, Directions.SW))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
            foreach (var rawMove in board.Shared.RawMoves.BishopRawMovesToDirection(CurrentPosition, Directions.NW))
            {
                var move = CanMoveTo(rawMove, board, false);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
        }
        
        /// <summary>
        /// In addition to all valid moves, return soft targets (captures on own pieces)
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public abstract IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board);

        /// <summary>
        /// List all capture moves for knowing possible attack squares.
        /// "Pseudo" as the pawn captures are listed even though there is no opponent in target square
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public virtual IEnumerable<SingleMove> PseudoCaptureMoves(IBoard board)
        {
            // Probably should have individual override for each function. 
            // Now only for pawn
            return Moves(board);
        }

        public virtual bool TryCreateSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            sliderAttack = new SliderAttack();
            return false;
        }

        protected bool TryCreateRookDirectionVector((int x, int y) pos1, (int x, int y) pos2, out (int x, int y) direction)
        {
            // e.g. piece (4,0), king (2,0). (2,0) - (4,0) = (-2,0) -> two steps left
            direction = (pos2.x - pos1.x, pos2.y - pos1.y);
            if (direction.x * direction.y == 0)
            {
                direction = (Math.Sign(direction.x), Math.Sign(direction.y));
                return true;
            }
            return false;
        }

        /// <summary>
        /// E.g. pos1 (4,4), pos2 (2,2). (2,2) - (4,4) = (-2,-2) -> two steps sw
        /// </summary>
        protected (int x, int y) GetTransformation((int x, int y) pos1, (int x, int y) pos2)
        {
            return (pos2.x - pos1.x, pos2.y - pos1.y);
        }

        protected bool TryCreateBishopDirectionVector((int x, int y) pos1, (int x, int y) pos2, out (int x, int y) direction)
        {
            // e.g. piece (4,4), king (2,2). (2,2) - (4,4) = (-2,-2) -> two steps sw
            // e.g. piece (0,4), king (4,0). (4,0) - (0,4) = (4, -4) -> two steps sw
            direction = GetTransformation(pos1, pos2);
            if (Math.Abs(direction.x) == Math.Abs(direction.y))
            {
                direction = (Math.Sign(direction.x), Math.Sign(direction.y));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lightweight validation if piece in current position can attack target (king)
        /// </summary>
        public abstract bool CanAttackQuick((int column, int row) target, IBoard board);

        protected bool TryCreateBishopSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            sliderAttack = new SliderAttack();
            if (TryCreateBishopDirectionVector(CurrentPosition, opponentKing, out var direction))
            {
                sliderAttack.Attacker = CurrentPosition;
                sliderAttack.WhiteAttacking = IsWhite;
                sliderAttack.King = opponentKing;
                var pinCount = 0;
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * direction.x;
                    var nextY = CurrentPosition.row + i * direction.y;
                    sliderAttack.AttackLine.Add((nextX, nextY));
                    if (opponentKing.Equals((nextX, nextY))) break;

                    var pin = board.ValueAt((nextX, nextY));
                    if (pin != null && pin.IsWhite != IsWhite)
                    {
                        sliderAttack.Pin = ((nextX, nextY));
                        pinCount++;
                    }

                    if (pinCount > 1) return false;

                }
                return true;
            }
            return false;
        }
        protected bool TryCreateRookSliderAttack(IBoard board, (int column, int row) opponentKing, out SliderAttack sliderAttack)
        {
            sliderAttack = new SliderAttack();
            if (TryCreateRookDirectionVector(CurrentPosition, opponentKing, out var direction))
            {
                sliderAttack.Attacker = CurrentPosition;
                sliderAttack.WhiteAttacking = IsWhite;
                sliderAttack.King = opponentKing;
                var pinCount = 0;
                for (int i = 1; i < 8; i++)
                {
                    var nextX = CurrentPosition.column + i * direction.x;
                    var nextY = CurrentPosition.row + i * direction.y;
                    sliderAttack.AttackLine.Add((nextX, nextY));
                    if (opponentKing.Equals((nextX, nextY))) break;

                    var pin = board.ValueAt((nextX, nextY));
                    if (pin != null && pin.IsWhite != IsWhite)
                    {
                        sliderAttack.Pin = ((nextX, nextY));
                        pinCount++;
                    }

                    if (pinCount > 1) return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Slider move to one direction, Rook, bishop, queen
        /// </summary>
        public IReadOnlyList<(int column, int row)> MovesValidatedToDirection((int x, int y) direction)
        {
            var result = new List<(int column, int row)>();
            for (int i = 1; i < 8; i++)
            {
                var nextX = CurrentPosition.column + i * direction.x;
                var nextY = CurrentPosition.row + i * direction.y;

                if (Validator.IsOutside((nextX, nextY))) break;
                result.Add((nextX, nextY));
            }

            return result;
        }
    }
}
