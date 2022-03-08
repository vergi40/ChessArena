using System;
using System.Collections;
using System.Collections.Generic;
using CommonNetStandard.Interface;
using vergiBlue.BoardModel;


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

        /// <summary>
        /// List all valid moves. It is important to check least amount of positions
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<SingleMove> Moves(IBoard board, bool returnSoftTargets)
        {
            var (column, row) = CurrentPosition;
            var (start, end, enpassantRow) = GetSpecialRows();

            if (row == end)
            {
                foreach (var move in GetPromotionMoves(column, row, board))
                {
                    yield return move;
                }
            }
            else
            {
                var normalMove = TryForward(column, row, board);
                if (normalMove != null)
                {
                    yield return normalMove;
                    if (row == start && board.ValueAt((column, row + Direction * 2)) == null)
                    {
                        // Free to do start move
                        yield return new SingleMove((column, row), (column, row + Direction * 2));
                    }
                }

                if (ValidCapturePosition(column - 1, row + Direction, board))
                {
                    yield return new SingleMove((column, row), (column - 1, row + Direction), true);
                }
                if (ValidCapturePosition(column + 1, row + Direction, board))
                {
                    yield return new SingleMove((column, row), (column + 1, row + Direction), true);
                }

                if (board.Strategic.EnPassantPossibility != null && row == enpassantRow)
                {
                    // E.g. if possibility (2,2)
                    // (3,3) -> (2,2)
                    // (1,3) -> (2,2)
                    var (eColumn, eRow) = board.Strategic.EnPassantPossibility.Value;
                    if (column == eColumn + 1 && row == eRow - Direction)
                    {
                        yield return new SingleMove((column, row), (eColumn, eRow), true, true);
                    }
                    else if (column == eColumn - 1 && row == eRow - Direction)
                    {
                        yield return new SingleMove((column, row), (eColumn, eRow), true, true);
                    }
                }
            }
        }

        /// <summary>
        /// If possible to move from current position, return move
        /// </summary>
        private SingleMove? TryForward(int column, int row, IBoard board)
        {
            if (board.ValueAt((column, row + Direction)) == null)
            {
                return new SingleMove((column, row), (column, row + Direction));
            }

            return null;
        }

        private IEnumerable<SingleMove> GetPromotionMoves(int column, int row, IBoard board)
        {
            var nextRow = row + Direction;
            if(board.ValueAt((column, nextRow)) == null)
            {
                yield return new SingleMove((column, row), (column, nextRow), false, PromotionPieceType.Queen);
                yield return new SingleMove((column, row), (column, nextRow), false, PromotionPieceType.Rook);
                yield return new SingleMove((column, row), (column, nextRow), false, PromotionPieceType.Knight);
                yield return new SingleMove((column, row), (column, nextRow), false, PromotionPieceType.Bishop);
            }
            if (ValidCapturePosition(column - 1, nextRow, board))
            {
                yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Queen);
                yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Rook);
                yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Knight);
                yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Bishop);
            }
            if (ValidCapturePosition(column + 1, nextRow, board))
            {
                yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Queen);
                yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Rook);
                yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Knight);
                yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Bishop);
            }
        }

        /// <summary>
        /// Used for attack squares - use allPromotions switch to return all possibilities
        /// </summary>
        private IEnumerable<SingleMove> GetPseudoPromotionMoves(int column, int row, IBoard board, bool allPromotions = false, bool returnSoftTargets = false)
        {
            var nextRow = row + Direction;
            if (ValidPseudoCapturePosition(column - 1, nextRow, board))
            {
                yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Queen);
                if (allPromotions)
                {
                    yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Rook);
                    yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Knight);
                    yield return new SingleMove((column, row), (column - 1, nextRow), true, PromotionPieceType.Bishop);
                }
            }
            else if (ValidPseudoCapturePosition(column - 1, nextRow, board, true))
            {
                // TODO efficiency improvements
                yield return SingleMoveFactory.CreateSoftTarget((column, row), (column - 1, nextRow));
            }
            if (ValidPseudoCapturePosition(column + 1, nextRow, board, returnSoftTargets))
            {
                yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Queen);
                if (allPromotions)
                {
                    yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Rook);
                    yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Knight);
                    yield return new SingleMove((column, row), (column + 1, nextRow), true, PromotionPieceType.Bishop);
                }
            }
            else if (ValidPseudoCapturePosition(column + 1, nextRow, board, true))
            {
                // TODO efficiency improvements
                yield return SingleMoveFactory.CreateSoftTarget((column, row), (column + 1, nextRow));
            }
        }

        /// <summary>
        /// Assumes row value is valid. Column can be outside.
        /// </summary>
        private bool ValidCapturePosition(int column, int row, IBoard board)
        {
            if (column < 0 || column > 7) return false;
            var piece = board.ValueAt((column, row));
            if (piece != null && piece.IsWhite != IsWhite)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Valid if there is opponent or empty "possible" square. If own piece, possibly return soft target
        /// </summary>
        private bool ValidPseudoCapturePosition(int column, int row, IBoard board, bool returnSoftTargets = false)
        {
            if (column < 0 || column > 7) return false;
            var piece = board.ValueAt((column, row));
            if (piece == null)
            {
                return true;
            }
            if (piece.IsWhite != IsWhite)
            {
                return true;
            }
            if (returnSoftTargets)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate pseudo capture if empty or opponent. Soft target possibility.
        /// Prerequisite: row is valid
        /// </summary>
        private bool TryCreatePseudoCapture(int column, int row, IBoard board, bool returnSoftTargets, out SingleMove move)
        {
            move = SingleMoveFactory.CreateEmpty();
            if (column < 0 || column > 7) return false;
            var piece = board.ValueAt((column, row));
            if (piece == null)
            {
                move = new SingleMove(CurrentPosition, (column, row));
                return true;
            }
            if (piece.IsWhite != IsWhite)
            {
                move = new SingleMove(CurrentPosition, (column, row), true);
                return true;
            }

            if (returnSoftTargets)
            {
                move = SingleMoveFactory.CreateSoftTarget(CurrentPosition, (column, row));
                return true;
            }

            return false;
        }

        private bool ValidSoftPosition(int column, int row, IBoard board)
        {
            if (column < 0 || column > 7) return false;
            var piece = board.ValueAt((column, row));
            if (piece != null && piece.IsWhite == IsWhite)
            {
                return true;
            }

            return false;
        }

        private (int start, int end, int enpassantRow) GetSpecialRows()
        {
            if (IsWhite) return (1, 6, 4);
            return (6, 1, 3);
        }

        public override PieceBase CreateCopy()
        {
            var piece = new Pawn(IsWhite, CurrentPosition);
            return piece;
        }

        [Obsolete("Directly implemented to pseudo moves")]
        public override IEnumerable<SingleMove> MovesWithSoftTargets(IBoard board)
        {
            foreach (var move in Moves(board, false))
            {
                yield return move;
            }

            var (column, row) = CurrentPosition;
            if (ValidCapturePosition(column - 1, row + Direction, board))
            {
                yield return new SingleMove((column, row), (column - 1, row + Direction), true);
            }
            if (ValidCapturePosition(column + 1, row + Direction, board))
            {
                yield return new SingleMove((column, row), (column + 1, row + Direction), true);
            }
        }

        /// <summary>
        /// Normal forwarding and en passant
        /// </summary>
        public override IEnumerable<SingleMove> PawnNormalMoves(IBoard board)
        {
            var (column, row) = CurrentPosition;
            var (start, end, enpassantRow) = GetSpecialRows();

            if (row == end)
            {
                // Use PseudoCaptureMoves
            }
            else
            {
                var normalMove = TryForward(column, row, board);
                if (normalMove != null)
                {
                    yield return normalMove;
                    if (row == start && board.ValueAt((column, row + Direction * 2)) == null)
                    {
                        // Free to do start move
                        yield return new SingleMove((column, row), (column, row + Direction * 2));
                    }
                }
                
                if (board.Strategic.EnPassantPossibility != null && row == enpassantRow)
                {
                    // E.g. if possibility (2,2)
                    // (3,3) -> (2,2)
                    // (1,3) -> (2,2)
                    var (eColumn, eRow) = board.Strategic.EnPassantPossibility.Value;
                    if (column == eColumn + 1 && row == eRow - Direction)
                    {
                        yield return new SingleMove((column, row), (eColumn, eRow), true, true);
                    }
                    else if (column == eColumn - 1 && row == eRow - Direction)
                    {
                        yield return new SingleMove((column, row), (eColumn, eRow), true, true);
                    }
                }
            }
        }

        /// <summary>
        /// All capturing positions, even if there is no opponent present
        /// </summary>
        public override IEnumerable<SingleMove> PawnPseudoCaptureMoves(IBoard board, bool returnSoftTargets)
        {
            // This is a bit tricky
            // Add capture moves even though there isn't opponent at square
            // Remember promotions
            var (column, row) = CurrentPosition;
            var (start, end, enpassantRow) = GetSpecialRows();

            if (row == end)
            {
                foreach (var move in GetPseudoPromotionMoves(column, row, board, true, returnSoftTargets))
                {
                    yield return move;
                }
            }
            else
            {
                if (TryCreatePseudoCapture(column - 1, row + Direction, board, returnSoftTargets, out var move))
                {
                    yield return move;
                }
                if (TryCreatePseudoCapture(column + 1, row + Direction, board, returnSoftTargets, out move))
                {
                    yield return move;
                }
            }
        }
    }
}
