using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.BoardModel;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.Parallel
{
    internal class ParallelBasic : IAlgorithm
    {
        public SingleMove CalculateBestMove(BoardContext context)
        {
            var evaluated = GetMoveScoreListParallel(context.ValidMoves, context.NominalSearchDepth, context.CurrentBoard,
                context.IsWhiteTurn);

            if (evaluated.Empty)
            {
                throw new ArgumentException($"Logical error - parallel computing lost moves during evaluation.");
            }

            return MoveResearch.SelectBestMove(evaluated, context.IsWhiteTurn, true);
        }

        public static EvaluationResult GetMoveScoreListParallel(IReadOnlyList<SingleMove> moves, int searchDepth, IBoard board, bool isMaximizing, int timeLimitInMs = 5000)
        {
            var result = new EvaluationResult();
            var evaluated = new List<(double, SingleMove)>();
            var syncObject = new object();
            var timer = SearchTimer.Start(timeLimitInMs);

            // TODO maybe refactor to some task factories to simplify main loop

            // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-write-a-parallel-for-loop-with-thread-local-variables
            System.Threading.Tasks.Parallel.ForEach(moves,
                () => new List<(double, SingleMove)>(), // Local initialization. Need to inform compiler the type by initializing
                (move, loopState, localState) => // Predefined lambda expression (Func<SingleMove, ParallelLoopState, thread-local variable, body>)
                {
                    var newBoard = BoardFactory.CreateFromMove(board, move);
                    var value = MiniMax.ToDepth(newBoard, searchDepth, MiniMaxGeneral.DefaultAlpha, MiniMaxGeneral.DefaultBeta, !isMaximizing, timer);
                    localState.Add((value, move));
                    return localState;
                },
                finalResult =>
                {
                    lock (syncObject) evaluated.AddRange(finalResult);
                });

            result.Add(evaluated);
            return result;
        }
    }
}
