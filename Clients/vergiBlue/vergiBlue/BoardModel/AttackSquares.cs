using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
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
    public class AttackSquares
    {
        /// <summary>
        /// Contains every cache linked to each square
        /// </summary>
        private AttackLink[,] Links { get; }

        private List<AttackCache> Whites { get; } = new();
        private List<AttackCache> Blacks { get; } = new();


        public AttackSquares(IBoard board)
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
            foreach (var cache in Links[prev.column, prev.row].CacheList)
            {
                needUpdate.Add(cache);
            }
            foreach (var cache in Links[next.column, next.row].CacheList)
            {
                needUpdate.Add(cache);
            }

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
                    foreach (var position in cache.Squares)
                    {
                        yield return position;
                    }
                }
            }
            else
            {
                foreach (var cache in Blacks)
                {
                    foreach (var position in cache.Squares)
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
    }

    class AttackLink
    {
        public List<AttackCache> CacheList { get; set; } = new();

        public void AddLink(AttackCache cache)
        {
            CacheList.Add(cache);
        }

        public void RemoveLink(AttackCache cache)
        {
            CacheList.Remove(cache);
        }

        public bool HasAttackToSquare(bool white)
        {
            // TODO substitute with static. would need to update on each add/remove
            return CacheList.Any(c => c.IsWhite == white);
        }

        public override string ToString()
        {
            if (!CacheList.Any()) return "-";
            return string.Join(", ", CacheList.Select(c => c.Identity));
        }
    }

    class AttackCache
    {
        public PieceBase Piece { get; }
        public char Identity => Piece.Identity;
        public bool IsWhite => Piece.IsWhite;
        public HashSet<(int column, int row)> Squares { get; } = new();

        public AttackCache(PieceBase piece)
        {
            Piece = piece;
        }

        public void TriggerCacheUpdate(IBoard board, AttackLink[,] links)
        {
            ClearPreviousLinks(links);

            foreach (var move in Piece.Moves(board))
            {
                if (Identity == 'P' && !move.Capture) continue;

                var position = move.NewPos;
                Squares.Add(position);
                links[position.column, position.row].AddLink(this);
            }
        }

        private void ClearPreviousLinks(AttackLink[,] links)
        {
            foreach (var (column, row) in Squares)
            {
                links[column, row].RemoveLink(this);
            }
            Squares.Clear();
        }
    }
}
