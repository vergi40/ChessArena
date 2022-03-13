using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems.Attacking
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
        /// <summary>
        /// All direct captures excl. king attacks
        /// </summary>
        private DirectAttackMap DirectAttackMap { get; } = new DirectAttackMap();

        /// <summary>
        /// All direct king attacks
        /// </summary>
        private DirectAttackMap KingDirectAttackMap { get; } = new DirectAttackMap();

        /// <summary>
        /// All attacks or attack possibilities (with piece in the way) to king by sliding piece
        /// </summary>
        private List<SliderAttack> KingSliderAttacks { get; } = new();

        /// <summary>
        /// Own pieces that are guarded by other piece. Use for validating if king can capture
        /// </summary>
        private GuardedMap Guarded { get; } = new();

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

        public AttackCache(List<SingleMove> pseudoAttackMoves, List<SliderAttack> kingSliderAttacks, (int column, int row) opponentKing)
        {
            foreach (var pseudoAttack in pseudoAttackMoves)
            {
                if (pseudoAttack.SoftTarget)
                {
                    Guarded.Add(pseudoAttack);
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

            KingSliderAttacks = kingSliderAttacks;
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
                if(ThreatResolver.KingSliderAttackerBlocked(move, piece, KingSliderAttacks)) return true;
                return false;
            }


            if (piece.Identity == 'K')
            {
                if (KingMovedToDirectAttack(move)) return false;
                return true;
            }

            // Direct slider attacks can be resolved with other pieces also

            // Moving piece is not king
            if (!ThreatResolver.IndirectSliderAttackResolved(move, KingSliderAttacks))
            {
                return false;
            }
            
            return true;
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

                if (Guarded.IsGuarded(move.NewPos))
                {
                    // Illegal as the guard will capture king
                    return false;
                }

                foreach (var sliderAttack in KingSliderAttacks.Where(a => !a.IsGuarded))
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
                    if (piece.Identity == 'K' && Guarded.IsGuarded(attacker))
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

            if (Guarded.IsGuarded(move.NewPos))
            {
                return true;
            }

            return false;
        }

        public List<(int column, int row)> SlideTargets()
        {
            var result = new List<(int column, int row)>();
            foreach (var attack in KingSliderAttacks)
            {
                result.AddRange(attack.AttackLine);
                result.AddRange(attack.BehindKing);
            }

            return result;
        }

        public void UpdateAfterMove(SingleMove move, PieceBase piece, MoveGeneratorV2 moveGenerator)
        {
            // Move = already done move

            // Remove old references

            // private DirectAttackMap DirectAttackMap { get; } = new DirectAttackMap();
            // private DirectAttackMap KingDirectAttackMap { get; } = new DirectAttackMap();
            // private List<SliderAttack> KingSliderAttacks { get; } = new();
            // private HashSet<(int column, int row)> GuardedMap { get; } = new();
            // public HashSet<(int column, int row)> CaptureTargets { get; set; } = new();
            var prevPos = move.PrevPos;

            DirectAttackMap.Remove(move);
            KingDirectAttackMap.Remove(move);
            KingSliderAttacks.RemoveAll(a => a.Attacker.Equals(prevPos));

            Guarded.Remove(prevPos);
            CaptureTargets.Remove(move.NewPos);

            // TODO inspect each slider attack if it needs updating
            // TODO castling

            // Now generate attacks from new position
            var (pseudoAttackMoves, sliderAttack, opponentKing) = moveGenerator.MovesAndSlidersForPiece(piece);
            
            foreach (var pseudoAttack in pseudoAttackMoves)
            {
                if (pseudoAttack.SoftTarget)
                {
                    Guarded.Add(pseudoAttack);
                    continue;
                }
                if (pseudoAttack.NewPos == opponentKing)
                {
                    KingDirectAttackMap.Add(pseudoAttack);
                }
                else
                {
                    DirectAttackMap.Add(pseudoAttack);
                }

                CaptureTargets.Add(pseudoAttack.NewPos);
            }

            if(sliderAttack != null) KingSliderAttacks.Add(sliderAttack);
        }
    }
}
