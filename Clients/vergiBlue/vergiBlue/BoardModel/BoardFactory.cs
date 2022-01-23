namespace vergiBlue.BoardModel
{
    public static class BoardFactory
    {
        /// <summary>
        /// Start game initialization
        /// </summary>
        public static IBoard Create()
        {
            return new Board();
        }

        /// <summary>
        /// Create board clone for testing purposes. Set kings explicitly
        /// </summary>
        public static IBoard CreateClone(IBoard previous)
        {
            return new Board(previous);
        }

        /// <summary>
        /// Create board setup after move
        /// </summary>
        public static IBoard CreateFromMove(IBoard previous, SingleMove move)
        {
            return new Board(previous, move);
        }

        public static IBoard CreateDefault()
        {
            var board = Create();
            board.InitializeDefaultBoard();
            return board;
        }
    }
}