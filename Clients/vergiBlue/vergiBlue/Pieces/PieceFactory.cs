using System;
using System.Collections.Generic;

namespace vergiBlue.Pieces
{
    public class PieceFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity">Upper case: white. Lower case: black</param>
        /// <param name="position"></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public static PieceBase Create(char identity, (int column, int row) position, bool isWhite)
        {
            PieceBase piece = identity switch
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identityWithColor">Upper case: white. Lower case: black</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static PieceBase Create(char identityWithColor, (int column, int row) position)
        {
            var isWhite = char.IsUpper(identityWithColor);
            var id = char.ToUpper(identityWithColor);

            PieceBase piece = id switch
            {
                'K' => new King(isWhite, position),
                'Q' => new Queen(isWhite, position),
                'R' => new Rook(isWhite, position),
                'N' => new Knight(isWhite, position),
                'B' => new Bishop(isWhite, position),
                'P' => new Pawn(isWhite, position),
                _ => throw new ArgumentException($"Unknown piece type: {identityWithColor}")
            };
            return piece;
        }

        public static PieceBase Create(char identityWithColor, string algebraic)
        {
            return Create(identityWithColor, algebraic.ToTuple());
        }

        /// <summary>
        /// Create full board with syntax e.g. ("a1K", "b2P", "g5k")
        /// </summary>
        /// <param name="posAndIdentityList"></param>
        /// <returns></returns>
        public static List<PieceBase> CreateWithShortSyntax(params string[] posAndIdentityList)
        {
            var result = new List<PieceBase>();
            foreach (var piece in posAndIdentityList)
            {
                var pos = piece.Substring(0, 2);
                var identityWithColor = piece[2];
                result.Add(Create(identityWithColor, pos));
            }

            return result;
        }
    }
}
