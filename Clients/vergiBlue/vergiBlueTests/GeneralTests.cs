using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueTests
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void PawnAllowedMoves()
        {
            var board = new Board();
            var pawn1 = new Pawn(true, board);
            pawn1.CurrentPosition = (0, 1);

            var pawn2 = new Pawn(false, board);
            pawn2.CurrentPosition = (1, 2);

            board.AddNew(pawn1);
            board.AddNew(pawn2);

            var moves = pawn1.Moves();
            var coordinates = moves.Select(m => m.ToAlgebraic(m.NewPos));

        }

        [TestMethod]
        public void TestAlgebraicToIntArrayConversions()
        {
            var startCorner = "a1";
            var intArray = Logic.ToTuple(startCorner);
            intArray.ShouldBe((0,0));

            Logic.ToAlgebraic(intArray).ShouldBe("a1");

            var endCorner = "h8";
            intArray = Logic.ToTuple(endCorner);
            intArray.ShouldBe((7, 7));

            Logic.ToAlgebraic(intArray).ShouldBe("h8");


        }
    }
}
