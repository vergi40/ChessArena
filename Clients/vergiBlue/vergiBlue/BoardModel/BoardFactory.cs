﻿using System;
using System.Collections.Generic;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public static class BoardFactory
    {
        /// <summary>
        /// Create empty board.
        /// Add pieces with <see cref="IBoard.AddNew(PieceBase)"/>
        /// Initialize hashing with <see cref="IBoard.InitializeHashing"/>
        /// </summary>
        public static IBoard Create()
        {
            // TODO rename to "CreateEmptyBoard"
            return new Board();
        }

        /// <summary>
        /// Preferred way to create board from pieces. Does initializations and hashing.
        /// </summary>
        public static IBoard CreateFromPieces(IEnumerable<PieceBase> pieces)
        {
            var board = Create();
            board.AddNew(pieces);
            board.InitializeHashing();
            return board;
        }

        /// <summary>
        /// Create board clone for testing purposes
        /// </summary>
        public static IBoard CreateClone(IBoard previous, bool cloneBoardHash = true)
        {
            return new Board(previous, cloneBoardHash);
        }
        
        /// <summary>
        /// Create board setup after move
        /// </summary>
        public static IBoard CreateFromMove(IBoard previous, SingleMove move)
        {
            return new Board(previous, move);
        }

        public static IBoard CreateDefault()
        {
            var board = Create();
            board.InitializeDefaultBoard();
            return board;
        }


        public static IBoard CreateFromFen(string fen, out bool isWhiteTurn)
        {
            var board = new Board();
            // https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation
            // Example start position
            // rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
            var components = fen.Split(' ');
            var rows = components[0].Split("/");

            // Fen row order: row 8 -> row 1 
            for (int i = 0; i < 8; i++)
            {
                var row = rows[i];
                var columnIndex = 0;
                for (int j = 0; j < row.Length; j++)
                {
                    // Account rows like 2p5
                    // Input<2>: inputIndex 0. outputIndex 0.
                    // Input<p>: inputIndex 1. outputIndex 2
                    // -> 
                    var tile = row[j];
                    if (char.IsDigit(tile))
                    {
                        var skip = int.Parse(tile.ToString());
                        columnIndex += skip;
                        continue;
                    }

                    var rowIndex = 7 - i;
                    var piece = PieceFactory.Create(tile, (columnIndex, rowIndex));
                    board.AddNew(piece);

                    columnIndex++;
                }
            }
            isWhiteTurn = char.Parse(components[1]) == 'w';

            board.Strategic.SetCastlingStatus(components[2]);
            var enPassantInput = components[3];
            if (enPassantInput != "-")
            {
                board.Strategic.EnPassantPossibility = enPassantInput.ToTuple();
            }
            //var halfMoveClock = int.Parse(components[4]);
            //var fullMoveNumber = int.Parse(components[5]);

            board.InitializeHashing();
            return board;
        }
    }
}