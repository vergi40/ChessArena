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
    }
}
