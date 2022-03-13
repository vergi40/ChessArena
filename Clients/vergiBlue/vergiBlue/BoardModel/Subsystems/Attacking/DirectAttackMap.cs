using System.Collections.Generic;
using System.Linq;

namespace vergiBlue.BoardModel.Subsystems.Attacking
{
    public class DirectAttackMap
    {
        /// <summary>
        /// [capture target position][list of attacker positions]
        /// </summary>
        public Dictionary<(int column, int row), HashSet<(int column, int row)>> TargetAttackerDict { get; set; } = new();

        /// <summary>
        /// [attacker position][capture target position]
        /// Only used for post-update cleaning
        /// </summary>
        private Dictionary<(int column, int row), (int column, int row)> _attackerTargetDict { get; set; } = new();

        public void Add(SingleMove move)
        {
            // key = new position
            // values = prev pos
            if (TargetAttackerDict.TryGetValue(move.NewPos, out var value))
            {
                value.Add(move.PrevPos);
            }
            else
            {
                var attackerList = new HashSet<(int column, int row)> { move.PrevPos };
                TargetAttackerDict.Add(move.NewPos, attackerList);
            }

            _attackerTargetDict.TryAdd(move.PrevPos, move.NewPos);
        }

        public IEnumerable<(int column, int row)> AllTargets()
        {
            return TargetAttackerDict.Select(d => d.Key);
        }

        public IEnumerable<(int column, int row)> AllAttackers()
        {
            foreach (var keyValue in TargetAttackerDict)
            {
                foreach (var attacker in keyValue.Value)
                {
                    yield return attacker;
                }
            }
        }

        public IEnumerable<(int column, int row)> Attackers((int column, int row) target)
        {
            foreach (var attacker in TargetAttackerDict[target])
            {
                yield return attacker;
            }
        }

        /// <summary>
        /// Done move from previous pos to possibly capture target pos
        /// </summary>
        /// <param name="move"></param>
        public void Remove(SingleMove move)
        {
            if (_attackerTargetDict.TryGetValue(move.PrevPos, out var target))
            {
                TargetAttackerDict.Remove(target);
            }
        }
    }
}