using System;
using System.Collections.Generic;
using System.Linq;
using vergiBlue.Algorithms;

namespace vergiBlue.BoardModel.Subsystems
{
    public class MoveGenerator
    {
        private IBoard _board { get; }

        public MoveGenerator(IBoard board)
        {
            _board = board;
        }

        private (int column, int row) GetKingLocationOrDefault(bool whiteKing)
        {
            var opponentKing = _board.KingLocation(whiteKing);
            var position = opponentKing != null ? opponentKing.CurrentPosition : (-1, -1);
            return position;
        }

        /// <summary>
        /// Find every possible move for every piece for given color. IEnumerable should be utilized when there are cutoff etc changes.
        /// </summary>
        /// <param name="forWhite"></param>
        /// <param name="kingInDanger">Validate check for each move</param>
        /// <returns></returns>
        [Obsolete("Use ValidMovesQuick or MovesWithOrdering. The kingInDanger option is dropped")]
        public IEnumerable<SingleMove> MovesQuick(bool forWhite, bool kingInDanger = false)
        {
            return ValidMovesQuick(forWhite);
        }

        /// <summary>
        /// All valid, legal moves. IEnumerable pattern can be utilized to stop calculation early
        /// </summary>
        /// <param name="forWhite"></param>
        /// <returns></returns>
        public IEnumerable<SingleMove> ValidMovesQuick(bool forWhite)
        {
            var ownKing = GetKingLocationOrDefault(forWhite);
            if (ownKing.Equals((-1, -1)))
            {
                if (_board.Shared.Testing)
                {
                    foreach (var pseudo in PseudoMoves(forWhite, false)) yield return pseudo;
                }
                else throw new ArgumentException("Cannot generate valid moves without own king");
            }

            var isCheck = _board.IsCheck(!forWhite);
            foreach (var piece in _board.PieceQuery.GetColor(forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (isCheck)
                    {
                        // Only allow moves that don't result in check
                        // TODO most probably here is lot's to improve
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (!newBoard.IsCheck(!forWhite))
                        {
                            yield return singleMove;
                        }
                    }
                    else
                    {
                        if (Validator.IsLegalMove(singleMove, _board, piece, ownKing))
                        {
                            yield return singleMove;
                        }
                    }
                }
            }

            if (!isCheck)
            {
                // See castling last, as there might be cutoffs earlier
                // Allowed only if not currently not in check
                foreach (var castlingMove in CastlingMoves(forWhite))
                {
                    yield return castlingMove;
                }
            }
        }

