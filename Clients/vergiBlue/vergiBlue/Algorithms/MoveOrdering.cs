using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Interface;
using vergiBlue.BoardModel;
using vergiBlue.Pieces;

namespace vergiBlue.Algorithms
{
    class MoveOrdering
    {
        // ------------
        // Order by light score guessing
        public static IList<SingleMove> SortMovesByGuessWeight(IList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateGuessWeightedList(moves, board, isMaximizing);
            var sorted = SortWeightedMovesWithSort(list, isMaximizing);
            return sorted.Select(m => m.move).ToList();
        }

        /// <summary>
        /// Captures: Add piece value substraction.
        /// Promotion: Add piece value
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="board"></param>
        /// <param name="isMaximizing"></param>
        /// <returns></returns>
        private static IList<(double eval, SingleMove move)> CreateGuessWeightedList(IEnumerable<SingleMove> moves,
            IBoard board, bool isMaximizing)
        {
            IList<(double weight, SingleMove move)> list = new List<(double weight, SingleMove move)>();
            foreach (var singleMove in moves)
            {
                var scoreGuess = 0.0;
                if (singleMove.Capture)
                {
                    double relativeStrength;
                    if (singleMove.EnPassant)
                    {
                        // This fails if pieces have been deleted externally (game ended or sandbox)
                        var opponentPawn = board.ValueAtDefinitely(singleMove.EnPassantOpponentPosition);
                        relativeStrength = opponentPawn.RelativeStrength;
                    }
                    else
                    {
                        relativeStrength = board.ValueAtDefinitely(singleMove.NewPos).RelativeStrength;
                    }

                    // Give more value on capturing stronger opponent than player piece.
                    var opponentWeight = Math.Abs(10 * relativeStrength);
                    var ownWeight = Math.Abs(board.ValueAtDefinitely(singleMove.PrevPos).RelativeStrength);

                    if (isMaximizing) scoreGuess += opponentWeight - ownWeight;
                    else scoreGuess -= opponentWeight + ownWeight;
                }

                if (singleMove.Promotion)
                {
                    // TODO positional strength missing
                    var typeScore = singleMove.PromotionType switch
                    {
                        PromotionPieceType.Queen => PieceBaseStrength.Queen,
                        PromotionPieceType.Rook => PieceBaseStrength.Rook,
                        PromotionPieceType.Knight => PieceBaseStrength.Knight,
                        PromotionPieceType.Bishop => PieceBaseStrength.Bishop,
                        _ => 0.0
                    };

                    if (isMaximizing) scoreGuess += typeScore;
                    else scoreGuess -= typeScore;
                }
                
                // Penalize moving to position where opponent pawn is attacking
                // TODO
                
                list.Add((scoreGuess, singleMove));
            }

            return list;
        }

        // -----------
        // Order by evaluation

        public static IList<SingleMove> SortMovesByEvaluation(IList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateEvaluationList(moves, board, isMaximizing);
            var sorted = SortWeightedMovesWithSort(list, isMaximizing);
            return sorted.Select(m => m.move).ToList();
        }

        private static IList<(double eval, SingleMove move)> CreateEvaluationList(IEnumerable<SingleMove> moves,
            IBoard board, bool isMaximizing)
        {
            IList<(double eval, SingleMove move)> list = new List<(double eval, SingleMove move)>();
            foreach (var singleMove in moves)
            {
                var newBoard = BoardFactory.CreateFromMove(board, singleMove);
                var eval = newBoard.Evaluate(isMaximizing, false);
                list.Add((eval, singleMove));
            }

            return list;
        }

        /// <summary>
        /// Use C# Sort. Bit quicker than <see cref="SortWeightedMovesWithOrderBy"/>
        /// Weight can be any abstract measure of evaluation. Positivive is better for maximizing.
        /// </summary>
        /// <param name="evaluationList"></param>
        /// <param name="isMaximizing"></param>
        /// <returns></returns>
        public static IList<(double weight, SingleMove move)> SortWeightedMovesWithSort(IEnumerable<(double weight, SingleMove move)> evaluationList,
            bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var sorted = evaluationList.ToList();
            sorted.Sort(Compare);

            int Compare((double weight, SingleMove move) move1, (double weight, SingleMove move) move2)
            {
                if (Math.Abs(move1.weight - move2.weight) < 1e-6)
                {
                    return 0;
                }

                if (!isMaximizing)
                {
                    if (move1.weight > move2.weight) return 1;
                    return -1;
                }

                if (move1.weight < move2.weight) return 1;
                return -1;
            }

            // 
            return sorted;
        }
        
        /// <summary>
        /// Uses OrderBy.
        /// Weight can be any abstract measure of evaluation. Positivive is better for maximizing.
        /// </summary>
        /// <param name="evaluationList"></param>
        /// <param name="isMaximizing"></param>
        /// <returns></returns>
        public static IList<(double weight, SingleMove move)> SortWeightedMovesWithOrderBy(
            IEnumerable<(double weight, SingleMove move)> evaluationList, bool isMaximizing)
        {
            var sorted = evaluationList.ToList();
            if (isMaximizing)
            {
                sorted = sorted.OrderByDescending(item => item.weight).ToList();
            }
            else
            {
                sorted = sorted.OrderBy(item => item.weight).ToList();
            }

            return sorted;
        }
    }
}
