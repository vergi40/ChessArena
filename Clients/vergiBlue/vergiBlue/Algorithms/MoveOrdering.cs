using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Interface;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.Pieces;

namespace vergiBlue.Algorithms
{
    /// <summary>
    /// Improvement ideas:
    /// * Span support and other optimization
    /// * Advanced ordering with TT
    /// </summary>
    public static class MoveOrdering
    {
        // ------------
        // Order by light score guessing
        public static List<SingleMove> SortMovesByGuessWeight(IReadOnlyList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateGuessWeightedList(moves, board, isMaximizing);
            var sorted = SortWeightedMovesWithSort(list, isMaximizing);
            return sorted.Select(m => m.move).ToList();
        }

        /// <summary>
        /// Return weight results for testing
        /// </summary>
        public static List<(double weight, SingleMove move)> DebugSortMovesByGuessWeight(IReadOnlyList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateGuessWeightedList(moves, board, isMaximizing);
            var sorted = SortWeightedMovesWithSort(list, isMaximizing);
            return sorted;
        }

        /// <summary>
        /// Captures: Add piece value substraction.
        /// Promotion: Add piece value
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="board"></param>
        /// <param name="isMaximizing"></param>
        /// <returns></returns>
        private static List<(double eval, SingleMove move)> CreateGuessWeightedList(IReadOnlyList<SingleMove> moves,
            IBoard board, bool isMaximizing)
        {
            var list = new List<(double weight, SingleMove move)>();
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

                if (singleMove.Castling)
                {
                    var bonus = EvalConstants.CASTLING_BONUS;
                    if (!isMaximizing) bonus *= -1;
                    scoreGuess += bonus;
                }
                
                // Penalize moving to position where opponent pawn is attacking
                // TODO

                // Add some extra for check
                
                list.Add((scoreGuess, singleMove));
            }

            return list;
        }

        // -----------
        // Order by evaluation

        public static List<SingleMove> SortMovesByEvaluation(IReadOnlyList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateEvaluationList(moves, board, isMaximizing);
            var sorted = SortWeightedMovesWithSort(list, isMaximizing);
            return sorted.Select(m => m.move).ToList();
        }

        public static List<(double weight, SingleMove move)> DebugSortMovesByEvaluation(IReadOnlyList<SingleMove> moves, IBoard board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateEvaluationList(moves, board, isMaximizing);
            var sorted = SortWeightedMovesWithSort(list, isMaximizing);
            return sorted;
        }

        private static List<(double eval, SingleMove move)> CreateEvaluationList(IReadOnlyList<SingleMove> moves,
            IBoard board, bool isMaximizing)
        {
            var list = new List<(double eval, SingleMove move)>();
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
        public static List<(double weight, SingleMove move)> SortWeightedMovesWithSort(IReadOnlyList<(double weight, SingleMove move)> evaluationList,
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

        public static List<(double weight, SingleMove move)> SortWeightedMovesWithTTSort(
            IReadOnlyList<(double weight, SingleMove move)> evaluationList,
            bool isMaximizing)
        {
            // TODO move ordering with help of transposition tables entries
            return SortWeightedMovesWithSort(evaluationList, isMaximizing);
        }

        /// <summary>
        /// Uses OrderBy.
        /// Weight can be any abstract measure of evaluation. Positivive is better for maximizing.
        /// </summary>
        /// <param name="evaluationList"></param>
        /// <param name="isMaximizing"></param>
        /// <returns></returns>
        public static List<(double weight, SingleMove move)> SortWeightedMovesWithOrderBy(
            IReadOnlyList<(double weight, SingleMove move)> evaluationList, bool isMaximizing)
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
