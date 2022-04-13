using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using vergiBlue.Analytics;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.Subsystems.TranspositionTables;

namespace vergiBlue.Algorithms.IterativeDeepening
{
    internal static class Common
    {
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

        public static void AddPVDiagnostics(int depthUsed, IBoard board, ISingleMove bestMove, bool rootIsMaximizing)
        {
            var transpositions = board.Shared.Transpositions;
            var message = new StringBuilder();
            var nextMove = bestMove;
            var nextIsMaximizing = rootIsMaximizing;

            // E.g. this board = white
            // i = 0, black
            var nextBoard = board;

            message.Append($"[PVS] Depth: {depthUsed}, {nextMove.ToCompactString()} ");

            try
            {
                for (int i = 0; i < depthUsed; i++)
                {
                    nextBoard = BoardFactory.CreateFromMove(nextBoard, nextMove);
                    nextIsMaximizing = !nextIsMaximizing;
                    var nextMoves = nextBoard.MoveGenerator.ValidMovesQuick(nextIsMaximizing).ToList();
                    if (!nextMoves.Any())
                    {
                        // 
                        message.Append("checkmate or stalemate");
                        break;
                    }

                    var hash = nextBoard.BoardHash;

                    if (!transpositions.TryGet(hash, out var entry))
                    {
                        message.Append("??? ");
                        break;
                    }

                    //Debug.Assert(entry.BestMove != null, "Principal variation was not saved");
                    if (entry.BestMove == null || entry.Type != NodeType.Exact)
                    {
                        message.Append("??? ");
                        break;
                    }
                    else
                    {
                        nextMove = entry.BestMove;
                        message.Append($"{nextMove.ToCompactString()} ");
                    }
                }
            }
            catch (Exception e)
            {
                Collector.AddCustomMessage($"[PVS] Error {e.Message}");
                return;
            }

            Collector.AddCustomMessage($"{message.ToString()}");
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
