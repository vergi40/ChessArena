﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard.Common;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public static class Validator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidMoveException"></exception>
        public static void ValidateMove(IBoard board, SingleMove move)
        {
            if (IsOutside(move.PrevPos))
            {
                throw new InvalidMoveException($"Invalid move. Start position outside of board {move.PrevPos}");
            }
            if (IsOutside(move.NewPos))
            {
                throw new InvalidMoveException($"Invalid move. End position outside of board {move.NewPos}");
            }
            if (board.ValueAt(move.PrevPos) == null)
            {
                throw new InvalidMoveException($"Invalid move. No piece at start pos {move.PrevPos.ToAlgebraic()}");
            }

            // Check that is really valid move for current player
            var piece = board.ValueAtDefinitely(move.PrevPos);
            var validMoves = board.MoveGenerator.MovesQuick(piece.IsWhite, true);
            if (!validMoves.Any(m => m.EqualPositions(move)))
            {
                throw new InvalidMoveException(
                    $"Invalid move. Cannot move {piece.Identity} from {move.PrevPos} to {move.NewPos}. " +
                    $"Valid moves: {string.Join(", ", piece.Moves(board))}");
            }
        }

        public static bool IsOutside((int, int) target)
        {
            if (target.Item1 < 0 || target.Item1 > 7 || target.Item2 < 0 || target.Item2 > 7)
                return true;
            return false;
        }

        public static bool IsLegalMove(SingleMove move, IBoard board, PieceBase piece, (int column, int row) kingLocation)
        {
            // https://chess.stackexchange.com/a/16901
            // Simple and neat steps

            var forWhite = piece.IsWhite;
            HashSet<(int column, int row)>? opponentCaptures = null;
            
            // TODO boardfactory light
            var newBoard = BoardFactory.CreateFromMove(board, move);

            // Modern engines
            // en passant tricky. test whether king is attacked after move made
            if (move.EnPassant)
            {
                opponentCaptures = GenerateOpponentCaptures(!forWhite, newBoard, move);
                if (opponentCaptures.Contains(kingLocation)) return false;
            }
            
            // king: test if king is attacked after move is made
            // TODO benchmark if just using yield return in captures quicker
            if (piece.Identity == 'K')
            {
                opponentCaptures ??= GenerateOpponentCaptures(!forWhite, newBoard, move);

                if (opponentCaptures.Contains(move.NewPos)) return false;
            }

            // others: if not pinned -> move ok. if pinned -> if it's moving along the attack ray, ok
            foreach (var slider in board.MoveGenerator.GenerateSliders(!forWhite, newBoard))
            {
                if (slider.AttackLine.Contains(move.PrevPos))
                {
                    // pinned found
                    if (!slider.AttackLine.Contains(move.NewPos) && move.NewPos != slider.Attacker)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static HashSet<(int column, int row)> GenerateOpponentCaptures(bool opponentWhite, IBoard opponentBoard,
            SingleMove move)
        {
            // TODO boardfactory light
            // TODO which is quicker: definitely check once each piece (hashset) vs yield return? create benchmark
            return opponentBoard.MoveGenerator.AttackMoves(opponentWhite).Select(m => m.NewPos).ToHashSet();
        }
    }
}
