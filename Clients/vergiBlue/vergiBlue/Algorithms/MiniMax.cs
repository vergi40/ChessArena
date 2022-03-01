using System;
using System.Linq;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;

namespace vergiBlue.Algorithms
{
    /// <summary>
    /// General minimax alpha beta abstract:
    ///
    /// When maximizing: 
    /// * We know previous level current best minimizing move (beta).
    /// * We update current best max move to alpha (Max(alpha, value))
    /// * If alpha >= beta, we know that previous level minimizing will skip this.
    /// * Prune
    ///
    /// When minimizing:
    /// * We know previous level current best maximizing move (alpha)
    /// * We update current best min move to beta (Min(beta, value))
    /// * If beta <= alpha, we know that previous level maximizing will skip this.
    /// * Prune
    /// </summary>
    public static class MiniMax
    {
        /// <summary>
        /// Main game decision feature. Calculate player and opponent moves to certain depth. When
        /// maximizing, return best move evaluation value for white player. When minimizing return best value for black.
        ///
        /// </summary>
        /// <param name="newBoard">Board setup to be evaluated</param>
        /// <param name="depth">How many player and opponent moves by turns are calculated</param>
        /// <param name="alpha">The highest known value at previous recursion level</param>
        /// <param name="beta">The lowest known value at previous recursion level</param>
        /// <param name="maximizingPlayer">Maximizing = white, minimizing = black</param>
        /// <returns></returns>
        public static double ToDepth(IBoard newBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            if (depth == 0) return newBoard.Evaluate(maximizingPlayer, false, depth);
            var allMoves = newBoard.MoveGenerator.MovesWithOrdering(maximizingPlayer, false);

            // Checkmate or stalemate
            if (!allMoves.Any()) return newBoard.EvaluateNoMoves(maximizingPlayer, false, depth);
            if (maximizingPlayer)
            {
                var value = -1000000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, move);
                    value = Math.Max(value, ToDepth(nextBoard, depth - 1, alpha, beta, false));
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Prune. Alpha is better than previous level beta. Don't want to use moves from this board set.
                        // Saved some time by noticing this branch is a dead end
                        Diagnostics.IncrementAlpha();
                        break;
                    }
                }
                return value;
            }
            else
            {
                var value = 1000000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, move);
                    value = Math.Min(value, ToDepth(nextBoard, depth - 1, alpha, beta, true));
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Prune. Beta is smaller than previous level alpha. Don't want to use moves from this board set.
                        Diagnostics.IncrementBeta();
                        break;
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// Minimax with transpositions.
        ///
        /// Example - search depth 5
        /// 5    4   3   2   1
        /// A1 - B - E - F - G: Transposition added for each move
        /// A2 - C - E: Same transposition encountered at depth 3.
        /// -> No need to search deeper
        ///
        /// General guidelines
        /// * Don't save transposition values near the leaf nodes (depth 0, maybe 1)
        ///
        /// Example
        /// maximizing depth 4    alpha = 5, beta = 10
        ///
        /// minimizing depth 3    alpha = 5, beta = 10
        /// minimizing depth 3    value = 12 ok.
        /// minimizing depth 3    value = 8 ok. beta = 8
        /// minimizing depth 3    value = 4 -> prune and save as lowerbound. this move results to 4 or lower. return 4
        ///
        /// maximizing depth 2    alpha = 5, beta = 8
        /// maximizing depth 2    value = 13 -> prune and save as upperbound. this move results to 13 or higher. return 13
        /// maximizing depth 2    value = 6 ok. alpha = 6
        /// 
        /// minimizing depth 1    alpha = 6 beta = 8
        /// transposition found: lowerbound, 4
        /// transposition found: upperbound, 13
        ///
        ///
        /// maximizing depth 4 receives 4 - ignored
        /// minimizing depth 3 receives 13 - ignored
        /// </summary>
        public static double ToDepthWithTranspositions(IBoard newBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            if (depth == 0) return newBoard.Evaluate(maximizingPlayer, false, depth);
            
            // Check if solution already exists
            var transposition = newBoard.Shared.Transpositions.GetTranspositionForBoard(newBoard.BoardHash);
            if (transposition != null && transposition.Depth >= depth)
            {
                Diagnostics.IncrementTranspositionsFound();
                var transpositionEval = Evaluator.CheckMateScoreAdjustToDepthFixed(transposition.Evaluation, depth);
                
                if (transposition.Type == NodeType.Exact) return transpositionEval;
                if (transposition.Type == NodeType.UpperBound && transpositionEval < beta)
                {
                    beta = transpositionEval;
                }
                else if (transposition.Type == NodeType.LowerBound && transpositionEval > alpha)
                {
                    alpha = transpositionEval;
                }
                //else if (transposition.Type == NodeType.UpperBound && !maximizingPlayer && transposition.Evaluation <= alpha)
                //{
                //    // No need to search further, as score was worse than alpha
                //    return transposition.Evaluation;
                //}
                //else if (transposition.Type == NodeType.LowerBound && maximizingPlayer && transposition.Evaluation >= beta)
                //{
                //    // No need to search further, as score was worse than beta
                //    return transposition.Evaluation;
                //}
            }

            var allMoves = newBoard.MoveGenerator.MovesWithOrdering(maximizingPlayer, false);
            if (!allMoves.Any())
            {
                // Checkmate or stalemate
                return newBoard.EvaluateNoMoves(maximizingPlayer, false, depth);
            }
            
            if (maximizingPlayer)
            {
                var value = MiniMaxGeneral.DefaultAlpha;
                foreach (var move in allMoves)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, move);
                    value = ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, false);
                    if (value >= beta)
                    {
                        // Eval is at least beta. Fail high
                        // Prune. Alpha is better than previous level beta. Don't want to use moves from this board set.

                        if(depth > 1)
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value,
                                NodeType.LowerBound, nextBoard.Shared.GameTurnCount);
                        }
                        Diagnostics.IncrementBeta();
                        break;
                    }

                    if(value > alpha)
                    {
                        // Value between alpha and beta. Save as exact score
                        // Update alpha for rest of iteration
                        alpha = value;
                        if(depth > 1)
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value, NodeType.Exact, nextBoard.Shared.GameTurnCount);
                        }
                    }
                }
                return value;
            }
            else
            {
                var value = MiniMaxGeneral.DefaultBeta;
                foreach (var move in allMoves)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, move);
                    value = ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, true);
                    if (value <= alpha)
                    {
                        // Eval is at most alpha. Fail low
                        // Prune. Beta is smaller than previous level alpha. Don't want to use moves from this board set.
                        
                        if (depth > 1)
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value,
                                NodeType.UpperBound, nextBoard.Shared.GameTurnCount);
                        }
                        Diagnostics.IncrementAlpha();
                        break;
                    }

                    if(value < beta)
                    {
                        // Value between alpha and beta. Save as exact score
                        // Update beta for rest of iteration
                        beta = value;
                        if (depth > 1)
                        {
                            
                            // Add new transposition table
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value, NodeType.Exact, nextBoard.Shared.GameTurnCount);
                        }
                    }
                }
                return value;
            }
        }
    }
}
