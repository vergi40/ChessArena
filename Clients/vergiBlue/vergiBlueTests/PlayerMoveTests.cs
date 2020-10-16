using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;

namespace vergiBlueTests
{
    [TestClass]
    public class PlayerMoveTests
    {
        [TestMethod]
        public void PawnAllowedMoves()
        {
            var board = new Board();

            // Lonely pawns, not very high eval
            for (int i = 1; i < 4; i++)
            {
                var lonelyPawn = new Pawn(false, true, board);
                lonelyPawn.CurrentPosition = (i, 1);
                board.AddNew(lonelyPawn);
            }
            
            var thePawn = new Pawn(false, true, board);
            thePawn.CurrentPosition = (5, 3);
            board.AddNew(thePawn);

            var toBeEaten = new Pawn(true, false, board);
            toBeEaten.CurrentPosition = (4, 4);
            board.AddNew(toBeEaten);
            var target = Logic.ToAlgebraic(toBeEaten.CurrentPosition);

            // Random opponent pawns to confuse
            for (int i = 1; i < 4; i++)
            {
                var opponentPawn = new Pawn(true, false, board);
                opponentPawn.CurrentPosition = (i, 6);
                board.AddNew(opponentPawn);
            }

            var logic = new Logic();
            logic.Board = board;
            var playerMove = logic.CreateMove();

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("e5");
        }
    }
}
