using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Algorithms
{
    public class MiniMax
    {
        /// <summary>
        /// Main game decision feature. Calculate player and opponent moves to certain depth. When
        /// maximizing, return best move evaluation value for white player. When minimizing return best value for black.
        /// </summary>
        /// <param name="newBoard">Board setup to be evaluated</param>
        /// <param name="depth">How many player and opponent moves by turns are calculated</param>
        /// <param name="alpha">The highest known value at previous recursion level</param>
        /// <param name="beta">The lowest known value at previous recursion level</param>
        /// <param name="maximizingPlayer">Maximizing = white, minimizing = black</param>
        /// <returns></returns>
        public static double ToDepth(Board newBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            if (depth == 0) return newBoard.Evaluate(maximizingPlayer, false, depth);
            var allMoves = newBoard.Moves(maximizingPlayer, false);

            if (!allMoves.Any()) return newBoard.Evaluate(maximizingPlayer, false, depth);
            if (maximizingPlayer)
            {
                var value = -100000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = new Board(newBoard, move);
                    value = Math.Max(value, ToDepth(nextBoard, depth - 1, alpha, beta, false));
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Saved some time by noticing this branch is a dead end
                        Diagnostics.IncrementAlpha();
                        break;
                    }
                }
                return value;
            }
            else
            {
                var value = 100000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = new Board(newBoard, move);
                    value = Math.Min(value, ToDepth(nextBoard, depth - 1, alpha, beta, true));
                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Saved some time by noticing this branch is a dead end
                        Diagnostics.IncrementBeta();
                        break;
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// Minimax with transpositions.
        /// Root1: transposition added at depth 1
        /// Root2: Same transposition encountered at depth 3.
        /// -> No need to use
        ///
        /// Case: transposition added at depth 4
        /// Another tree encounters this at depth 3
        /// -> good
        /// </summary>
        public static double ToDepthWithTranspositions(Board board, int depth, double alpha, double beta, bool maximizingPlayer, bool createStartTranspositionCheck = false)
        {
            if (depth == 0) return board.Evaluate(maximizingPlayer, false, depth);
            if (createStartTranspositionCheck)
            {
                var transposition = board.Shared.Transpositions.GetTranspositionForBoard(board.BoardHash);
                if (transposition != null && transposition.Depth >= depth)
                {
                    return transposition.Evaluation;
                }
            }
            
            var allMoves = board.MovesWithTranspositionOrder(maximizingPlayer, false);
            if (!allMoves.Any()) return board.Evaluate(maximizingPlayer, false, depth);
            
            if (maximizingPlayer)
            {
                var value = -100000.0;
                foreach (var move in allMoves)
                {
                    var transposition = board.Shared.Transpositions.GetTranspositionForMove(board, move);
                    if (transposition != null && transposition.Depth >= depth)
                    {
                        // Saved some time
                        transposition.ReadOnly = true;
                        value = Math.Max(value, transposition.Evaluation);
                        Diagnostics.IncrementTranspositionsFound();
                        
                        if(transposition.Type == NodeType.UpperBound)
                        {
                            // Saved big time
                            break;
                        }
                    }
                    else
                    {
                        var nextBoard = new Board(board, move);
                        var deeperValue = ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, false);
                        
                        // Add new transposition table
                        nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth - 1, deeperValue, NodeType.Exact);
                        value = Math.Max(value, deeperValue);
                    }

                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Move at previous depth is really bad. Break search.
                        // Beta cutoff, alpha surpassed beta
                        // Lower bound, cut-node (exact evaluation might be greater)

                        // Move at previous depth is really bad. Break search.
                        board.Shared.Transpositions.Add(board.BoardHash, depth - 1, value, NodeType.UpperBound, true);
                        Diagnostics.IncrementBeta();
                        break;
                    }
                }
                return value;
            }
            else
            {
                var value = 100000.0;
                foreach (var move in allMoves)
                {
                    var transposition = board.Shared.Transpositions.GetTranspositionForMove(board, move);
                    if (transposition != null && transposition.Depth >= depth)
                    {
                        // Saved some time
                        transposition.ReadOnly = true;
                        value = Math.Min(value, transposition.Evaluation);
                        Diagnostics.IncrementTranspositionsFound();

                        if (transposition.Type == NodeType.LowerBound)
                        {
                            // Saved big time
                            break;
                        }
                    }
                    else
                    {
                        var nextBoard = new Board(board, move);
                        var deeperValue = ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, true);

                        // Add new transposition table
                        nextBoard.Shared.Transpositions.Add(nextBoard.BoardHash, depth - 1, deeperValue, NodeType.Exact);
                        value = Math.Min(value, deeperValue);
                    }

                    beta = Math.Min(beta, value);
                    if (beta <= alpha)
                    {
                        // Move at previous depth is really bad. Break search.
                        // Alpha cutoff, beta went below alpha
                        // Upper bound, all-node (exact evaluation might be less)

                        // Save previous level as cut node
                        board.Shared.Transpositions.Add(board.BoardHash, depth - 1, value, NodeType.UpperBound, true);
                        Diagnostics.IncrementAlpha();
                        break;
                    }
                }
                return value;
            }
        }
    }
}
