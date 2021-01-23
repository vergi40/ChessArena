using System;
using System.Collections.Generic;
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

            if (useTranspositions)
            {
                foreach (var move in moves)
                {
                    var transposition = board.SharedData.Transpositions.GetTranspositionForMove(board, move);
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
                        var value = MiniMax.ToDepthWithTranspositions(newBoard, searchDepth, -100000, 100000,
                            !isMaximizing, true);
                        result.Add(value, move);

                        // Add new transposition table
                        newBoard.SharedData.Transpositions.Add(newBoard.BoardHash, searchDepth, value, NodeType.Exact, true);
                    }
                }
            }
            else
            {
                foreach (var move in moves)
                {
                    // Board evaluation at current depth
                    var newBoard = new Board(board, move);
                    var value = MiniMax.ToDepth(newBoard, searchDepth, -100000, 100000, !isMaximizing);
                    result.Add(value, move);
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

        private static IList<(double eval, SingleMove move)> CreateEvaluationList(IEnumerable<SingleMove> moves,
            Board board, bool isMaximizing)
        {
            IList<(double eval, SingleMove move)> list = new List<(double eval, SingleMove move)>();
            foreach (var singleMove in moves)
            {
                var newBoard = new Board(board, singleMove);
                var eval = newBoard.Evaluate(isMaximizing);
                list.Add((eval, singleMove));
            }

            return list;
        }

        public static IList<SingleMove> OrderMovesByEvaluation(IList<SingleMove> moves, Board board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateEvaluationList(moves, board, isMaximizing);
            var sorted = SortEvaluatedMoves(list, isMaximizing, true);
            return sorted.Select(m => m.move).ToList();
        }

        /// <summary>
        /// Use C# Sort. Bit quicker than <see cref="OrderEvaluatedMoves"/>
        /// </summary>
        /// <param name="evaluationList"></param>
        /// <param name="isMaximizing"></param>
        /// <param name="prioritizeCaptures"></param>
        /// <returns></returns>
        private static IList<(double eval, SingleMove move)> SortEvaluatedMoves(IEnumerable<(double evaluationScore, SingleMove move)> evaluationList, 
            bool isMaximizing, bool prioritizeCaptures)
        {
            // Sort moves by evaluation score they produce
            var sorted = evaluationList.ToList();
            sorted.Sort(Compare);

            int Compare((double eval, SingleMove move) move1, (double eval, SingleMove move) move2)
            {
                if (Math.Abs(move1.eval - move2.eval) < 1e-6)
                {
                    if (prioritizeCaptures)
                    {
                        return move1.move.Capture.CompareTo(move2.move.Capture);
                    }
                    else
                    {
                        return 0;
                    }
                }

                if (!isMaximizing)
                {
                    if (move1.eval > move2.eval) return 1;
                    return -1;
                }
                else
                {
                    if (move1.eval < move2.eval) return 1;
                    return -1;
                }
            }

            // 
            return sorted;
        }

        /// <summary>
        /// Uses OrderBy
        /// </summary>
        /// <param name="evaluationList"></param>
        /// <param name="isMaximizing"></param>
        /// <param name="prioritizeCaptures"></param>
        /// <returns></returns>
        private static IList<(double eval, SingleMove move)> OrderEvaluatedMoves(
            IEnumerable<(double evaluationScore, SingleMove move)> evaluationList, bool isMaximizing,
            bool prioritizeCaptures)
        {
            var sorted = evaluationList.ToList();
            if (prioritizeCaptures)
            {
                sorted = sorted.OrderByDescending(item => item.move.Capture).ToList();
            }

            if (isMaximizing)
            {
                sorted = sorted.OrderByDescending(item => item.evaluationScore).ToList();
            }
            else
            {
                sorted = sorted.OrderBy(item => item.evaluationScore).ToList();
            }

            return sorted;
        }

        public static IList<SingleMove> OrderMovesByCapture(IList<SingleMove> moves)
        {
            return moves.OrderByDescending(m => m.Capture).ToList();
        }

        /// <summary>
        /// Really slow compared to OrderBy
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        public static void SortMovesByCapture(List<SingleMove> moves)
        {
            moves.Sort((move1, move2) => move1.Capture.CompareTo(move2.Capture));
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
        private static SingleMove IterativeDeepeningBasic(IList<SingleMove> allMoves, int searchDepth, Board board, bool isMaximizing)
        {
            //var result1 = new List<(double, SingleMove)>();
            //var evaluationResult = new EvaluationResult();
            var midResult = new List<(double, SingleMove)>();
            var previousOrder = new List<SingleMove>(allMoves);

            // Initial depth 2
            for (int i = 2; i <= searchDepth; i++)
            {
                midResult.Clear();
                foreach (var move in allMoves)
                {
                    var newBoard = new Board(board, move);
                    var evaluation = MiniMax.ToDepth(newBoard, i, -100000, 100000, !isMaximizing);
                    //evaluationResult.Add(evaluation, move);
                    midResult.Add((evaluation, move));
                }

                //var bestStart = evaluationResult.Best(isMaximizing);

                midResult = OrderEvaluatedMoves(midResult, isMaximizing, true).ToList();
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
                foreach (var move in allMoves)
                {
                    var newBoard = new Board(board, move);
                    var evaluation = MiniMax.ToDepthWithTranspositions(newBoard, i, -100000, 100000, !isMaximizing, true);
                    
                    // Top-level result should always be saved with priority
                    newBoard.SharedData.Transpositions.Add(newBoard.BoardHash, i, evaluation, NodeType.Exact, true);
                    midResult.Add((evaluation, move));
                }

                midResult = OrderEvaluatedMoves(midResult, isMaximizing, true).ToList();
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
