using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace vergiBlue
{
    public static class SingleMoveFactory
    {
        /// <summary>
        /// Capture own piece. For internal attack squares
        /// </summary>
        public static SingleMove CreateSoftTarget((int column, int row) previousPosition,
            (int column, int row) newPosition)
        {
            return new SingleMove(previousPosition, newPosition, true)
            {
                SoftTarget = true
            };
        }

        public static SingleMove CreateCastling((int column, int row) previousPosition,
            (int column, int row) newPosition)
        {
            return new SingleMove(previousPosition, newPosition)
            {
                Castling = true
            };
        }

        public static SingleMove CreateEmpty()
        {
            return new SingleMove((-1, -1), (-1, -1));
        }

        /// <summary>
        /// Move with compact parameter e.g. "a1b1" or "c4f1"
        /// </summary>
        public static SingleMove Create(string compact, bool capture = false)
        {
            if (compact.Length != 4)
                throw new ArgumentException($"Compact move string {compact} should have 4 characters.");

            var prev = compact.Substring(0, 2);
            var next = compact.Substring(2, 2);

            return new SingleMove(prev, next, capture);
        }
    }
}
