using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vergiBlue.Pieces;

namespace vergiBlue.BoardModel.Subsystems
{
    /// <summary>
    /// Instead of initializing new pieces, pick them using identity and position
    /// </summary>
    public class PieceCache
    {
        /// <summary>
        /// All pieces for each position and color in single array
        /// Pawn blacks, whites, king blacks, whites ... etc
        /// </summary>
        private IPiece[] AllPieces { get; } = new IPiece[6*2*64];

        public void Initialize()
        {
            AddPieces('P');
            AddPieces('K');
            AddPieces('N');
            AddPieces('R');
            AddPieces('B');
            AddPieces('Q');
        }

        private void AddPieces(char identity)
        {
            var pieceIndex = PieceToInt(identity);
            var blackIdentity = char.ToLower(identity);

            for (int i = 0; i < 64; i++)
            {
                // Black pawn 0-63
                // White pawn 64 - 127
                // Black king 128 - 191 etc...
                var index = i + (64 * 2 * pieceIndex);
                AllPieces[index] = PieceFactory.Create(blackIdentity, i.ToTuple());
                AllPieces[index+64] = PieceFactory.Create(identity, i.ToTuple());
            }
        }

        public IPiece Get((int column, int row) position, char identity, bool isWhite)
        {
            return Get(position.To1DimensionArray(), identity, isWhite);
        }

        public IPiece Get(int position1D, char identity, bool isWhite)
        {
            // Black pawn 0-63
            // White pawn 64 - 127
            // Black king 128 - 191 etc...
            var pieceIndex = PieceToInt(identity);
            var colorIndex = ColorToInt(isWhite);
            var pieceBaseIndex = 64 * 2 * pieceIndex;
            return AllPieces[pieceBaseIndex + 64 * colorIndex + position1D];
        }
        
        protected static int PieceToInt(char identity)
        {
            return identity switch
            {
                'P' => 0,
                'K' => 1,
                'N' => 2,
                'R' => 3,
                'B' => 4,
                'Q' => 5,
                _ => throw new ArgumentException($"Unknown identity: {identity}")
            };
        }

        protected static int ColorToInt(bool isWhite)
        {
            return isWhite ? 1 : 0;
        }
    }
}
