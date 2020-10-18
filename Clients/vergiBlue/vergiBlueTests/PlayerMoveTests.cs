using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Shouldly;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueTests
{
    [TestClass]
    public class PlayerMoveTests
    {
        /// <summary>
        /// Create setup where each player has 3 pawns on start row and 1 pawn agaist each other
        /// </summary>
        /// <param name="isPlayerWhite"></param>
        /// <returns></returns>
        private Board CreateMockPawnSetup(bool isPlayerWhite)
        {
            var board = new Board();

            // Lonely pawns, not very high eval
            for (int i = 1; i < 4; i++)
            {
                var whitePawn = new Pawn(!isPlayerWhite, true, board);
                whitePawn.CurrentPosition = (i, 1);
                board.AddNew(whitePawn);
            }

            // e4
            var whiteBattlePawn = new Pawn(!isPlayerWhite, true, board);
            whiteBattlePawn.CurrentPosition = Logic.ToTuple("e4");
            board.AddNew(whiteBattlePawn);

            // Diagonal relation (northwest)

            // f5
            var blackBattlePawn = new Pawn(isPlayerWhite, false, board);
            blackBattlePawn.CurrentPosition = Logic.ToTuple("f5");
            board.AddNew(blackBattlePawn);

            // Random opponent pawns to confuse
            for (int i = 1; i < 4; i++)
            {
                var blackPawn = new Pawn(isPlayerWhite, false, board);
                blackPawn.CurrentPosition = (i, 6);
                board.AddNew(blackPawn);
            }

            return board;
        }

        private Board CreateMockPawnRookSetup(bool isPlayerWhite)
        {
            var board = CreateMockPawnSetup(isPlayerWhite);
            var whiteRook = new Rook(!isPlayerWhite, true, board);
            whiteRook.CurrentPosition = (0, 0);
            board.AddNew(whiteRook);

            var blackRook = new Rook(isPlayerWhite, false, board);
            blackRook.CurrentPosition = (0, 7);
            board.AddNew(blackRook);

            return board;
        }

        [TestMethod]
        public void PlayerWhitePawnShouldEatOpponent()
        {
            var logic = new Logic(true);
            logic.Board = CreateMockPawnSetup(true);
            var playerMove = logic.CreateMove();

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("f5");
        }

        [TestMethod]
        public void PlayerBlackPawnShouldEatOpponent()
        {
            var logic = new Logic(false);
            logic.Board = CreateMockPawnSetup(false);
            var playerMove = logic.CreateMove();

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("e4");
        }

        [TestMethod]
        public void PlayerWhiteRookShouldEatOpponentRook()
        {
            var logic = new Logic(true);
            logic.Board = CreateMockPawnRookSetup(true);
            var playerMove = logic.CreateMove();

            // Let's see if the best move selected
            // Diagnostics: time elapsed 12ms
            playerMove.Move.EndPosition.ShouldBe("a8");
            Logger.LogMessage($"Test: {nameof(PlayerWhiteRookShouldEatOpponentRook)}, diagnostics: {playerMove.Diagnostics}");
        }

        [TestMethod]
        public void PlayerBlackRookShouldEatOpponentRook()
        {
            var logic = new Logic(false);
            logic.Board = CreateMockPawnRookSetup(false);
            var playerMove = logic.CreateMove();

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("a1");
        }
    }
}
