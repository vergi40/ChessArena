using System;
using System.Diagnostics;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems;
using vergiBlue.BoardModel.Subsystems.TranspositionTables;

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
        private const int MOVELIST_MAX_SIZE = 128;

        private static double TimerExceededValue(double alpha, double beta, bool maximizingPlayer)
        {
            // Create immediate cutoff
            if (maximizingPlayer) return alpha - 1;
            return beta + 1;
        }

        /// <summary>
        /// Main game decision feature. Calculate player and opponent moves to certain depth. When
        /// maximizing, return best move evaluation value for white player. When minimizing return best value for black.
        ///
        /// Fail-soft.
        /// If min assured for maximizing player goes over higher depth beta -> cutoff. This branch would never be played.
        ///
        /// beta (max assured for minimizing player) e.g. +30
        ///  |
        ///  |  best move - both maximizing and minimizing agrees
        ///  |
        /// alpha (min assured for maximizing player) e.g. -30
        ///
        /// </summary>
        /// <param name="newBoard">Board setup to be evaluated</param>
        /// <param name="depth">How many player and opponent moves by turns are calculated</param>
        /// <param name="alpha">The highest known value at previous recursion level</param>
        /// <param name="beta">The lowest known value at previous recursion level</param>
        /// <param name="maximizingPlayer">Maximizing = white, minimizing = black</param>
        /// <returns></returns>
        public static double ToDepth(IBoard newBoard, int depth, double alpha, double beta, bool maximizingPlayer, ISearchTimer timer)
        {
            if (depth == 0)
            {
                return newBoard.Evaluate(maximizingPlayer, false, depth);
            }
            if (timer.Exceeded())
            {
                Collector.IncreaseOperationCount("TimerExceeded");
                return TimerExceededValue(alpha, beta, maximizingPlayer);
            }

            // Allocating move memory to stack instead of heap
            // https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code
            // https://tearth.dev/posts/performance-of-chess-engines-written-in-csharp-part-1/
            Span<MoveStruct> allMoves = stackalloc MoveStruct[MOVELIST_MAX_SIZE];
            newBoard.MoveGenerator.MovesWithOrderingSpan(maximizingPlayer, false, allMoves, out var length);

            // Checkmate or stalemate
            if (length == 0)
            {
                return newBoard.EvaluateNoMoves(maximizingPlayer, false, depth);
            }
            if (maximizingPlayer)
            {
                var value = MiniMaxGeneral.DefaultAlpha;
                for (int i = 0; i < length; i++)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, allMoves[i]);
                    value = Math.Max(value, ToDepth(nextBoard, depth - 1, alpha, beta, false, timer));
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Prune. Alpha is better than previous level beta. Don't want to use moves from this board set.
                        // Saved some time by noticing this branch is a dead end
                        Collector.IncreaseOperationCount(OperationsKeys.Alpha);
                        break;
                    }
                }
                return value;
            }
            else
            {
                var value = MiniMaxGeneral.DefaultBeta;
                for (int i = 0; i < length; i++)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, allMoves[i]);
                    value = Math.Min(value, ToDepth(nextBoard, depth - 1, alpha, beta, true, timer));
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Prune. Beta is smaller than previous level alpha. Don't want to use moves from this board set.
                        Collector.IncreaseOperationCount(OperationsKeys.Beta);
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
        public static double ToDepthWithTranspositions(IBoard newBoard, int depth, double alpha, double beta, bool maximizingPlayer, ISearchTimer timer)
        {
            if (depth == 0)
            {
                return newBoard.Evaluate(maximizingPlayer, false, depth);
            }
            if (timer.Exceeded())
            {
                Collector.IncreaseOperationCount("TimerExceeded");
                return TimerExceededValue(alpha, beta, maximizingPlayer);
            }

            // Check if solution already exists
            var transposition = newBoard.Shared.Transpositions.GetTranspositionForBoard(newBoard.BoardHash);
            if (transposition != null && transposition.Depth >= depth)
            {
                Collector.IncreaseOperationCount(OperationsKeys.TranspositionUsed);
                var transpositionEval = Evaluator.CheckMateScoreAdjustToDepthFixed(transposition.Evaluation, depth);
                
                if (transposition.Type == NodeType.Exact) return transpositionEval;
                else if (transposition.Type == NodeType.UpperBound && transpositionEval < beta)
                {
                    beta = transpositionEval;
                }
                else if (transposition.Type == NodeType.LowerBound && transpositionEval > alpha)
                {
                    alpha = transpositionEval;
                }

                // Early cutoff, nice
                if (alpha >= beta)
                {
                    Collector.IncreaseOperationCount("TTcutoff");
                    return transpositionEval;
                }
            }

            // Allocating move memory to stack instead of heap
            // https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code
            // https://tearth.dev/posts/performance-of-chess-engines-written-in-csharp-part-1/
            Span<MoveStruct> allMoves = stackalloc MoveStruct[MOVELIST_MAX_SIZE];
            newBoard.MoveGenerator.MovesWithOrderingSpan(maximizingPlayer, false, allMoves, out var length);

            if (length == 0)
            {
                // Checkmate or stalemate
                return newBoard.EvaluateNoMoves(maximizingPlayer, false, depth);
            }
            
            if (maximizingPlayer)
            {
                var value = MiniMaxGeneral.DefaultAlpha;
                var bestMoveIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, allMoves[i]);
                    var searchResult = ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, false, timer);
                    if (searchResult > value)
                    {
                        value = searchResult;
                        bestMoveIndex = i;
                    }
                    
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Eval is at least beta. Fail high
                        // Prune. Alpha is better than previous level beta. Don't want to use moves from this board set.

                        if(depth > 1 && !timer.Exceeded())
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value,
                                NodeType.LowerBound, nextBoard.Shared.GameTurnCount, allMoves[i]);
                        }
                        Collector.IncreaseOperationCount(OperationsKeys.Beta);
                        break;
                    }
                }

                // Save best move
                if(value >= alpha && value <= beta && depth > 1 && !timer.Exceeded())
                {
                    Debug.Assert(bestMoveIndex >= 0, "Logical error. No best move found");
                    newBoard.Shared.Transpositions.Add(newBoard.BoardHash, depth, value, NodeType.Exact, newBoard.Shared.GameTurnCount, allMoves[bestMoveIndex]);
                }
                return value;
            }
            else
            {
                var value = MiniMaxGeneral.DefaultBeta;
                var bestMoveIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, allMoves[i]);
                    var searchResult = ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, true, timer);
                    if (searchResult < value)
                    {
                        value = searchResult;
                        bestMoveIndex = i;
                    }

                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Eval is at most alpha. Fail low
                        // Prune. Beta is smaller than previous level alpha. Don't want to use moves from this board set.
                        
                        if (depth > 1 && !timer.Exceeded())
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value,
                                NodeType.UpperBound, nextBoard.Shared.GameTurnCount, allMoves[i]);
                        }
                        Collector.IncreaseOperationCount(OperationsKeys.Alpha);
                        break;
                    }
                }
                // Save best move
                if (value >= alpha && value <= beta && depth > 1 && !timer.Exceeded())
                {
                    Debug.Assert(bestMoveIndex >= 0, "Logical error. No best move found");
                    newBoard.Shared.Transpositions.Add(newBoard.BoardHash, depth, value, NodeType.Exact, newBoard.Shared.GameTurnCount, allMoves[bestMoveIndex]);
                }
                return value;
            }
        }

        public static double ToDepthUciPrototype(IBoard newBoard, int depth, double alpha, double beta, bool maximizingPlayer, ISearchStopControl stopControl)
        {
            if (depth == 0)
            {
                return newBoard.Evaluate(maximizingPlayer, false, depth);
            }
            if (stopControl.StopSearch())
            {
                Collector.IncreaseOperationCount($"{stopControl.Reason}");
                return TimerExceededValue(alpha, beta, maximizingPlayer);
            }

            // Check if solution already exists
            var transposition = newBoard.Shared.Transpositions.GetTranspositionForBoard(newBoard.BoardHash);
            if (transposition != null && transposition.Depth >= depth)
            {
                Collector.IncreaseOperationCount(OperationsKeys.TranspositionUsed);
                var transpositionEval = Evaluator.CheckMateScoreAdjustToDepthFixed(transposition.Evaluation, depth);

                if (transposition.Type == NodeType.Exact) return transpositionEval;
                else if (transposition.Type == NodeType.UpperBound && transpositionEval < beta)
                {
                    beta = transpositionEval;
                }
                else if (transposition.Type == NodeType.LowerBound && transpositionEval > alpha)
                {
                    alpha = transpositionEval;
                }

                // Early cutoff, nice
                if (alpha >= beta)
                {
                    Collector.IncreaseOperationCount("TTcutoff");
                    return transpositionEval;
                }
            }

            // Allocating move memory to stack instead of heap
            // https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code
            // https://tearth.dev/posts/performance-of-chess-engines-written-in-csharp-part-1/
            Span<MoveStruct> allMoves = stackalloc MoveStruct[MOVELIST_MAX_SIZE];
            newBoard.MoveGenerator.MovesWithOrderingSpan(maximizingPlayer, false, allMoves, out var length);

            if (length == 0)
            {
                // Checkmate or stalemate
                return newBoard.EvaluateNoMoves(maximizingPlayer, false, depth);
            }

            if (maximizingPlayer)
            {
                var value = MiniMaxGeneral.DefaultAlpha;
                var bestMoveIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, allMoves[i]);
                    var searchResult = ToDepthUciPrototype(nextBoard, depth - 1, alpha, beta, false, stopControl);
                    if (searchResult > value)
                    {
                        value = searchResult;
                        bestMoveIndex = i;
                    }

                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Eval is at least beta. Fail high
                        // Prune. Alpha is better than previous level beta. Don't want to use moves from this board set.

                        if (depth > 1 && !stopControl.StopSearch())
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value,
                                NodeType.LowerBound, nextBoard.Shared.GameTurnCount, allMoves[i]);
                        }
                        Collector.IncreaseOperationCount(OperationsKeys.Beta);
                        break;
                    }
                }

                // Save best move
                if (value >= alpha && value <= beta && depth > 1 && !stopControl.StopSearch())
                {
                    Debug.Assert(bestMoveIndex >= 0, "Logical error. No best move found");
                    newBoard.Shared.Transpositions.Add(newBoard.BoardHash, depth, value, NodeType.Exact, newBoard.Shared.GameTurnCount, allMoves[bestMoveIndex]);
                }
                return value;
            }
            else
            {
                var value = MiniMaxGeneral.DefaultBeta;
                var bestMoveIndex = -1;
                for (int i = 0; i < length; i++)
                {
                    var nextBoard = BoardFactory.CreateFromMove(newBoard, allMoves[i]);
                    var searchResult = ToDepthUciPrototype(nextBoard, depth - 1, alpha, beta, true, stopControl);
                    if (searchResult < value)
                    {
                        value = searchResult;
                        bestMoveIndex = i;
                    }

                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Eval is at most alpha. Fail low
                        // Prune. Beta is smaller than previous level alpha. Don't want to use moves from this board set.

                        if (depth > 1 && !stopControl.StopSearch())
                        {
                            nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth, value,
                                NodeType.UpperBound, nextBoard.Shared.GameTurnCount, allMoves[i]);
                        }
                        Collector.IncreaseOperationCount(OperationsKeys.Alpha);
                        break;
                    }
                }
                // Save best move
                if (value >= alpha && value <= beta && depth > 1 && !stopControl.StopSearch())
                {
                    Debug.Assert(bestMoveIndex >= 0, "Logical error. No best move found");
                    newBoard.Shared.Transpositions.Add(newBoard.BoardHash, depth, value, NodeType.Exact, newBoard.Shared.GameTurnCount, allMoves[bestMoveIndex]);
                }
                return value;
            }
        }
    }
}
