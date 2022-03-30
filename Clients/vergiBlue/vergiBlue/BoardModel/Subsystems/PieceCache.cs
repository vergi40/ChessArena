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
    public sealed class PieceCache
    {
        // Efficiency: Use array
        private Dictionary<char, PieceDict> AllPieces { get; } = new();

        public void Initialize()
        {
            AllPieces.Add('P', PieceDict.Create('P'));
            AllPieces.Add('K', PieceDict.Create('K'));
            AllPieces.Add('N', PieceDict.Create('N'));
            AllPieces.Add('R', PieceDict.Create('R'));
            AllPieces.Add('B', PieceDict.Create('B'));
            AllPieces.Add('Q', PieceDict.Create('Q'));
        }

        public IPiece Get((int column, int row) position, char identity, bool isWhite)
        {
            return Get(position.To1DimensionArray(), identity, isWhite);
        }

        public IPiece Get(int position1D, char identity, bool isWhite)
        {
            return AllPieces[identity].Pieces[isWhite][position1D];
        }

        // Private classes or records which are not derived in current assembly should be marked as 'sealed'
        private sealed class PieceDict
        {
            public Dictionary<bool, IPiece[]> Pieces { get; } = new();


            public static PieceDict Create(char identity)
            {
                var dict = new PieceDict();

                var whites = new IPiece[64];
                for (int i = 0; i < 64; i++)
                {
                    var piece = PieceFactory.Create(identity, i.ToTuple());
                    whites[i] = piece;
                }

                dict.Pieces[true] = whites;

                // PieceFactory uses lowercase for black
                var toLower = char.ToLower(identity);
                var blacks = new IPiece[64];
                for (int i = 0; i < 64; i++)
                {
                    var piece = PieceFactory.Create(toLower, i.ToTuple());
                    blacks[i] = piece;
                }

                dict.Pieces[false] = blacks;
                return dict;
            }
        }
    }
}
