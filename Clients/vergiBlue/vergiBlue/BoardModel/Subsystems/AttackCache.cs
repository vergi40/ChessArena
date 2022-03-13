using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using vergiBlue.Pieces;

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
        private DirectAttackMap DirectAttackMap { get; } = new DirectAttackMap();
        private DirectAttackMap KingDirectAttackMap { get; } = new DirectAttackMap();
        private List<KingUnderSliderAttack> KingSliderAttack { get; } = new();

        /// <summary>
        /// Own pieces that are guarded by other piece. Use for validating if king can capture
        /// </summary>
        private HashSet<(int column, int row)> GuardedMap { get; } = new();

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

        public AttackCache(List<SingleMove> pseudoAttackMoves, List<KingUnderSliderAttack> kingPseudos, (int column, int row) opponentKing)
        {
            foreach (var pseudoAttack in pseudoAttackMoves)
            {
                if (pseudoAttack.SoftTarget)
                {
                    GuardedMap.Add(pseudoAttack.NewPos);
                }
                else if (pseudoAttack.NewPos == opponentKing)
                {
                    KingDirectAttackMap.Add(pseudoAttack);
                }
                else
                {
                    DirectAttackMap.Add(pseudoAttack);
                }
            }

            CaptureTargets = DirectAttackMap.AllTargets().Concat(KingDirectAttackMap.AllTargets()).ToHashSet();

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
            // If king is targeted by capture: 
            // * Either move has to put king safe
            // * Or attacker captured
            // * Or move piece in attack line
            
            // Don't move king to capture square

            // Don't move piece positioned to indirect slider attack line
            

            // Return as soon as invalid move discovered
            var kingUnderDirectAttack = KingDirectAttackMap.TargetAttackerDict.Any();
            var piece = board.ValueAtDefinitely(move.PrevPos);

            if (kingUnderDirectAttack)
            {
                if (KingMovedOutOfDirectAttacks(move, board)) return true; 
                if(KingAttackerCaptured(move, piece)) return true;
                if(KingSliderAttackerBlocked(move, piece)) return true;
                return false;
            }


            if (piece.Identity == 'K')
            {
                if (KingMovedToDirectAttack(move)) return false;
                return true;
            }

            // Direct slider attacks can be resolved with other pieces also

            // Moving piece is not king
            if (!IndirectSliderAttackResolved(move))
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Prerequisite: piece not king
        /// </summary>
        private bool IndirectSliderAttackResolved(SingleMove move)
        {
            foreach (var sliderAttack in KingSliderAttack)
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
        private bool KingSliderAttackerBlocked(SingleMove move, PieceBase piece)
        {
            if (!KingSliderAttack.Any()) return false;
            if (piece.Identity == 'K') return false;
            var unguardedList = KingSliderAttack.Where(a => !a.IsGuarded).ToList();
            if (unguardedList.Count != 1) return false;

            var unguarded = unguardedList.Single();
            if (unguarded.AttackLine.Contains(move.NewPos)) return true;
            return false;
        }

        /// <summary>
        /// Out of capture targets. Out of attack lines (they continue behind king)
        /// </summary>
        private bool KingMovedOutOfDirectAttacks(SingleMove move, IBoard board)
        {
            var piece = board.ValueAtDefinitely(move.PrevPos);
            if (piece.Identity == 'K')
            {
                if (CaptureTargets.Contains(move.NewPos))
                {
                    // Still in danger
                    return false;
                }

                if (GuardedMap.Contains(move.NewPos))
                {
                    // Illegal as the guard will capture king
                    return false;
                }

                foreach (var sliderAttack in KingSliderAttack.Where(a => !a.IsGuarded))
                {
                    if (sliderAttack.BehindKing.Contains(move.NewPos))
                    {
                        return false;
                    }
                }

                // All clear for king move
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prerequisite: single attacker
        /// </summary>
        private bool KingAttackerCaptured(SingleMove move, PieceBase piece)
        {
            var attackers = KingDirectAttackMap.AllAttackers().ToList();
            if (attackers.Count == 0)
            {
                // Not sure if this is ever possible, as "king under direct attack"
                throw new ArgumentException("Logical error: there should be attackers if king under direct attack");
                return true;
            }
            if (attackers.Count == 1)
            {
                // Capture attacker (with king or any piece)
                // TODO is valid move if attacker is guarded anyway?

                var attacker = attackers.Single();
                if (attacker == move.NewPos)
                {
                    // King can't capture guarded piece
                    if (piece.Identity == 'K' && GuardedMap.Contains(attacker))
                    {
                        return false;
                    }

                    return true;
                }
            }
            return false;
        }

        private bool KingMovedToDirectAttack(SingleMove move)
        {
            // E.g. queen attack. Can't move pawn b2, it's guarding
            // Can move king away to a1 a3
            // 8 k   
            // 7           
            // 6       
            // 5 
            // 4           
            // 3 o x       
            // 2 K P q P   
            // 1 o x           
            //   A B C D E F G H
            if (CaptureTargets.Contains(move.NewPos))
            {
                return true;
            }

            if (GuardedMap.Contains(move.NewPos))
            {
                return true;
            }

            return false;
        }

        public List<(int column, int row)> SlideTargets()
        {
            var result = new List<(int column, int row)>();
            foreach (var attack in KingSliderAttack)
            {
                result.AddRange(attack.AttackLine);
                result.AddRange(attack.BehindKing);
            }

            return result;
        }
    }

    public class DirectAttackMap
    {
        /// <summary>
        /// [capture target position][list of attacker positions]
        /// </summary>
        public Dictionary<(int column, int row), HashSet<(int column, int row)>> TargetAttackerDict { get; set; } = new();

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
                var attackerList = new HashSet<(int column, int row)>();
                attackerList.Add(move.PrevPos);
                TargetAttackerDict.Add(move.NewPos, attackerList);
            }
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
        public bool IsGuarded
        {
            get
            {
                if(GuardPiece != (-1, -1)) return true;
                if (HasEnPassantPawnOpportunity) return true;
                return false;
            }
        }

        public bool WhiteAttacking { get; set; }
        public (int column, int row) Attacker { get; set; }
        public (int column, int row) GuardPiece { get; set; } = (-1, -1);
        public (int column, int row) King { get; set; }

        /// <summary>
        /// Only valid if attack row contains both: opponent enpassant pawn and next to it own pawn
        /// </summary>
        public bool HasEnPassantPawnOpportunity { get; set; }

        /// <summary>
        /// All squares leading to king, including king
        /// </summary>
        public HashSet<(int column, int row)> AttackLine { get; set; } = new();
        public HashSet<(int column, int row)> BehindKing { get; set; } = new();
    }
}
