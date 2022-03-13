using System;
using System.Collections.Generic;

namespace vergiBlue.BoardModel.Subsystems.Attacking
{
    public class GuardedMap
    {
        /// <summary>
        /// [guarded position][guard positions]
        /// </summary>
        private Dictionary<(int column, int row), HashSet<(int column, int row)>> _guardedDict { get; set; } = new();

        /// <summary>
        /// [guard position][guarded positions]
        /// Only used for post-update cleaning
        /// </summary>
        private Dictionary<(int column, int row), HashSet<(int column, int row)>> _guardDict { get; set; } = new();

        /// <summary>
        /// Prerequisite: Move is soft move
        /// </summary>
        /// <param name="move"></param>
        public void Add(SingleMove move)
        {
            if (!move.SoftTarget)
                throw new ArgumentException("Only soft moves (capture own piece) supported in GuardedMap");

            // key = new position
            // values = prev pos
            if (_guardedDict.TryGetValue(move.NewPos, out var value))
            {
                value.Add(move.PrevPos);
            }
            else
            {
                var guardList = new HashSet<(int column, int row)> { move.PrevPos };
                _guardedDict.Add(move.NewPos, guardList);
            }

            if (_guardDict.TryGetValue(move.PrevPos, out var guardDictValue))
            {
                guardDictValue.Add(move.NewPos);
            }
            else
            {
                var guardList = new HashSet<(int column, int row)> { move.NewPos };
                _guardDict.Add(move.PrevPos, guardList);
            }
        }

        public bool IsGuarded((int column, int row) position)
        {
            return _guardedDict.ContainsKey(position);
        }

        /// <summary>
        /// Remove all references that guard piece was guarding
        /// </summary>
        public void Remove((int column, int row) guardPosition)
        {
            if (_guardDict.TryGetValue(guardPosition, out var guardedPositions))
            {
                foreach (var guardedPosition in guardedPositions)
                {
                    _guardedDict.Remove(guardedPosition);
                }
            }
        }
    }
}
