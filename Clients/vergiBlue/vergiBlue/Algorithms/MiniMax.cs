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
            if (depth == 0) return newBoard.Evaluate(maximizingPlayer, depth);
            var allMoves = newBoard.Moves(maximizingPlayer, false);

            if (!allMoves.Any()) return newBoard.Evaluate(maximizingPlayer, depth);
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
                        //Diagnostics.IncrementAlpha();
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
                    if (beta < alpha)
                    {
                        // Saved some time by noticing this branch is a dead end
                        //Diagnostics.IncrementBeta();
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
        public static double ToDepthWithTranspositions(Board newBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            if (newBoard.SharedData.Transpositions.Tables.ContainsKey(newBoard.BoardHash))
            {
                // Transposition found
                var transposition = newBoard.SharedData.Transpositions.Tables[newBoard.BoardHash];
                if (transposition.Depth >= depth)
                {
                    // Useful
                    newBoard.SharedData.Transpositions.Update(transposition.Hash, transposition.Depth, 
                        transposition.Evaluation, transposition.Type, true);
                    return transposition.Evaluation;
                }
            }
            if (depth == 0) return newBoard.Evaluate(maximizingPlayer, depth);
            
            var allMoves = newBoard.Moves(maximizingPlayer, false);
            if (!allMoves.Any()) return newBoard.Evaluate(maximizingPlayer, depth);
            
            if (maximizingPlayer)
            {
                var value = -100000.0;
                foreach (var move in allMoves)
                {
                    var nextBoard = new Board(newBoard, move);
                    value = Math.Max(value, ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, false));
                    
                    // Add to transpositions
                    nextBoard.SharedData.Transpositions.Add(nextBoard.BoardHash, depth - 1, value, NodeType.Exact);
                    
                    alpha = Math.Max(alpha, value);
                    if (alpha >= beta)
                    {
                        // Beta cutoff
                        // Lower bound, cut-node (exact evaluation might be greater)
                        
                        nextBoard.SharedData.Transpositions.Update(nextBoard.BoardHash, depth - 1, value, NodeType.LowerBound, true);
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
                    value = Math.Min(value, ToDepthWithTranspositions(nextBoard, depth - 1, alpha, beta, true));

                    // Add to transpositions
                    nextBoard.SharedData.Transpositions.Add(nextBoard.BoardHash, depth - 1, value, NodeType.Exact);
                    
                    beta = Math.Min(beta, value);
                    if (beta < alpha)
                    {
                        // Alpha cutoff
                        // Upper bound, all-node (exact evaluation might be less)

                        nextBoard.SharedData.Transpositions.Update(nextBoard.BoardHash, depth - 1, value, NodeType.UpperBound, true);
                        break;
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// Call MiniMax separately on each depth. If checkmate is found, break;
        /// </summary>
        /// <param name="newBoard"></param>
        /// <param name="depth"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="maximizingPlayer"></param>
        /// <returns></returns>
        public static double IterateToDepth(Board newBoard, int depth, double alpha, double beta, bool maximizingPlayer)
        {
            const double minIterations = 2;//TODO need tweaking
            const double maxIterations = 3;//TODO need tweaking
            var bestValue = WorstValue(maximizingPlayer);
            for (int i = 1; i <= Math.Min(maxIterations, Math.Max(minIterations, depth)); i++)
            {
                var value = ToDepth(newBoard, i, alpha, beta, maximizingPlayer);
                if (maximizingPlayer)
                {
                    // Checkmate
                    //if (value > StrengthTable.King / 2) return value;
                    if (value > bestValue)
                    {
                        bestValue = value;
                    }
                }
                else
                {
                    // Checkmate
                    //if (value < -StrengthTable.King / 2) return value;
                    if (value < bestValue)
                    {
                        bestValue = value;
                    }
                }
            }

            return bestValue;
        }

        private static double BestValue(bool isMaximizing)
        {
            if (isMaximizing) return 1000000;
            else return -1000000;
        }

        private static double WorstValue(bool isMaximizing)
        {
            if (isMaximizing) return -1000000;
            else return 1000000;
        }

    }
}
