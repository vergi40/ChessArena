using System.Collections.Generic;
using System.Linq;

namespace vergiBlue.BoardModel.Subsystems.Attacking
{
    public class DirectAttackMap
    {
        /// <summary>
        /// [capture target position][attacker positions]
        /// </summary>
        public Dictionary<(int column, int row), HashSet<(int column, int row)>> TargetAttackerDict { get; set; } = new();

        /// <summary>
        /// [attacker position][capture target positions]
        /// Only used for post-update cleaning
        /// </summary>
        protected Dictionary<(int column, int row), HashSet<(int column, int row)>> _attackerTargetDict { get; set; } = new();

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

            if (_attackerTargetDict.TryGetValue(move.PrevPos, out var targetsValue))
            {
                targetsValue.Add(move.NewPos);
            }
            else
            {
                var targetsList = new HashSet<(int column, int row)> { move.NewPos };
                _attackerTargetDict.Add(move.PrevPos, targetsList);
            }
        }

        public IEnumerable<(int column, int row)> AllTargets()
        {
            return TargetAttackerDict.Select(d => d.Key);
        }

        public IEnumerable<(int column, int row)> AllAttackers()
        {
            return _attackerTargetDict.Select(d => d.Key);
        }

        public IEnumerable<(int column, int row)> Attackers((int column, int row) target)
        {
            foreach (var attacker in TargetAttackerDict[target])
            {
                yield return attacker;
            }
        }

        /// <summary>
        /// Remove attackers capture targets
        /// </summary>
        public void Remove((int column, int row) attackerPosition)
        {
            if (_attackerTargetDict.TryGetValue(attackerPosition, out var targets))
            {
                foreach (var targetPosition in targets)
                {
                    TargetAttackerDict.Remove(targetPosition);
                }
            }
        }

        public DirectAttackMap Clone()
        {
            return ShallowCopy();
        }

        private DirectAttackMap ShallowCopy()
        {
            var map = new DirectAttackMap();
            map.TargetAttackerDict =
                new Dictionary<(int column, int row), HashSet<(int column, int row)>>(TargetAttackerDict);
            map._attackerTargetDict =
                new Dictionary<(int column, int row), HashSet<(int column, int row)>>(_attackerTargetDict);
            return map;
        }

        private DirectAttackMap DeepCopy()
        {
            var map = new DirectAttackMap();
            map.TargetAttackerDict = TargetAttackerDict.ToDictionary(
                item => item.Key,
                item => new HashSet<(int column, int row)>(item.Value));

            map._attackerTargetDict = _attackerTargetDict.ToDictionary(
                item => item.Key,
                item => new HashSet<(int column, int row)>(item.Value));
            return map;
        }
    }
}