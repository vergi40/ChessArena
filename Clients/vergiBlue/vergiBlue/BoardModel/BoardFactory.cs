using System;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel
{
    public static class BoardFactory
    {
        /// <summary>
        /// Create empty board. Add pieces with <see cref="Board.AddNew(vergiBlue.Pieces.PieceBase)"/>
        /// </summary>
        public static IBoard Create()
        {
            return new Board();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        public static IBoard CreateClone(IBoard previous)
        {
            return new Board(previous);
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
            var enPassantTarget = components[3];
            //var halfMoveClock = int.Parse(components[4]);
            //var fullMoveNumber = int.Parse(components[5]);

            return board;
        }
    }
}