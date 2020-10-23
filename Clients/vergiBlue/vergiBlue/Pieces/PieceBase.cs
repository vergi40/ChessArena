using System.Collections.Generic;

namespace vergiBlue.Pieces
{
    public abstract class PieceBase
    {
        public bool IsWhite { get; }

        public abstract double RelativeStrength { get; }

        /// <summary>
        /// Sign of general direction. Can also be used to classify white as positive and black as negative value.
        /// </summary>
        public int Direction
        {
            get
            {
                if (IsWhite) return 1;
                return -1;
            }
        }

        public (int column, int row) CurrentPosition { get; set; }

        /// <summary>
        /// If using this, need to set position explicitly
        /// </summary>
        /// <param name="isWhite"></param>
        protected PieceBase(bool isWhite)
        {
            IsWhite = isWhite;
        }

        protected PieceBase(bool isWhite, (int column, int row) position)
        {
            IsWhite = isWhite;
            CurrentPosition = position;
        }

        protected PieceBase(bool isWhite, string position)
        {
            IsWhite = isWhite;
            CurrentPosition = position.ToTuple();
        }

        /// <summary>
        /// Each move the piece can make in current board setting
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SingleMove> Moves(Board board);

        /// <summary>
        /// Copy needs to be made with the derived class constructor so type matches
        /// </summary>
        /// <returns></returns>
        public abstract PieceBase CreateCopy();
    }
}
