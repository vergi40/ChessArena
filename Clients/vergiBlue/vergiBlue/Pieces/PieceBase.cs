using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
            var column = CurrentPosition.column;
            var row = CurrentPosition.row;

            // Up
            for (int i = row + 1; i < 8; i++)
            {
                var move = CanMoveTo((column, i), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // Down
            for (int i = row - 1; i >= 0; i--)
            {
                var move = CanMoveTo((column, i), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // Right
            for (int i = column + 1; i < 8; i++)
            {
                var move = CanMoveTo((i, row), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // Left
            for (int i = column - 1; i >= 0; i--)
            {
                var move = CanMoveTo((i, row), board);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }
        }

        protected IEnumerable<SingleMove> BishopMoves(IBoard board, bool returnSoftTargets = false)
        {
            var column = CurrentPosition.column;
            var row = CurrentPosition.row;

            // NE
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column + i, row + i), board, true, returnSoftTargets);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // SE
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column + i, row - i), board, true, returnSoftTargets);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // SW
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column - i, row - i), board, true, returnSoftTargets);
                if (move != null)
                {
                    yield return move;
                    if (move.Capture) break;
                }
                else break;
            }

            // NW
            for (int i = 1; i < 8; i++)
            {
                var move = CanMoveTo((column - i, row + i), board, true, returnSoftTargets);
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
        /// Pawn capturing positions, even if there is no opponent present
        /// </summary>
        public virtual IEnumerable<SingleMove> PawnPseudoCaptureMoves(IBoard board)
        {
            return Enumerable.Empty<SingleMove>();
        }

        /// <summary>
        /// Pawn normal forwarding and en passant
        /// </summary>
        public virtual IEnumerable<SingleMove> PawnNormalMoves(IBoard board)
        {
            return Enumerable.Empty<SingleMove>();
        }

        /// <summary>
        /// Sliding attacker has line for king
        /// </summary>
        public bool TryFindPseudoKingCapture(IBoard board, out KingUnderSliderAttack attack)
        {
            attack = new KingUnderSliderAttack();
            if (Identity == 'R')
            {
                if (TryCreateRookAttack(board, out attack))
                {
                    return true;
                }
            }
            else if (Identity == 'B')
            {
                if (TryCreateBishopAttack(board, out attack))
                {
                    return true;
                }
            }
            else if (Identity == 'Q')
            {
                if (TryCreateRookAttack(board, out attack))
                {
                    return true;
                }
                if (TryCreateBishopAttack(board, out attack))
                {
                    return true;
                }
            }
            
            return false;
        }

        enum SquareTypes
        {
            Empty,
            OpponentKing,
            Outside,
            OpponentPiece,
            OwnPiece
        }

        /// <summary>
        /// 0: go on
        /// 1: king
        /// 2: outside
        /// 3: own piece
        /// </summary>
        private SquareTypes IsKingOrOutside((int column, int row) target, IBoard board)
        {
            if (Validator.IsOutside(target)) return SquareTypes.Outside;

            var valueAt = board.ValueAt(target);
            if (valueAt == null)
            {
                return SquareTypes.Empty;
            }
            else if (valueAt.IsWhite != IsWhite && valueAt.Identity == 'K')
            {
                return SquareTypes.OpponentKing;
            }
            else if (valueAt.IsWhite == IsWhite)
            {
                return SquareTypes.OwnPiece;
            }

            return SquareTypes.OpponentPiece;
        }

        // TODO use unit vectors to check king direction 

        /// <summary>
        /// Iterate to one direction and collect square information.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="directionUnit">E.g. (+1, -1)</param>
        /// <param name="attack"></param>
        /// <returns></returns>
        private bool TryBuildKingSliderAttack(IBoard board, (int column, int row) directionUnit, out KingUnderSliderAttack attack)
        {
            var (column, row) = CurrentPosition;
            attack = new KingUnderSliderAttack
            {
                Attacker = CurrentPosition,
                WhiteAttacking = IsWhite
            };
            var kingFound = false;
            var guardPieceCount = 0;

            for (int i = 1; i < 8; i++)
            {
                var nextColumn = column + i * directionUnit.column;
                var nextRow = row + i * directionUnit.row;
                var next = IsKingOrOutside((nextColumn, nextRow), board);
                if (next == SquareTypes.Outside) break;
                else if (next == SquareTypes.OwnPiece && !kingFound) break;
                else if (next == SquareTypes.OpponentKing)
                {
                    attack.AttackLine.Add((nextColumn, nextRow));
                    attack.King = (nextColumn, nextRow);
                    kingFound = true;
                }
                else if (next == SquareTypes.OpponentPiece && !kingFound)
                {
                    guardPieceCount++;
                    attack.AttackLine.Add((nextColumn, nextRow));
                    attack.GuardPiece = (nextColumn, nextRow);
                }
                else
                {
                    if (kingFound)
                    {
                        attack.BehindKing.Add((nextColumn, nextRow));
                    }
                    else
                    {
                        attack.AttackLine.Add((nextColumn, nextRow));
                    }
                }
            }

            if (!kingFound) return false;
            if (guardPieceCount > 1) return false;
            return true;
        }

        private bool TryCreateRookAttack(IBoard board, out KingUnderSliderAttack attack)
        {
            if (TryBuildKingSliderAttack(board, (1, 0), out attack))
            {
                return true;
            }
            if (TryBuildKingSliderAttack(board, (-1, 0), out attack))
            {
                return true;
            }
            if (TryBuildKingSliderAttack(board, (0, 1), out attack))
            {
                return true;
            }
            if (TryBuildKingSliderAttack(board, (0, -1), out attack))
            {
                return true;
            }

            return false;
        }

        private bool TryCreateBishopAttack(IBoard board, out KingUnderSliderAttack attack)
        {
            if (TryBuildKingSliderAttack(board, (1, 1), out attack))
            {
                return true;
            }
            if (TryBuildKingSliderAttack(board, (-1, 1), out attack))
            {
                return true;
            }
            if (TryBuildKingSliderAttack(board, (1, -1), out attack))
            {
                return true;
            }
            if (TryBuildKingSliderAttack(board, (-1, -1), out attack))
            {
                return true;
            }

            return false;
        }
    }
}
