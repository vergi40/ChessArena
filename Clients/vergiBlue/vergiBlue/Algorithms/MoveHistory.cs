using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard.Interface;

namespace vergiBlue.Algorithms
{
    public class MoveHistory
    {
        /// <summary>
        /// Check if threefold repetition occurs (when the same position occurs three times with the same player to move).
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        public static bool IsDraw(IList<IPlayerMove> moves)
        {
            return IsDraw(moves.Select(m => m.Move).ToList());
        }

        /// <summary>
        /// Check if threefold repetition occurs (when the same position occurs three times with the same player to move).
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        public static bool IsDraw(IList<IMove> moves)
        {
            if (moves.Count > 15)
            {
                var count = moves.Count;
                if (!MovesMatch(moves, count - 1, count - 5)) return false;
                if (!MovesMatch(moves, count - 3, count - 7)) return false;
                if (!MovesMatch(moves, count - 5, count - 9)) return false;
                if (!MovesMatch(moves, count - 7, count - 11)) return false;
                if (!MovesMatch(moves, count - 9, count - 13)) return false;
                if (!MovesMatch(moves, count - 11, count - 15)) return false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if repetition starting to occur (when the same position occurs multiple times times with the same player to move).
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        public static bool IsLeaningToDraw(IList<IMove> moves)
        {
            if (moves.Count > 15)
            {
                var count = moves.Count;
                if (!MovesMatch(moves, count - 2, count - 6)) return false;
                if (!MovesMatch(moves, count - 4, count - 8)) return false;
                if (!MovesMatch(moves, count - 6, count - 10)) return false;
                if (!MovesMatch(moves, count - 8, count - 12)) return false;
                return true;
            }

            return false;
        }

        public static bool MovesMatch(IList<IMove> allMoves, int firstIndex, int secondIndex)
        {
            if (allMoves[firstIndex].StartPosition == allMoves[secondIndex].StartPosition
                && allMoves[firstIndex].EndPosition == allMoves[secondIndex].EndPosition)
            {
                return true;
            }

            return false;
        }
    }
}
