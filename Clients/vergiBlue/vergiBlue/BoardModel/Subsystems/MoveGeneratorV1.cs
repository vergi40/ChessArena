using System.Collections.Generic;
using System.Linq;
using vergiBlue.Algorithms;

namespace vergiBlue.BoardModel.Subsystems
{
    /// <summary>
    /// 100% Perft-proof move generator. Slow, since check validation is done by generating new board based on each move.
    /// </summary>
    public class MoveGeneratorV1
    {
        private IBoard _board { get; }

        public MoveGeneratorV1(IBoard board)
        {
            _board = board;
        }

        /// <summary>
        /// Find every possible move for every piece for given color. IEnumerable should be utilized when there are cutoff etc changes.
        /// </summary>
        /// <param name="forWhite"></param>
        /// <param name="kingInDanger">Validate check for each move</param>
        /// <returns></returns>
        public IEnumerable<SingleMove> MovesQuick(bool forWhite, bool kingInDanger = false)
        {
            foreach (var castlingMove in CastlingMoves(forWhite))
            {
                yield return castlingMove;
            }

            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    yield return singleMove;
                }
            }
        }

        public IEnumerable<SingleMove> MovesForPiece((int column, int row) position)
        {
            var piece = _board.ValueAtDefinitely(position);
            var isWhite = piece.IsWhite;

            if (piece.Identity == 'K')
            {
                foreach (var castlingMove in CastlingMoves(isWhite))
                {
                    yield return castlingMove;
                }
            }

            foreach (var singleMove in piece.Moves(_board))
            {
                // Only allow moves that don't result in check
                var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                if (newBoard.IsCheck(!isWhite)) continue;

                yield return singleMove;
            }
        }

        private IEnumerable<SingleMove> CastlingMoves(bool forWhite)
        {
            // In tests king might not exist
            var king = _board.KingLocation(forWhite);
            if (king != null)
            {
                // Quick validations
                var (leftOk, rightOk) = Castling.PreValidation(_board, king);
                if (!leftOk && !rightOk)
                {
                    yield break;
                }

                // Heavy validation (attack squares)
                var attackSquares = AttackMoves(!forWhite).Select(m => m.NewPos).ToHashSet();
                if (leftOk && Castling.TryCreateLeftCastling(king, attackSquares, out var leftCastling))
                {
                    yield return leftCastling;
                }
                if (rightOk && Castling.TryCreateRightCastling(king, attackSquares, out var rightCastling))
                {
                    yield return rightCastling;
                }
            }
        }

        /// <summary>
        /// Find every possible move for every piece for given color.
        /// Because sorting, full list returned.
        /// </summary>
        /// <param name="forWhite"></param>
        /// <param name="heavyOrdering">Sort by light guess weight vs evaluate each new position.</param>
        /// <param name="kingInDanger">Validate check for each move</param>
        /// <returns></returns>
        public IList<SingleMove> MovesWithOrdering(bool forWhite, bool heavyOrdering, bool kingInDanger = false)
        {
            IList<SingleMove> list = new List<SingleMove>();
            foreach (var castlingMove in CastlingMoves(forWhite))
            {
                list.Add(castlingMove);
            }

            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    list.Add(singleMove);
                }
            }

            if (heavyOrdering) return MoveOrdering.SortMovesByEvaluation(list, _board, forWhite);
            return MoveOrdering.SortMovesByGuessWeight(list, _board, forWhite);
        }

        /// <summary>
        /// Find every possible move for every piece for given color. IEnumerable should be utilized when there are cutoff etc changes.
        /// </summary>
        /// <param name="forWhite"></param>
        /// <param name="kingInDanger">Validate check for each move</param>
        /// <returns></returns>
        public IEnumerable<SingleMove> MovesQuickWithoutCastling(bool forWhite, bool kingInDanger = false)
        {
            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        // Only allow moves that don't result in check
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (newBoard.IsCheck(!forWhite)) continue;
                    }

                    yield return singleMove;
                }
            }
        }
        
        /// <summary>
        /// All possible capture positions (including pawn).
        /// No need to validate check (atm)
        /// </summary>
        /// <param name="forWhiteAttacker"></param>
        /// <returns></returns>
        public IEnumerable<SingleMove> AttackMoves(bool forWhiteAttacker)
        {
            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhiteAttacker))
            {
                if (piece.Identity == 'P')
                {
                    foreach (var pawnMove in piece.PawnPseudoCaptureMoves(_board, false))
                    {
                        yield return pawnMove;
                    }
                }
                else
                {
                    foreach (var singleMove in piece.Moves(_board))
                    {
                        yield return singleMove;
                    }
                }
            }
        }
    }
}
