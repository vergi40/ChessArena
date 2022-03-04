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

        public AttackCache WhiteAttackCache { get; private set; }
        public AttackCache BlackAttackCache { get; private set; }

        /// <summary>
        /// Pieces not known - no attack cache
        /// </summary>
        /// <param name="board"></param>
        public MoveGenerator(IBoard board)
        {
            _board = board;

            WhiteAttackCache = new AttackCache();
            BlackAttackCache = new AttackCache();
        }

        /// <summary>
        /// Copy previous attack caches
        /// </summary>
        /// <param name="board"></param>
        /// <param name="other"></param>
        public MoveGenerator(IBoard board, MoveGenerator other)
        {
            _board = board;
            WhiteAttackCache = other.WhiteAttackCache;
            BlackAttackCache = other.BlackAttackCache;
        }
        
        public AttackCache GetAttacks(bool forWhite)
        {
            return forWhite ? WhiteAttackCache : BlackAttackCache;
        }

        /// <summary>
        /// Use only if known that no further moves are generated after executing these moves. Otherwise refer <see cref="GenerateMovesAndUpdateCache"/>
        /// IEnumerable should be utilized when there are cutoff etc changes.
        /// </summary>
        /// <param name="forWhite"></param>
        /// <param name="kingInDanger">Obsolete: Validate check for each move</param>
        /// <returns></returns>
        public IEnumerable<SingleMove> MovesQuick(bool forWhite, bool kingInDanger = false)
        {
            foreach (var castlingMove in CastlingMoves(forWhite))
            {
                yield return castlingMove;
            }

            foreach (var singleMove in MovesQuickWithoutCastling(forWhite, kingInDanger))
            {
                yield return singleMove;
            }
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

            var attacksModel = GetAttacks(!isWhite);
            foreach (var singleMove in piece.Moves(_board))
            {
                if (attacksModel.IsValidMove(singleMove, _board))
                {
                    yield return singleMove;
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
                var attackModel = GetAttacks(!forWhite);
                var attackSquares = attackModel.CaptureTargets;
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
        /// <returns></returns>
        public IList<SingleMove> MovesWithOrdering(bool forWhite, bool heavyOrdering)
        {
            var list = GenerateMovesAndUpdateCache(forWhite).ToList();

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
            var attacksModel = GetAttacks(!forWhite);
            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                foreach (var singleMove in piece.Moves(_board))
                {
                    if (kingInDanger)
                    {
                        if (!attacksModel.IsValidMove(singleMove, _board))
                        {
                            continue;
                        }
                    }

                    yield return singleMove;
                }
            }
        }

        /// <summary>
        /// Either run this or <see cref="GenerateMovesAndUpdateCache"/> before next move to refresh attack cache
        /// </summary>
        /// <param name="updateWhiteAttacks"></param>
        public void UpdateAttackCache(bool updateWhiteAttacks)
        {
            // TODO simplify
            _ = GenerateMovesAndUpdateCache(!updateWhiteAttacks).ToList();
        }

        /// <summary>
        /// Simulatenously generate valid moves and add pseudo capture moves to attack model
        /// </summary>
        public IEnumerable<SingleMove> GenerateMovesAndUpdateCache(bool forWhite)
        {
            foreach (var castlingMove in CastlingMoves(forWhite))
            {
                yield return castlingMove;
            }

            foreach (var singleMove in GenerateMovesWithoutCastlingAndUpdateCache(forWhite))
            {
                yield return singleMove;
            }
        }

        /// <summary>
        /// Simulatenously generate valid moves and add pseudo capture moves to attack model
        /// </summary>
        public IList<SingleMove> GenerateMovesWithoutCastlingAndUpdateCache(bool forWhite)
        {
            var validMoves = new List<SingleMove>();
            var attackMoves = new List<SingleMove>();
            var lines = new List<KingUnderSliderAttack>();
            // TODO list version for main moves generation, ienumerable version for quick generation

            var attacksModel = GetAttacks(!forWhite);

            foreach (var piece in _board.PieceList.Where(p => p.IsWhite == forWhite))
            {
                if (piece.Identity == 'P')
                {
                    foreach (var normalMove in piece.PawnNormalMoves(_board))
                    {
                        if (attacksModel.IsValidMove(normalMove, _board))
                        {
                            validMoves.Add(normalMove);
                        }
                    }

                    foreach (var captureMove in piece.PawnPseudoCaptureMoves(_board))
                    {
                        var targetPos = captureMove.NewPos;
                        var valueAt = _board.ValueAt(targetPos);

                        // Check pseudo moves that have valid capture
                        if (valueAt != null && valueAt.IsWhite != forWhite)
                        {
                            if (attacksModel.IsValidMove(captureMove, _board))
                            {
                                validMoves.Add(captureMove);
                            }
                        }
                        attackMoves.Add(captureMove);
                    }
                }
                else
                {
                    if (piece.TryFindPseudoKingCapture(_board, out var kingAttack))
                    {
                        lines.Add(kingAttack);
                    }

                    foreach (var singleMove in piece.Moves(_board))
                    {

                        if (attacksModel.IsValidMove(singleMove, _board))
                        {
                            validMoves.Add(singleMove);
                        }
                        attackMoves.Add(singleMove);
                    }
                }
            }

            if (forWhite)
            {
                WhiteAttackCache = new AttackCache(attackMoves, lines);
            }
            else
            {
                BlackAttackCache = new AttackCache(attackMoves, lines);
            }

            return validMoves;
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
        /// </summary>
        public IEnumerable<(int column, int row)> AttackedPositions(bool forWhiteAttacker)
        {
            var attackModel = GetAttacks(forWhiteAttacker);
            return attackModel.CaptureTargets;
        }
    }
}
