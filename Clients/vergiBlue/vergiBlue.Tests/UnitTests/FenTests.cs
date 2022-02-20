using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue.BoardModel;

namespace UnitTests
{
    [TestClass]
    public class FenTests
    {
        [TestMethod]
        public void DefaultBoard_ShouldMatch()
        {
            var startLayout = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            var board = BoardFactory.CreateDefault();
            var fenBoard = BoardFactory.CreateFromFen(startLayout, out _);

            CommonAsserts.ShouldMatch(board, fenBoard);
        }

        [TestMethod]
        public void TurnColor_ShouldBeWhite()
        {
            var fen = "8/8/8/8/8/8/8/8 w - - 0 0";
            _ = BoardFactory.CreateFromFen(fen, out var isWhite);
            isWhite.ShouldBeTrue();
        }

        [TestMethod]
        public void TurnColor_ShouldBeBlack()
        {
            var fen = "8/8/8/8/8/8/8/8 b - - 0 0";
            _ = BoardFactory.CreateFromFen(fen, out var isWhite);
            isWhite.ShouldBeFalse();
        }

        [TestMethod]
        public void Castling_KingSideOnly()
        {
            var fen = "8/8/8/8/8/8/8/8 w Kk - 0 0";
            var board = BoardFactory.CreateFromFen(fen, out _);
            
            board.Strategic.WhiteLeftCastlingValid.ShouldBeFalse();
            board.Strategic.WhiteRightCastlingValid.ShouldBeTrue();
            board.Strategic.BlackLeftCastlingValid.ShouldBeFalse();
            board.Strategic.BlackRightCastlingValid.ShouldBeTrue();
        }

        [TestMethod]
        public void AfterFirstMoves_ShouldHaveAllPieces()
        {
            var fen = "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2";
            var board = BoardFactory.CreateFromFen(fen, out _);

            board.PieceList.Count(p => p.Identity == 'P').ShouldBe(16);
        }
    }
}
