namespace vergiBlue.BoardModel
{
    public static class BoardFactory
    {
        /// <summary>
        /// Start game initialization
        /// </summary>
        public static Board Create()
        {
            return new Board();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        public static Board CreateClone(IBoard previous)
        {
            return new Board(previous);
        }

        /// <summary>
        /// Create board setup after move
        /// </summary>
        public static Board CreateFromMove(IBoard previous, SingleMove move)
        {
            return new Board(previous, move);
        }
    }
}