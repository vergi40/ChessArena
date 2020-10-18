using System.Collections.Generic;

namespace vergiBlue.Pieces
{
    public abstract class PieceBase
    {
        public bool IsOpponent { get; }
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

        public Board Board { get; }

        public (int column, int row) CurrentPosition { get; set; }

        protected PieceBase(bool isOpponent, bool isWhite, Board boardReference)
        {
            IsOpponent = isOpponent;
            IsWhite = isWhite;
            Board = boardReference;
        }

        /// <summary>
        /// Try if move can be made. Return outcome.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="validateBorders"></param>
        /// <returns>Null if not possible</returns>
        protected abstract SingleMove CanMoveTo((int, int) target, bool validateBorders = false);

        public void MoveTo((int column, int row) target)
        {
            var move = new SingleMove(CurrentPosition, target);
            Board.ExecuteMove(move);
        }

        /// <summary>
        /// Each move the piece can make in current board setting
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SingleMove> Moves();

        /// <summary>
        /// Copy needs to be made with the derived class constructor so type matches
        /// </summary>
        /// <param name="newBoard"></param>
        /// <returns></returns>
        public abstract PieceBase CreateCopy(Board newBoard);
    }
}
