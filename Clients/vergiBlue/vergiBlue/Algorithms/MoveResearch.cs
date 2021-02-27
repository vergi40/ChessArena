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
        public static double DefaultAlpha => -1000000;
        public static double DefaultBeta => 1000000;
        
        // TODO Delete. somehow unnecessary and complicated.
        public static EvaluationResult GetMoveScoreList(IList<SingleMove> moves,
            int searchDepth, Board board, bool isMaximizing, bool useTranspositions)
        {
            var result = new EvaluationResult();
            var alpha = DefaultAlpha;
            var beta = DefaultBeta;

            if (useTranspositions)
            {
                foreach (var move in moves)
                {
                    var transposition = board.Shared.Transpositions.GetTranspositionForMove(board, move);
                    if (transposition != null && transposition.Depth >= searchDepth)
                    {
                        // Saved some time
                        // TODO extra parameters to evaluationresult if this was lower or upper bound
                        result.Add(transposition.Evaluation, move);
                    }
                    else
                    {
                        // Board evaluation at current depth
                        var newBoard = new Board(board, move);
                        var value = MiniMax.ToDepthWithTranspositions(newBoard, searchDepth, alpha, beta,
                            !isMaximizing);
                        result.Add(value, move);

                        // Add new transposition table
                        newBoard.Shared.Transpositions.Add(newBoard.BoardHash, searchDepth, value, NodeType.Exact, newBoard.Shared.GameTurnCount);

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
                    var value = MiniMax.ToDepth(newBoard, searchDepth, DefaultAlpha, DefaultBeta, !isMaximizing);
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
            // 
            var minimumSearchPercentForHigherDepthUse = 1 / (double) 3;
            var timeUp = false;
            int depthUsed = 0;
            
            var midResult = new List<(double weight, SingleMove move)>();
            var currentIterationMoves = new List<SingleMove>(allMoves);
            (double eval, SingleMove move) previousIterationBest = new (0.0, new SingleMove((-1,-1),(-1,-1)));
            var watch = new Stopwatch();
            watch.Start();

            // Why this works for black start, but not white?
            //var alpha = -1000000.0;
            //var beta = 1000000.0;

            // Initial depth 2
            for (int i = 2; i <= searchDepth; i++)
            {
                var alpha = DefaultAlpha;
                var beta = DefaultBeta;
                depthUsed = i;
                midResult.Clear();
                
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

                
                // Full search finished for depth
                midResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();

                // Found checkmate
                //if (isMaximizing && midResult.First().weight > PieceBaseStrength.CheckMateThreshold
                //    || !isMaximizing && midResult.First().weight < -PieceBaseStrength.CheckMateThreshold)
                //{
                //    // TODO This might result in stupid movements, if opponent doesn't do the exact move AI thinks is best for it
                    
                //    Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed}. Check mate found.");
                //    Diagnostics.AddMessage($" Move evaluation: {midResult.First().weight}.");
                //    return midResult.First().move;
                //}
                
                if (timeUp) break;

                currentIterationMoves = midResult.Select(item => item.Item2).ToList();
                previousIterationBest = midResult.First();
            }
            
            // midResult is either partial or full. Just sort and return first.

            // If too small percent was searched for new depth, use prevous results
            // E.g. out of 8 possible moves, only 2 were searched
            if (midResult.Count / (double) allMoves.Count < minimumSearchPercentForHigherDepthUse)
            {
                var result = previousIterationBest;
                AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, result.eval, result.move, board);
                return result.move;
            }
            
            var finalResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
            AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, finalResult.First().weight, finalResult.First().move, board);
            return finalResult.First().move;
        }

        private static void AddIterativeDeepeningResultDiagnostics(int depthUsed, int totalMoveCount, int searchMoveCount, double evaluation, SingleMove? move = null, Board? board = null)
        {
            if (searchMoveCount < totalMoveCount)
            {
                Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed - 1} [partial {depthUsed}: ({searchMoveCount}/{totalMoveCount})].");
            }
            else
            {
                Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed} ({searchMoveCount}/{totalMoveCount}).");
            }
            Diagnostics.AddMessage($" Move evaluation: {evaluation}.");

            // DEBUG
            if (move != null && board != null && board.Strategic.EndGameWeight > 0.50)
            {
                var newBoard = new Board(board, move);
                var isWhite = newBoard.ValueAtDefinitely(move.NewPos).IsWhite;
                Diagnostics.AddMessage(" EndGameKingToCornerEvaluation: " + newBoard.EndGameKingToCornerEvaluation(isWhite));
            }
        }

        /// <summary>
        /// Iterative deepening sub-method.
        /// Evaluate moves at search depth 2. Reorder. Evaluate moves at search depth 3. Reorder ...
        /// 
        /// </summary>
        private static SingleMove IterativeDeepeningWithTranspositions(IList<SingleMove> allMoves, int searchDepth, Board board, bool isMaximizing, int timeLimitInMs = 5000)
        {
            // 
            var minimumSearchPercentForHigherDepthUse = 1 / (double)3;
            var timeUp = false;
            int depthUsed = 0;

            var midResult = new List<(double weight, SingleMove move)>();
            var currentIterationMoves = new List<SingleMove>(allMoves);
            (double eval, SingleMove move) previousIterationBest = new(0.0, new SingleMove((-1, -1), (-1, -1)));
            var watch = new Stopwatch();
            watch.Start();

            // Why this works for black start, but not white?
            //var alpha = -1000000.0;
            //var beta = 1000000.0;

            // Initial depth 2
            for (int i = 2; i <= searchDepth; i++)
            {
                var alpha = DefaultAlpha;
                var beta = DefaultBeta;
                depthUsed = i;
                midResult.Clear();

                foreach (var move in currentIterationMoves)
                {
                    var newBoard = new Board(board, move);
                    var evaluation = MiniMax.ToDepthWithTranspositions(newBoard, i, alpha, beta, !isMaximizing);
                    midResult.Add((evaluation, move));
                    
                    if (isMaximizing)
                    {
                        alpha = Math.Max(alpha, evaluation);
                        if (alpha >= beta) { /* */ }
                        else
                        {
                            //newBoard.Shared.Transpositions.Add(newBoard.BoardHash, i, evaluation, NodeType.Exact, newBoard.Shared.GameTurnCount);
                        }
                    }
                    else
                    {
                        beta = Math.Min(beta, evaluation);
                        if(beta <= alpha) { /* */ }
                        else
                        {
                            //newBoard.Shared.Transpositions.Add(newBoard.BoardHash, i, evaluation, NodeType.Exact, newBoard.Shared.GameTurnCount);
                        }
                    }

                    if (watch.ElapsedMilliseconds > timeLimitInMs)
                    {
                        timeUp = true;
                        break;
                    }
                }


                // Full search finished for depth
                midResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();

                // Found checkmate
                //if (isMaximizing && midResult.First().weight > PieceBaseStrength.CheckMateThreshold
                //    || !isMaximizing && midResult.First().weight < -PieceBaseStrength.CheckMateThreshold)
                //{
                //    // TODO This might result in stupid movements, if opponent doesn't do the exact move AI thinks is best for it

                //    Diagnostics.AddMessage($" Iterative deepening search depth was {depthUsed}. Check mate found.");
                //    Diagnostics.AddMessage($" Move evaluation: {midResult.First().weight}.");
                //    return midResult.First().move;
                //}

                if (timeUp) break;

                currentIterationMoves = midResult.Select(item => item.Item2).ToList();
                previousIterationBest = midResult.First();
            }

            // midResult is either partial or full. Just sort and return first.

            // If too small percent was searched for new depth, use prevous results
            // E.g. out of 8 possible moves, only 2 were searched
            if (midResult.Count / (double)allMoves.Count < minimumSearchPercentForHigherDepthUse)
            {
                var result = previousIterationBest;
                AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, result.eval, result.move, board);
                return result.move;
            }

            var finalResult = MoveOrdering.SortWeightedMovesWithSort(midResult, isMaximizing).ToList();
            AddIterativeDeepeningResultDiagnostics(depthUsed, allMoves.Count, midResult.Count, finalResult.First().weight, finalResult.First().move, board);
            return finalResult.First().move;
        }

        
        // TODO Could move to other place
        public static double CheckMateScoreAdjustToEven(double evalScore)
        {
            if(Math.Abs(evalScore) > PieceBaseStrength.CheckMateThreshold)
            {
                if (evalScore > 0)
                {
                    return PieceBaseStrength.King;
                }
                else
                {
                    return -PieceBaseStrength.King;
                }
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
                else
                {
                    return -PieceBaseStrength.King - depth;
                }
            }

            return evalScore;
        }
    }
}
