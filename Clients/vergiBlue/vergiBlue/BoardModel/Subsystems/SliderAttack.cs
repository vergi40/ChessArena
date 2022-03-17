using System.Collections.Generic;
using System.Linq;

namespace vergiBlue.BoardModel.Subsystems.Attacking
{
    /// <summary>
    /// Slider attack to targeting king
    /// 
    /// If not guarded, resolved by:
    /// 1. Move king out of AttackLine
    /// 2. Move piece in AttackLine
    /// 3. Capture Attacker
    /// 
    /// Guarded resolved by:
    /// 1. Don't move guard piece
    /// 2. Move along AttackLine
    /// 3. Capture Attacker
    /// </summary>
    public class SliderAttack
    {
        public bool WhiteAttacking { get; set; }
        public (int column, int row) Attacker { get; set; }


        public (int column, int row) King { get; set; }

        /// <summary>
        /// All squares leading to king, including king
        /// </summary>
        public HashSet<(int column, int row)> AttackLine { get; set; } = new();
    }
}