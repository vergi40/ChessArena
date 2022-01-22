using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace vergiBlueTests
{
    [TestClass]
    public class EndGameTests
    {
        /// <summary>
        /// Benchmarking checkmate calculation count and what AI would do in the situation
        /// See Game situations/Bishop ending.png
        /// </summary>
        [TestMethod]
        public void BishopEnding()
        {
            // https://en.wikipedia.org/wiki/Chess_endgame
            // Bishop and pawn endings
            // 

            // The adjacent diagram, from Molnar–Nagy, Hungary 1966, illustrates the concepts of good bishop versus bad bishop,
            // opposition, zugzwang, and outside passed pawn.
            // White wins with 1. e6! (vacating e5 for his king)
            // 1... Bxe6 2. Bc2! (threatening Bxg6)
            // 2... Bf7 3. Be4! (threatening Bxc6)
            // 3... Be8 4. Ke5! (seizing the opposition [i.e. the kings are two orthogonal squares apart, with the other player on move]
            // and placing Black in zugzwang—he must either move his king, allowing White's king to penetrate, or his bishop, allowing a
            // decisive incursion by White's bishop)
            // 4... Bd7 5. Bxg6!
         
            
            var board = new Board();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b4"),
                new Pawn(true, "c5"),
                new Pawn(true, "e5"),
                new Pawn(true, "g5"),
                new Pawn(true, "h6"),
                new Pawn(false, "b5"),
                new Pawn(false, "c6"),
                new Pawn(false, "g6"),
                new Pawn(false, "h7"),

                new Bishop(true, "b3"),
                new Bishop(false, "f7"),
                new King(true, "f4"),
                new King(false, "e7")
            };
            board.AddNew(pieces);
            var player = LogicFactory.CreateForTest(true, board);
            var opponent = LogicFactory.CreateForTest(false, board);
            
            var playerMove = player.CreateMoveWithDepth(8);
            playerMove.Move.EndPosition.ShouldBe("e6");

        }
    }
}
