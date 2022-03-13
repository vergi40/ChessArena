using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems.Attacking
{
    public static class ThreatResolver
    {
        /// <summary>
        /// Prerequisite: piece not king
        /// </summary>
        public static bool IndirectSliderAttackResolved(SingleMove move, List<SliderAttack> sliderAttacks)
        {
            foreach (var sliderAttack in sliderAttacks)
            {
                if (sliderAttack.AttackLine.Contains(move.PrevPos) || (sliderAttack.HasEnPassantPawnOpportunity && move.EnPassant))
                {
                    // E.g. cannot move guarding pawn
                    // 7    
                    // 6           
                    // 5       
                    // 4             b
                    // 3           x
                    // 2       ^ x
                    // 1 P P P P      
                    // 0     K        
                    //   0 1 2 3 4 5 6 7 
                    
                    // Only valid if moves to attack line or capture attacker
                    if (sliderAttack.AttackLine.Contains(move.NewPos) || move.NewPos == sliderAttack.Attacker)
                    {
                        // Ok
                    }
                    else if (sliderAttack.HasEnPassantPawnOpportunity)
                    {
                        // niche case
                        // En passant will leave king open
                        // 8K     K  
                        // 7   
                        // 6      o
                        // 5K   P p      r     
                        // 4
                        // 3         b
                        // 2
                        // 1      r
                        //  A B C D E F G H
                        if (move.EnPassant)
                        {
                            return false;
                        }
                        // In this case normal forward movement ok
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// If there is direct sliding attack and it's blocked, true.
        /// Prerequisite: moving piece not king
        /// </summary>
        public static bool KingSliderAttackerBlocked(SingleMove move, PieceBase piece, List<SliderAttack> sliderAttacks)
        {
            if (!sliderAttacks.Any()) return false;
            if (piece.Identity == 'K') return false;
            var unguardedList = sliderAttacks.Where(a => !a.IsGuarded).ToList();
            if (unguardedList.Count != 1) return false;

            var unguarded = unguardedList.Single();
            if (unguarded.AttackLine.Contains(move.NewPos)) return true;
            return false;
        }
    }
}
