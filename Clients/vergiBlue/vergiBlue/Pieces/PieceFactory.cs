using System;

namespace vergiBlue.Pieces
{
    public class PieceFactory
    {
        public static PieceBase Create(char identity, (int column, int row) position)
        {
            var isWhite = char.IsUpper(identity);
            var id = char.ToUpper(identity);

            PieceBase piece = id switch
            {
                'K' => new King(isWhite, position),
                'Q' => new Queen(isWhite, position),
                'R' => new Rook(isWhite, position),
                'N' => new Knight(isWhite, position),
                'B' => new Bishop(isWhite, position),
                'P' => new Pawn(isWhite, position),
                _ => throw new ArgumentException($"Unknown piece type: {identity}")
            };
            return piece;
        }
    }
}
