using NUnit.Framework;
using vergiBlue.BoardModel;

namespace PerftTests
{
    /// <summary>
    /// For a particular position and search depth a perft value is the number of nodes or positions resulting from legal moves.
    /// So for example perft(depth = 1) from the initial board position is 20, representing the total number of legal moves available for white.
    /// https://www.chessprogramming.org/Perft_Results
    /// https://sites.google.com/site/numptychess/perft
    /// </summary>
    public static class Cases
    {
        public static long StartPosition(int depth)
        {
            var board = BoardFactory.CreateDefault();

            var result = Perft.PerftRec(board, depth, true);
            TestContext.WriteLine($"{nameof(StartPosition)} with depth {depth}: node count {result}");
            return result;
        }

        /// <summary>
        /// Promotion, castling, en passant
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        public static long GoodPositions_AndrewWagner(int depth)
        {
            var fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out var whiteStart);

            var result = Perft.PerftRec(board, depth, whiteStart);
            return result;
        }

        /// <summary>
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        public static long Promotion_AndrewWagner(int depth)
        {
            var fen = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out var whiteStart);

            var result = Perft.PerftRec(board, depth, whiteStart);
            return result;
        }
    }
}
