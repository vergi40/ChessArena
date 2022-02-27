using System;
using System.Collections.Generic;
using System.Linq;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.SubSystems
{
    /// <summary>
    /// Semi-passive cache-driven tactic to keep updated attack squares.
    /// Attack squares here: All squares all (black/white) pieces can capture.
    ///
    /// In beginning (board construction) attack possibilities for each square are calculated.
    /// Own structure for white and black possibilities. Possibilities are cached.
    /// Each square is marked, which piece has attack possibility there.
    /// * When next move is calculated, if prev and new square links are inspected.
    /// * All pieces linked to prev and new squares are recalculated.
    /// This results in recalculating just 1 or few piece attack squares, instead of all pieces
    /// in every turn.
    ///
    /// Class provides fast dict-based or array-based methods to query square status or show all squares. 
    /// </summary>
    public class AttackSquareMapper
    {
        /// <summary>
        /// Contains every cache linked to each square
        /// </summary>
        private AttackLink[,] Links { get; }

        private List<AttackCache> Whites { get; } = new();
        private List<AttackCache> Blacks { get; } = new();

        /// <summary>
        /// Use only if known that mapper is initialized later
        /// </summary>
        public AttackSquareMapper(){ Links = new AttackLink[0,0]; }

        public AttackSquareMapper(IBoard board)
        {
            Links = new AttackLink[8,8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Links[i, j] = new AttackLink();
                }
            }

            Initialize(board);
        }

        /// <summary>
        /// Constructor for cloning
        /// </summary>
        private AttackSquareMapper(AttackLink[,] clonedLinks)
        {
            Links = clonedLinks;
        }

        /// <summary>
        /// First time initialization. Resource-heavy
        /// </summary>
        /// <param name="board"></param>
        private void Initialize(IBoard board)
        {
            foreach (var piece in board.PieceList)
            {
                var cache = new AttackCache(piece);
                cache.TriggerCacheUpdate(board, Links);
            }
        }

        /// <summary>
        /// Update each cache linked to prev and new position
        /// </summary>
        public void Update(IBoard board, SingleMove move)
        {
            var prev = move.PrevPos;
            var next = move.NewPos;
            
            var needUpdate = new List<AttackCache>();
            foreach (var cache in Links[prev.column, prev.row].PiecesLinkedToSquare.Keys)
            {
                needUpdate.Add(cache);
            }
            foreach (var cache in Links[next.column, next.row].PiecesLinkedToSquare.Keys)
            {
                needUpdate.Add(cache);
            }

            if (move.EnPassant)
            {
                var (col, row) = move.EnPassantOpponentPosition;
                needUpdate.AddRange(Links[col, row].PiecesLinkedToSquare.Keys);
            }
            else if (move.Castling)
            {
                if (move.NewPos.column == 2)
                {
                    // Execute also rook move
                    var row = move.NewPos.row;
                    needUpdate.AddRange(Links[0, row].PiecesLinkedToSquare.Keys);
                    needUpdate.AddRange(Links[3, row].PiecesLinkedToSquare.Keys);
                }
                else if (move.NewPos.column == 6)
                {
                    // Execute also rook move
                    var row = move.NewPos.row;
                    needUpdate.AddRange(Links[7, row].PiecesLinkedToSquare.Keys);
                    needUpdate.AddRange(Links[5, row].PiecesLinkedToSquare.Keys);
                }
            }

            // TODO extra handling, like promotion?

            // Note: maybe handle as enumerable?
            needUpdate = needUpdate.Distinct().ToList();
            
            // Trigger update
            foreach (var cache in needUpdate)
            {
                cache.TriggerCacheUpdate(board, Links);
            }
        }

        public bool IsPositionAttacked((int column, int row) position, bool byWhite)
        {
            return Links[position.column, position.row].HasAttackToSquare(byWhite);
        }

        /// <summary>
        /// IEnumerable with yield return pattern. Contains duplicates.
        /// This way if looped in foreach and match found, extra effort skipped.
        /// </summary>
        public IEnumerable<(int column, int row)> GetAllFor(bool white)
        {
            // TODO to be implemented when needed
            throw new NotImplementedException();

            if (white)
            {
                foreach (var cache in Whites)
                {
                    foreach (var position in cache.AttackSquares)
                    {
                        yield return position;
                    }
                }
            }
            else
            {
                foreach (var cache in Blacks)
                {
                    foreach (var position in cache.AttackSquares)
                    {
                        yield return position;
                    }
                }
            }
        }

        public IReadOnlyList<(int column, int row)> GetAllDistinctFor(bool white)
        {
            return GetAllFor(white).ToList();
        }

        public AttackSquareMapper Clone(IReadOnlyList<PieceBase> pieces)
        {
            // Usefulness of this class depends much how quick the cloning is
            var links = new AttackLink[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    links[i, j] = new AttackLink();
                }
            }

            foreach (var piece in pieces)
            {
                var (column, row) = piece.CurrentPosition;
                links[column, row] = Links[column, row].Clone();
            }

            return new AttackSquareMapper(links);
        }
    }

    /// <summary>
    /// Represents links for single square in board
    /// </summary>
    class AttackLink
    {
        /// <summary>
        /// [Cache item, soft target]
        /// </summary>
        public Dictionary<AttackCache, bool> PiecesLinkedToSquare { get; set; } = new();

        public void AddLink(AttackCache cache, bool softTarget)
        {
            PiecesLinkedToSquare.TryAdd(cache, softTarget);
        }

        public void RemoveLink(AttackCache cache)
        {
            PiecesLinkedToSquare.Remove(cache);
        }

        public bool HasAttackToSquare(bool white)
        {
            if (PiecesLinkedToSquare.Any(c => c.Key.IsWhite == white && !c.Value))
            {
                return true;
            }

            return false;

            // TODO substitute with static. would need to update on each add/remove
        }

        public override string ToString()
        {
            if (!PiecesLinkedToSquare.Any()) return "-";
            return string.Join(", ", PiecesLinkedToSquare.Select(c => c.Key.Identity));
        }

        public AttackLink Clone()
        {
            var dict = new Dictionary<AttackCache, bool>();
            foreach (var entry in PiecesLinkedToSquare)
            {
                var keyClone = entry.Key.Clone();
                dict.Add(keyClone, entry.Value);
            }

            var linkClone = new AttackLink()
            {
                PiecesLinkedToSquare = dict
            };
            return linkClone;
        }
    }

    /// <summary>
    /// Represents all attack and soft squares linked for piece
    /// </summary>
    class AttackCache
    {
        public PieceBase Piece { get; }
        public char Identity => Piece.Identity;
        public bool IsWhite => Piece.IsWhite;
        public HashSet<(int column, int row)> AttackSquares { get; } = new();
        public HashSet<(int column, int row)> SoftSquares { get; } = new();

        public AttackCache(PieceBase piece)
        {
            Piece = piece;
        }

        private AttackCache(PieceBase piece, HashSet<(int column, int row)> attackSquares,
            HashSet<(int column, int row)> softSquares)
        {
            Piece = piece;
            AttackSquares = attackSquares;
            SoftSquares = softSquares;
        }

        public void TriggerCacheUpdate(IBoard board, AttackLink[,] links)
        {
            ClearPreviousLinks(links);

            foreach (var move in Piece.MovesWithSoftTargets(board))
            {
                if (Identity == 'P' && !move.Capture) continue;
                
                var position = move.NewPos;
                if (move.SoftTarget)
                {
                    SoftSquares.Add(position);
                }
                else
                {
                    // Only valid positions
                    AttackSquares.Add(position);
                }

                // It is important to link also soft targets
                links[position.column, position.row].AddLink(this, move.SoftTarget);
            }
        }

        private void ClearPreviousLinks(AttackLink[,] links)
        {
            // Improvement: if really tight optimized, only remove squares that are not part of 
            // updated
            foreach (var (column, row) in AttackSquares.Concat(SoftSquares))
            {
                links[column, row].RemoveLink(this);
            }
            AttackSquares.Clear();
            SoftSquares.Clear();
        }

        public override string ToString()
        {
            return $"{Identity}: iswhite = {IsWhite}. {AttackSquares.Count} attack, {SoftSquares.Count} soft squares";
        }

        public AttackCache Clone()
        {
            var cacheClone = new AttackCache(Piece.CreateCopy(), AttackSquares.ToHashSet(), SoftSquares.ToHashSet());
            return cacheClone;
        }
    }
}
