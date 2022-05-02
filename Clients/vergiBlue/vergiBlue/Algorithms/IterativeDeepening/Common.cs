using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems.TranspositionTables;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    internal static class Common
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Common));
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
        public static List<ISingleMove> GetPrincipalVariation(IBoard board, ISingleMove rootMove, bool rootIsMaximizing)
        {
            var transpositions = board.Shared.Transpositions;
            var result = new List<ISingleMove>();
            var nextMove = rootMove;
            var nextIsMaximizing = rootIsMaximizing;

            // E.g. this board = white
            // i = 0, black
            var nextBoard = board;

            try
            {
                while(true)
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
                _logger.Error($"Error finding PV moves: {e.Message}");
            }
            
            return result;
        }

        public static string GetPrincipalVariationAsString(IBoard board, ISingleMove rootMove, bool rootIsMaximizing)
        {
            var pv = GetPrincipalVariation(board, rootMove, rootIsMaximizing);
            return string.Join(", ", pv.Select(move => move.ToCompactString()));
        }

        public static void AddPVDiagnostics(int depthUsed, IBoard board, ISingleMove bestMove, bool rootIsMaximizing)
        {
            var pv = GetPrincipalVariationAsString(board, bestMove, rootIsMaximizing);
            var message = $"[PV] Depth: {depthUsed}, {pv}";
            
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
