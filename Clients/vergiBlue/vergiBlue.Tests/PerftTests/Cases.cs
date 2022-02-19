using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using vergiBlue.BoardModel;

namespace PerftTests
{
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
