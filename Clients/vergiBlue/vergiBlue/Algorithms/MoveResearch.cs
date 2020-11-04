using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue.Algorithms
{
    public class MoveResearch
    {
        public static IEnumerable<(double evaluationScore, SingleMove move)> GetMoveScoreListParallel(IList<SingleMove> moves, int searchDepth, Board board, bool isMaximizing)
        {
            var evaluated = new List<(double, SingleMove)>();
            var syncObject = new object();

            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
            Parallel.ForEach(moves,
                () => (0.0, new SingleMove("a1", "a1")), // Local initialization. Need to inform compiler the type by initializing
                (move, loopState, localState) => // Predefined lambda expression (Func<SingleMove, ParallelLoopState, thread-local variable, body>)
                {
                    var newBoard = new Board(board, move);
                    var value = MiniMax.ToDepth(newBoard, searchDepth, -100000, 100000, !isMaximizing);
                    localState = (value, move);
                    return localState;
                },
                (finalResult) =>
                {
                    lock (syncObject) evaluated.Add(finalResult);
                });

            return evaluated;
        }

        public static SingleMove? SelectBestMove(IEnumerable<(double evaluationScore, SingleMove move)> evaluationList, bool isMaximizing)
        {
            SingleMove? bestMove = null;
            var bestValue = WorstValue(isMaximizing);

            foreach (var tuple in evaluationList)
            {
                var value = tuple.Item1;
                var singleMove = tuple.Item2;
                if (isMaximizing)
                {
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestMove = singleMove;
                    }
                }
                else
                {
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestMove = singleMove;
                    }
                }
            }

            return bestMove;
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

        public static List<SingleMove> OrderMovesByEvaluation(List<SingleMove> moves, Board board, bool isMaximizing)
        {
            // Sort moves by evaluation score they produce
            var list = CreateEvaluationList(moves, board, isMaximizing).ToList();
            list.Sort(Compare);

            int Compare((double eval, SingleMove move) move1, (double eval, SingleMove move) move2)
            {
                if (Math.Abs(move1.eval - move2.eval) < 1e-6) return 0;

                if(!isMaximizing)
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
            return list.Select(m => m.move).ToList();
        }

        public static List<SingleMove> OrderMovesByCapture(List<SingleMove> moves)
        {
            return moves.OrderByDescending(m => m.Capture).ToList();
        }
    }
}
