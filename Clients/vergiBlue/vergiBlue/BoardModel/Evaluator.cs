using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Algorithms;

namespace vergiBlue.BoardModel
{
    internal class Evaluator
    {
        public static double Evaluate(IBoard board, bool isMaximizing, bool simpleEvaluation, int? currentSearchDepth = null)
        {
            if (simpleEvaluation) return EvaluateSimple(board, isMaximizing, currentSearchDepth);
            return EvaluateIntelligent(board, isMaximizing, currentSearchDepth);
        }


        private static double EvaluateSimple(IBoard board, bool isMaximizing, int? currentSearchDepth = null)
        {
            Diagnostics.IncrementEvalCount();
            var evalScore = board.PieceList.Sum(p => p.RelativeStrength);

            return evalScore;
        }

        private static double EvaluateIntelligent(IBoard board, bool isMaximizing, int? currentSearchDepth = null)
        {
            Diagnostics.IncrementEvalCount();
            var evalScore = board.PieceList.Sum(p => p.GetEvaluationStrength(board.Strategic.EndGameWeight));

            // Checkmate override
            // Equalize checkmate scores, so relative positions of other pieces don't effect outcome
            // Also give more priority for shallower moves.
            if (Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold)
            {
                if (currentSearchDepth != null)
                {
                    return CheckMateScoreAdjustToDepthFixed(evalScore, currentSearchDepth.Value);
                }
                else
                {
                    return CheckMateScoreAdjustToEven(evalScore);
                }
            }

            // Stalemate
            // TODO not working in endgame properly somehow
            //if (Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold && !isInCheckForOther)
            //{
            //    return 0;
            //    // Otherwise would be evaluated like -200000
            //}


            // TODO pawn structure
            // Separate start game weight functions

            if (board.Strategic.EndGameWeight > 0.50)
            {
                evalScore += EndGameKingToCornerEvaluation(board, isMaximizing);
            }



            return evalScore;
        }

        public static double EndGameKingToCornerEvaluation(IBoard board, bool isWhite)
        {
            var ownPieces = board.PieceList.Where(p => p.IsWhite == isWhite).ToList();
            if (ownPieces.Count == 1)
            {
                return ownPieces.First().GetEvaluationStrength(-1);
            }

            var evaluation = 0.0;
            var opponentKing = board.KingLocation(!isWhite);
            var ownKing = board.KingLocation(isWhite);

            // Testing running
            if (opponentKing == null || ownKing == null) return 0.0;

            // In endgame, favor opponent king to be on edge of board
            double center = 3.5;
            var distanceToCenterRow = Math.Abs(center - opponentKing.CurrentPosition.row);
            var distanceToCenterColumn = Math.Abs(center - opponentKing.CurrentPosition.column);
            evaluation += 1 * (distanceToCenterRow + distanceToCenterColumn);

            // In endgame, favor own king closed to opponent to cut off escape routes
            var rowDifference = Math.Abs(ownKing.CurrentPosition.row - opponentKing.CurrentPosition.row);
            var columnDifference = Math.Abs(ownKing.CurrentPosition.column - opponentKing.CurrentPosition.column);
            var kingDifference = rowDifference + columnDifference;
            evaluation += 14 - kingDifference;

            evaluation += evaluation * 35 * board.Strategic.EndGameWeight;

            if (isWhite) return evaluation;
            else return -evaluation;
        }

        public static double CheckMateScoreAdjustToEven(double evalScore)
        {
            if(Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold)
            {
                if (evalScore > 0)
                {
                    return PieceBaseStrength.King;
                }

                return -PieceBaseStrength.King;
            }

            return evalScore;
        }

        public static double CheckMateScoreAdjustToDepthFixed(double evalScore, int depth)
        {
            if (Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold)
            {
                if (evalScore > 0)
                {
                    return PieceBaseStrength.King + depth;
                }

                return -PieceBaseStrength.King - depth;
            }

            return evalScore;
        }
    }
}
