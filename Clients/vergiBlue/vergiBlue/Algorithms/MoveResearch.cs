using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Algorithms
{
    public class MoveResearch
    {
        public static EvaluationResult GetMoveScoreList(IList<SingleMove> moves,
            int searchDepth, Board board, bool isMaximizing, bool useTranspositions)
        {
            var result = new EvaluationResult();
            var alpha = -100000.0;
            var beta = 100000.0;

            if (useTranspositions)
            {
                foreach (var move in moves)
                {
                    var transposition = board.Shared.Transpositions.GetTranspositionForMove(board, move);
                    if (transposition != null && transposition.Depth >= searchDepth)
                    {
                        // Saved some time
                        // TODO extra parameters to evaluationresult if this was lower or upper bound
                        transposition.ReadOnly = true;
                        result.Add(transposition.Evaluation, move);
                    }
                    else
                    {
                        // Board evaluation at current depth
                        var newBoard = new Board(board, move);
                        var value = MiniMax.ToDepthWithTranspositions(newBoard, searchDepth, alpha, beta,
                            !isMaximizing, true);
                        result.Add(value, move);

                        // Add new transposition table
                        newBoard.Shared.Transpositions.Add(newBoard.BoardHash, searchDepth, value, NodeType.Exact, true);

                        if (isMaximizing)
                        {
                            alpha = Math.Max(alpha, value);
                        }
                        else
                        {
                            beta = Math.Min(beta, value);
                        }
                    }
                }
            }
            else
            {
                foreach (var move in moves)
                {
                    // Board evaluation at current depth
                    var newBoard = new Board(board, move);
                    var value = MiniMax.ToDepth(newBoard, searchDepth, alpha, beta, !isMaximizing);
                    result.Add(value, move);

                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, value);
                    }
                    else
                    {
                        beta = Math.Min(beta, value);
                    }
                }
            }
            

            return result;
        }


        public static EvaluationResult GetMoveScoreListParallel(IList<SingleMove> moves, int searchDepth, Board board, bool isMaximizing)
        {
            var result = new EvaluationResult();
            var evaluated = new List<(double, SingleMove)>();
            var syncObject = new object();

            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
            Parallel.ForEach(moves,
                () => new List<(double, SingleMove)>(), // Local initialization. Need to inform compiler the type by initializing
                (move, loopState, localState) => // Predefined lambda expression (Func<SingleMove, ParallelLoopState, thread-local variable, body>)
                {
                    var newBoard = new Board(board, move);
                    var value = MiniMax.ToDepth(newBoard, searchDepth, -100000, 100000, !isMaximizing);
                    localState.Add((value, move));
                    return localState;
                },
                (finalResult) =>
                {
                    lock (syncObject) evaluated.AddRange(finalResult);
                });
            
            result.Add(evaluated);
            return result;
        }

        public static SingleMove SelectBestMove(EvaluationResult evaluated, bool isMaximizing, bool prioritizeCaptures)
        {
            if (isMaximizing) return evaluated.MaxMove;
            else return evaluated.MinMove;
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

        /// <summary>
        /// Returns checkmate move or null.
        /// </summary>
        /// <returns></returns>
        public static SingleMove? ImmediateCheckMateAvailable(IList<SingleMove> moves, Board board, bool isMaximizing)
        {
            // Brute search checkmate
            foreach (var singleMove in moves)
            {
                var newBoard = new Board(board, singleMove);
                if (newBoard.IsCheckMate(isMaximizing, false))
                {
                    singleMove.CheckMate = true;
                    return singleMove;
                }
            }

            return null;
        }

        public static IList<SingleMove> CheckMateInTwoTurns(IList<SingleMove> moves, Board board, bool isMaximizing)
        {
            IList<SingleMove> checkMates = new List<SingleMove>();
            foreach (var singleMove in moves)
            {
                var newBoard = new Board(board, singleMove);
                if (CheckMate.InTwoTurns(newBoard, isMaximizing))
                {
                    // TODO collect all choices and choose best
                    // Game goes to draw loop otherwise
                    checkMates.Add(singleMove);
                }
            }

            return checkMates;
        }
        
        /// <summary>
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ...
        /// 
        /// </summary>
        public static SingleMove SelectBestWithIterativeDeepening(IList<SingleMove> allMoves, int searchDepth, Board board, bool isMaximizing, bool useTranspositions)
        {
            if (useTranspositions)
            {
                return IterativeDeepeningWithTranspositions(allMoves, searchDepth, board, isMaximizing);
            }
            else
            {
                return IterativeDeepeningBasic(allMoves, searchDepth, board, isMaximizing);
            }
        }

        /// <summary>
        /// Iterative deepening sub-method.
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ... 
        /// </summary>
        private static SingleMove IterativeDeepeningBasic(IList<SingleMove> allMoves, int searchDepth, Board board, bool isMaximizing, int timeLimitInMs = 5000)
        {
            var timeUp = false;
            int depthUsed = 0;
            
            var midResult = new List<(double, SingleMove)>();
            var currentIterationMoves = new List<SingleMove>(allMoves);
            var watch = new Stopwatch();
            watch.Start();

            // Initial depth 2
            for (int i = 2; i <= searchDepth; i++)
            {
                depthUsed = i;
                midResult.Clear();

                // Initialize for each cycle
                var alpha = -100000.0;
                var beta = 100000.0;

                foreach (var move in currentIterationMoves)
                {
                    var newBoard = new Board(board, move);
                    var evaluation = MiniMax.ToDepth(newBoard, i, alpha, beta, !isMaximizing);
                    midResult.Add((evaluation, move));

                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, evaluation);
                    }
                    else
                    {
                        beta = Math.Min(beta, evaluation);
                    }

                    if (watch.ElapsedMilliseconds > timeLimitInMs)
                    {
                        timeUp = true;
                        break;
                    }
                }

                if (timeUp) break;
                
                // Full search finished for depth
                midResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
                currentIterationMoves = midResult.Select(item => item.Item2).ToList();
            }

            // midResult is either partial or full. Just sort and return first.
            // Awkward results might be produced if had only tie to calculate 1 result in depth...
            var finalResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
            Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed} ({midResult.Count}/{allMoves.Count}).");
            Diagnostics.AddMessage($" Move evaluation: {finalResult.First().weight}.");
            return finalResult.First().move;
        }

        /// <summary>
        /// Iterative deepening sub-method.
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ...
        /// 
        /// </summary>
        private static SingleMove IterativeDeepeningWithTranspositions(IList<SingleMove> allMoves, int searchDepth, Board board, bool isMaximizing)
        {
            var midResult = new List<(double, SingleMove)>();
            var previousOrder = new List<SingleMove>(allMoves);

            // Initial depth 2
            for (int i = 2; i <= searchDepth; i++)
            {
                midResult.Clear();
                
                // Initialize for each cycle
                var alpha = -100000.0;
                var beta = 100000.0;
                
                foreach (var move in allMoves)
                {
                    var newBoard = new Board(board, move);
                    var evaluation = MiniMax.ToDepthWithTranspositions(newBoard, i, alpha, beta, !isMaximizing, true);
                    
                    // Top-level result should always be saved with priority
                    newBoard.Shared.Transpositions.Add(newBoard.BoardHash, i, evaluation, NodeType.Exact, true);
                    midResult.Add((evaluation, move));

                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, evaluation);
                    }
                    else
                    {
                        beta = Math.Min(beta, evaluation);
                    }
                }

                midResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
                allMoves = midResult.Select(item => item.Item2).ToList();

                if (allMoves.Any())
                {
                    // Save previous level in case of time running out or empty result
                    previousOrder = new List<SingleMove>(allMoves);
                }
                else
                {
                    // TODO delete first and try search with second
                    return previousOrder.First();
                }
            }

            // Search finished
            return allMoves.First();
        }
    }
}
