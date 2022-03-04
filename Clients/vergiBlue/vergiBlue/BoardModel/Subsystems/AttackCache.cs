using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace vergiBlue.BoardModel.Subsystems
{
    /// <summary>
    /// When moves are generated for A, cache capture moves. When moves are generated for B, use A cache as attack model.
    ///
    /// Situations:
    /// * See immediate attack squares for next move gen
    /// * See "king-under-attack" squares. Dealt with either capturing attacker, moving piece on attack line or moving king away
    /// * See sliding attackers, that have line on king but pinned piece on the way. Deny moving pinned piece.
    /// </summary>
    public class AttackCache
    {
        private List<KingUnderSliderAttack>? KingSliderAttack { get; } = null;

        /// <summary>
        /// All squares that had capture opportunity. Includes pawn attacks.
        /// </summary>
        public HashSet<(int column, int row)> CaptureTargets { get; set; } = new();

        /// <summary>
        /// Pre-game initialization
        /// </summary>
        public AttackCache()
        {

        }

        public AttackCache(List<SingleMove> pseudoAttacks, List<KingUnderSliderAttack> kingPseudos)
        {
            CaptureTargets = pseudoAttacks.Select(m => m.NewPos).ToHashSet();

            KingSliderAttack = kingPseudos;
        }

        /// <summary>
        /// If this move is made, king should not be left to check
        /// </summary>
        /// <param name="move">Move about to execute</param>
        /// <param name="board">Board in pre-move state</param>
        /// <returns></returns>
        public bool IsValidMove(SingleMove move, IBoard board)
        {
            // Return as soon as invalid move discovered
            var piece = board.ValueAtDefinitely(move.PrevPos);
            if (piece.Identity == 'K')
            {
                if (CaptureTargets.Contains(move.NewPos))
                {
                    return false;
                }
            }

            if (KingSliderAttack != null)
            {
                foreach (var sliderAttack in KingSliderAttack)
                {
                    if (!sliderAttack.IsGuarded)
                    {
                        // Protection from direct sliding attacker
                        // E.g. move king away or pawn in front or capture queen
                        // 8       k  
                        // 7  
                        // 6K      q
                        // 5  Pp   
                        // 4
                        // 3   
                        // 2
                        // 1       
                        //  ABCDEFGH
                        if (piece.Identity == 'K')
                        {
                            if (sliderAttack.AttackLine.Contains(move.NewPos))
                            {
                                // Still along attack line
                                return false;
                            }
                        }
                        else
                        {
                            if (sliderAttack.AttackLine.Contains(move.NewPos) || move.NewPos == sliderAttack.Attacker)
                            {
                                // Ok
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
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
                        if (sliderAttack.AttackLine.Contains(move.PrevPos))
                        {
                            // Only valid if moves to attack line or capture attacker
                            if (sliderAttack.AttackLine.Contains(move.NewPos) || move.NewPos == sliderAttack.Attacker)
                            {
                                // Ok
                            }
                            else
                            {
                                return false;
                            }
                        }

                        // TODO known bug - en passant also removes other pasn
                        // Protection from direct sliding attacker
                        // E.g. move king away or pawn in front or capture queen
                        // 8       k  
                        // 7   
                        // 6   o
                        // 5K Pp     q     
                        // 4
                        // 3   
                        // 2
                        // 1       
                        //  ABCDEFGH
                    }
                }
            }
            
            return true;
        }

        public List<(int column, int row)> SlideTargets()
        {
            var result = new List<(int column, int row)>();
            if (KingSliderAttack != null)
            {
                foreach (var attack in KingSliderAttack)
                {
                    result.AddRange(attack.AttackLine);
                    result.AddRange(attack.BehindKing);
                }
            }

            return result;
        }
    }
    
    /// <summary>
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
    public class KingUnderSliderAttack
    {
        public bool IsGuarded => GuardPiece != (-1, -1);
        public bool WhiteAttacking { get; set; }
        public (int column, int row) Attacker { get; set; }
        public (int column, int row) GuardPiece { get; set; } = (-1, -1);
        public (int column, int row) King { get; set; }

        /// <summary>
        /// All squares leading to king, including king
        /// </summary>
        public HashSet<(int column, int row)> AttackLine { get; set; } = new();
        public HashSet<(int column, int row)> BehindKing { get; set; } = new();
    }
}
