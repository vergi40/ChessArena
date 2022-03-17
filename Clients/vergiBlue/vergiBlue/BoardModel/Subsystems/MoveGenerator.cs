using System.Collections;
using System.Collections.Generic;
using System.Linq;
using vergiBlue.Algorithms;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems
{
    public class MoveGenerator
    {
        private IBoard _board { get; }

        public MoveGenerator(IBoard board)
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

        /// <summary>
        /// Should be merged with "movesquick"
        /// </summary>
        /// <param name="forWhite"></param>
        /// <returns></returns>
        public IEnumerable<SingleMove> ValidMovesQuick(bool forWhite)
        {
            return MovesQuick(forWhite, true);
        }

        public IEnumerable<SingleMove> MovesForPiece((int column, int row) position)
        {
            var piece = _board.ValueAtDefinitely(position);
            var isWhite = piece.IsWhite;

            if(piece.Identity == 'K')
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

        public IList<SingleMove> MovesWithTranspositionOrder(bool forWhite, bool kingInDanger = false)
        {
            // Priority moves like known cutoffs
            var priorityList = new List<SingleMove>();
            var otherList = new List<SingleMove>();
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

                    // Check if move has transposition data
                    // Maximizing player needs lower bound moves
                    // Minimizing player needs upper bound moves
                    var transposition = _board.Shared.Transpositions.GetTranspositionForMove(_board, singleMove);
                    if (transposition != null)
                    {
                        if ((forWhite && transposition.Type == NodeType.LowerBound) ||
                            (!forWhite && transposition.Type == NodeType.UpperBound))
                        {
                            Diagnostics.IncrementPriorityMoves();
                            priorityList.Add(singleMove);
                            continue;
                        }
                    }
                    otherList.Add(singleMove);
                }
            }

            priorityList.AddRange(MoveOrdering.SortMovesByGuessWeight(otherList, _board, forWhite));
            return priorityList;
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
                foreach (var singleMove in piece.PseudoCaptureMoves(_board))
                {
                    yield return singleMove;
                }
            }
        }
    }
}
