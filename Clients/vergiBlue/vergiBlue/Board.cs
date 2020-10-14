using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    public class Board
    {
        /// <summary>
        /// Pieces are storaged with (column,row) pair. On algebraic notation [0,0] corresponds to the "a1" notations.
        /// Indexes start from 0
        /// </summary>
        public Dictionary<(int column, int row), Piece> Pieces { get; set; } = new Dictionary<(int, int), Piece>();

        // Reference
        public Dictionary<(int, int), Piece>.ValueCollection PieceList => Pieces.Values;
        public Dictionary<(int, int), Piece>.KeyCollection OccupiedCoordinates => Pieces.Keys;

        /// <summary>
        /// Return piece at coordinates, null if empty.
        /// </summary>
        /// <returns>Can be null</returns>
        public Piece ValueAt((int, int) target)
        {
            if (Pieces.ContainsKey(target)) return Pieces[target];
            return null;
        }

        public void AddNew(Piece piece)
        {
            Pieces.Add((piece.CurrentPosition), piece);
        }
    }
}
