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
    ///
    /// If there are 2 or more guard pieces, this is redundant and not created
    /// TODO open for side effects
    /// </summary>
    public class SliderAttack
    {
        /// <summary>
        /// No direct line of sight to king
        /// </summary>
        public bool IsGuarded
        {
            get
            {
                if(OpponentPiece != (-1, -1)) return true;
                if(GuardPieces.Any()) return true;
                return false;
            }
        }

        public bool WhiteAttacking { get; set; }
        public (int column, int row) Attacker { get; set; }

        /// <summary>
        /// Own piece on the way
        /// </summary>
        public HashSet<(int column, int row)> GuardPieces { get; set; } = new();

        /// <summary>
        /// Opponent piece on the way
        /// </summary>
        public (int column, int row) OpponentPiece { get; set; } = (-1, -1);


        public (int column, int row) King { get; set; }

        /// <summary>
        /// Only valid if attack row contains own pawn open for en passant capture
        /// </summary>
        public bool HasEnPassantPawnOpportunity { get; set; }

        /// <summary>
        /// All squares leading to king, including king
        /// </summary>
        public HashSet<(int column, int row)> AttackLine { get; set; } = new();
        public HashSet<(int column, int row)> BehindKing { get; set; } = new();
    }
}