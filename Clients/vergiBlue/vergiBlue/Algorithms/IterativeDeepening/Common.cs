using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems.TranspositionTables;
using vergiBlue.Logic;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    internal class Common
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<Common>();

        public static (int maxDepth, int timeLimit) DefineDepthAndTime(BoardContext context, SearchParameters parameters)
        {
            var uciParameters = parameters.UciParameters;
            var limits = uciParameters.SearchLimits;

            // Infinite -> use really large depth. Not set -> use some default
            var infinite = uciParameters.Infinite;
            int maxDepth = 11;
            if (infinite) maxDepth = 100;
            else if (limits.Depth != 0) maxDepth = limits.Depth;

            // Default = max value (e.g. command was infinite or just depth or nodecount constraints
            var timeLimit =  int.MaxValue;
            if (limits.Time != 0) timeLimit = limits.Time;
            else if (parameters.TurnStartInfo.isWhiteTurn)
            {
                if (uciParameters.WhiteTimeLeft > 0)
                {
                    // Use default
                    timeLimit = context.MaxTimeMs;
                }
            }
            else if (!parameters.TurnStartInfo.isWhiteTurn)
            {
                if (uciParameters.BlackTimeLeft > 0)
                {
                    // Use default
                    timeLimit = context.MaxTimeMs;
                }
            }

            return (maxDepth, timeLimit);
        }

        public static void AddIterativeDeepeningResultDiagnostics(int depthUsed, int totalMoveCount, int searchMoveCount, double evaluation, SingleMove? move = null, IBoard? board = null)
        {
            if (searchMoveCount < totalMoveCount)
            {
                Collector.AddCustomMessage($" Iterative deepening search depth was {depthUsed - 1} [partial {depthUsed}: ({searchMoveCount}/{totalMoveCount})].");
            }
            else
            {
                Collector.AddCustomMessage($" Iterative deepening search depth was {depthUsed} ({searchMoveCount}/{totalMoveCount}).");
            }
            Collector.AddCustomMessage($" Move evaluation: {evaluation}.");

            // DEBUG
            //if (move != null && board != null && board.Strategic.EndGameWeight > 0.50)
            //{
            //    var newBoard = BoardFactory.CreateFromMove(board, move);
            //    var isWhite = newBoard.ValueAtDefinitely(move.NewPos).IsWhite;
            //    Diagnostics.AddMessage(" EndGameKingToCornerEvaluation: " + Evaluator.EndGameKingToCornerEvaluation(newBoard, isWhite));
            //}
        }

        /// <summary>
        /// The "best" course of action for root player - aka move sequence it considers has best evaluation
        /// </summary>
        /// <param name="board"></param>
        /// <param name="rootMove"></param>
        /// <param name="rootIsMaximizing"></param>
        /// <returns></returns>
        public static List<ISingleMove> GetPrincipalVariation(int maxDepth, IBoard board, ISingleMove rootMove, bool rootIsMaximizing)
        {
            var transpositions = board.Shared.Transpositions;
            var result = new List<ISingleMove>(){rootMove};
            var nextMove = rootMove;
            var nextIsMaximizing = rootIsMaximizing;

            // E.g. this board = white
            // i = 0, black
            var nextBoard = board;

            try
            {
                for(int i = 0; i < maxDepth; i++)
                {
                    nextBoard = BoardFactory.CreateFromMove(nextBoard, nextMove);
                    nextIsMaximizing = !nextIsMaximizing;
                    var nextMoves = nextBoard.MoveGenerator.ValidMovesQuick(nextIsMaximizing).ToList();
                    if (!nextMoves.Any())
                    {
                        // Game ended for checkmate/stalemate
                        break;
                    }

                    var hash = nextBoard.BoardHash;

                    if (!transpositions.TryGet(hash, out var entry))
                    {
                        // Not found. Can happen for reason ....?
                        break;
                    }

                    if (entry.BestMove == null || entry.Type != NodeType.Exact)
                    {
                        // Best move since there is e.g. checkmate or great cutoff
                        break;
                    }
                    else
                    {
                        nextMove = entry.BestMove;
                        result.Add(nextMove);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error finding PV moves: {e.Message}");
            }
            
            return result;
        }

        public static string GetPrincipalVariationAsString(int maxDepth, IBoard board, ISingleMove rootMove, bool rootIsMaximizing)
        {
            var pv = GetPrincipalVariation(maxDepth, board, rootMove, rootIsMaximizing);
            return string.Join(", ", pv.Select(move => move.ToCompactString()));
        }

        public static void AddPVDiagnostics(int maxDepth, IBoard board, ISingleMove bestMove, bool rootIsMaximizing)
        {
            var pv = GetPrincipalVariationAsString(maxDepth, board, bestMove, rootIsMaximizing);
            var message = $"[PV] Depth: {maxDepth}, {pv}";
            
            Collector.AddCustomMessage($"{message}");
        }

        public static void DebugPrintWeighedMoves(List<(double weight, SingleMove move)> weightedMoves)
        {
            var message = new StringBuilder("Evaluation for all moves:");
            message.Append(Environment.NewLine);
            foreach (var (weight, move) in weightedMoves)
            {
                message.Append($"{move.ToCompactString()}: {weight}{Environment.NewLine}");
            }
            Debug.Print(message.ToString());
        }
    }
}
