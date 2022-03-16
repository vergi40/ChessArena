using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems.Attacking
{
    public enum CurrentCacheSource
    {
        Empty,

        /// <summary>
        /// Freshly generated during move generation
        /// </summary>
        Generated,

        /// <summary>
        /// After new board is created. Allow read-only operations
        /// </summary>
        FromReference,

        /// <summary>
        /// After new board is created, before move executed
        /// </summary>
        Cloned,

        /// <summary>
        /// Cache updated after ExecuteMove. Out-dated for next update
        /// </summary>
        MoveUpdated
    }

    /// <summary>
    /// Controller should serve appropriate cache type to minimize cloning overhead.
    /// 
    /// Cache update cycle            black    white
    /// 1. generate black moves       generate -
    /// 2. create new board           clone    -
    /// 3. update black attacks       update   -
    /// 4. generate white moves       -        generate
    /// 5. create new board           -        clone
    /// 6. generate white attacks     -        update
    /// </summary>
    public class CacheController
    {
        public Dictionary<bool, IAttackCache> Caches { get; set; } = new();
        public Dictionary<bool, CurrentCacheSource> CacheSource { get; set; } = new();

        /// <summary>
        /// Toggle to true when doing init generation, aka opponent caches will be empty
        /// </summary>
        public bool IsInitializing { get; set; } = false;

        public void InitializeEmpty()
        {
            Caches = new Dictionary<bool, IAttackCache>()
            {
                { true, new AttackCache() },
                { false, new AttackCache() }
            };

            CacheSource = new Dictionary<bool, CurrentCacheSource>()
            {
                { true, CurrentCacheSource.Empty },
                { false, CurrentCacheSource.Empty }
            };
        }

        public void InitializeFromTurnChange(CacheController previous)
        {
            Caches[true] = previous.Caches[true];
            Caches[false] = previous.Caches[false];

            CacheSource[true] = CurrentCacheSource.FromReference;
            CacheSource[false] = CurrentCacheSource.FromReference;
        }

        public IAttackCacheReadOnly Read(bool forWhite)
        {
            if (!IsInitializing && CacheSource[forWhite] == CurrentCacheSource.Empty)
            {
                throw new ArgumentException("Tried to read attack cache, but it's not generated yet.");
            }

            return Caches[forWhite];
        }

        public void Write(bool forWhite, List<SingleMove> attackMoves, List<SliderAttack> sliderAttacks, (int column, int row) opponentKing)
        {
            Caches[forWhite] = new AttackCache(attackMoves, sliderAttacks, opponentKing);
            CacheSource[forWhite] = CurrentCacheSource.Generated;
        }

        public void UpdateAfterMove(SingleMove move, PieceBase piece, MoveGeneratorV2 moveGenerator)
        {
            var forWhite = piece.IsWhite;
            if (CacheSource[forWhite] == CurrentCacheSource.Empty)
            {
                // Init or testing. E.g. in testing running only executeMove sequence

                //throw new ArgumentException("Tried to read attack cache, but it's not generated yet.");
                moveGenerator.UpdateAttackCacheSlow(forWhite);
                return;
            }
            if (CacheSource[forWhite] == CurrentCacheSource.MoveUpdated)
            {
                // Old results. Regenerate
                moveGenerator.UpdateAttackCacheSlow(forWhite);
                return;
            }
            if (CacheSource[forWhite] == CurrentCacheSource.FromReference)
            {
                // Moves generated. After this each move is executed in minimax. Need to clone so original isn't affected

                // Clone so the original reference is not modified
                Caches[forWhite] = Caches[forWhite].Clone();
                CacheSource[forWhite] = CurrentCacheSource.Cloned;
            }

            var cacheToUpdate = Caches[forWhite];
            cacheToUpdate.UpdateAfterMove(move, piece, moveGenerator);

            Caches[forWhite] = cacheToUpdate;
            CacheSource[forWhite] = CurrentCacheSource.MoveUpdated;
        }
    }
}