        public IEnumerable<SingleMove> ValidMovesQuickWithoutCastling(bool forWhite)
        {
            var ownKing = GetKingLocationOrDefault(forWhite);
            if (ownKing.Equals((-1, -1)))
            {
                if (_board.Shared.Testing)
                {
                    foreach (var pseudo in PseudoMoves(forWhite, false)) yield return pseudo;
                }
                else throw new ArgumentException("Cannot generate valid moves without own king");
            }

            var isCheck = _board.IsCheck(!forWhite);
            foreach (var piece in _board.PieceQuery.GetColor(forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (isCheck)
                    {
                        // Only allow moves that don't result in check
                        // TODO most probably here is lot's to improve
                        var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                        if (!newBoard.IsCheck(!forWhite))
                        {
                            yield return singleMove;
                        }
                    }
                    else
                    {
                        if (Validator.IsLegalMove(singleMove, _board, piece, ownKing))
                        {
                            yield return singleMove;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Only called from UI
        /// </summary>
        public IEnumerable<SingleMove> ValidMovesForPiece((int column, int row) position)
        {
            // TODO testing version, if no king
            var piece = _board.ValueAtDefinitely(position);
            var forWhite = piece.IsWhite;
            var ownKing = GetKingLocationOrDefault(forWhite);
            var isCheck = _board.IsCheck(!forWhite);

            foreach (var singleMove in piece.Moves(_board))
            {
                if (isCheck)
                {
                    // Only allow moves that don't result in check
                    var newBoard = BoardFactory.CreateFromMove(_board, singleMove);
                    if (!newBoard.IsCheck(!forWhite))
                    {
                        yield return singleMove;
                    }
                }
                else
                {
                    if (Validator.IsLegalMove(singleMove, _board, piece, ownKing))
                    {
                        yield return singleMove;
                    }
                }
            }

            if (piece.Identity == 'K' && !isCheck)
            {
                // See castling last, as there might be cutoffs earlier
                // Allowed only if not currently not in check
                foreach (var castlingMove in CastlingMoves(forWhite))
                {
                    yield return castlingMove;
                }
            }
        }

        /// <summary>
        /// Generate all available moves without any validation
        /// </summary>
        private IEnumerable<SingleMove> PseudoMoves(bool forWhite, bool includeCastling)
        {
            foreach (var piece in _board.PieceQuery.GetColor(forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    yield return singleMove;
                }
            }

            if(includeCastling)
            {
                foreach (var castlingMove in CastlingMoves(forWhite))
                {
                    yield return castlingMove;
                }
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
            IList<SingleMove> list = ValidMovesQuick(forWhite).ToList();

            if (heavyOrdering) return MoveOrdering.SortMovesByEvaluation(list, _board, forWhite);
            return MoveOrdering.SortMovesByGuessWeight(list, _board, forWhite);
        }

        /// <summary>
        /// Find every possible move for every piece for given color.
        /// Because sorting, full list returned.
        /// </summary>
        /// <param name="forWhite"></param>
        /// <param name="heavyOrdering">Sort by light guess weight vs evaluate each new position.</param>
        /// <returns></returns>
        public void MovesWithOrderingSpan(bool forWhite, bool heavyOrdering, Span<MoveStruct> moveSpan, out int spanLength)
        {
            IList<SingleMove> moveList = ValidMovesQuick(forWhite).ToList();
            spanLength = moveList.Count;

            if (heavyOrdering) moveList = MoveOrdering.SortMovesByEvaluation(moveList, _board, forWhite);
            else moveList = MoveOrdering.SortMovesByGuessWeight(moveList, _board, forWhite);

            FillMoveSpan(moveSpan, moveList);
        }
        
        public static void FillSpan<T>(Span<T> span, IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                span[i] = list[i];
            }
        }

        public static void FillMoveSpan(Span<MoveStruct> span, IList<SingleMove> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                span[i] = SingleMoveFactory.Create(list[i]);
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
            foreach (var piece in _board.PieceQuery.GetColor(forWhiteAttacker))
            {
                foreach (var singleMove in piece.PseudoCaptureMoves(_board))
                {
                    yield return singleMove;
                }
            }
        }

        public IEnumerable<SingleMove> GetOrCreateAttackMoves(bool forWhiteAttacker)
        {
            // TODO cache attack moves or cache each attack square to dictionary
            foreach (var piece in _board.PieceQuery.GetColor(forWhiteAttacker))
            {
                foreach (var singleMove in piece.PseudoCaptureMoves(_board))
                {
                    yield return singleMove;
                }
            }
        }

        public bool IsKingCurrentlyAttacked(bool whiteKing)
        {
            var king = GetKingLocationOrDefault(whiteKing);

            // Testing
            if (king.Equals((-1, -1))) return true;

            foreach (var piece in _board.PieceQuery.GetColor(!whiteKing))
            {
                if (piece.CanAttackQuick(king, _board))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSquareCurrentlyAttacked(bool whiteAttacker, (int column, int row) target)
        {
            foreach (var piece in _board.PieceQuery.GetColor(whiteAttacker))
            {
                if (piece.CanAttackQuick(target, _board))
                {
                    return true;
                }
            }

            return false;
        }

        public IList<SliderAttack>? SliderAttacksCached { get; set; } = null;
        public IList<SliderAttack> GetOrCreateSliders(bool forWhite)
        {
            if (SliderAttacksCached != null) return SliderAttacksCached;

            IList<SliderAttack> list = new List<SliderAttack>();
            var king = GetKingLocationOrDefault(!forWhite);
            if (king.Equals((-1, -1)))
            {
                SliderAttacksCached = list;
                return list;
            }

            foreach (var piece in _board.PieceQuery.GetColor(forWhite))
            {
                if (piece.TryCreateSliderAttack(_board, king, out var sliderAttack))
                {
                    list.Add(sliderAttack);
                }
            }

            SliderAttacksCached = list;
            return list;
        }

        public IEnumerable<SliderAttack> EnumerateSliders(bool forWhite)
        {
            var king = GetKingLocationOrDefault(!forWhite);
            if (king.Equals((-1, -1))) yield break;

            foreach (var piece in _board.PieceQuery.GetColor(forWhite))
            {
                if (piece.TryCreateSliderAttack(_board, king, out var sliderAttack))
                {
                    yield return sliderAttack;
                }
            }
        }
    }
}
