using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <returns></returns>
        private Board CreateMockPawnSetup()
        {
            var board = new Board();

            // Lonely pawns, not very high eval
            for (int i = 1; i < 4; i++)
            {
                var whitePawn = new Pawn(true, (i, 1));
                board.AddNew(whitePawn);
            }

            // e4
            var whiteBattlePawn = new Pawn(true, "e4");
            board.AddNew(whiteBattlePawn);

            // Diagonal relation (northwest)

            // f5
            var blackBattlePawn = new Pawn(false, "f5");
            board.AddNew(blackBattlePawn);

            // Random opponent pawns to confuse
            for (int i = 1; i < 4; i++)
            {
                var blackPawn = new Pawn(false, (i, 6));
                board.AddNew(blackPawn);
            }

            return board;
        }

        private Board CreateMockPawnRookSetup()
        {
            var board = CreateMockPawnSetup();
            var whiteRook = new Rook(true, (0, 0));
            board.AddNew(whiteRook);

            var blackRook = new Rook(false, (0, 7));
            board.AddNew(blackRook);

            return board;
        }

        [TestMethod]
        public void PlayerWhitePawnShouldEatOpponent()
        {
            var logic = new Logic(true);
            logic.Board = CreateMockPawnSetup();
            var playerMove = logic.CreateMoveWithDepth(1);

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("f5");
        }

        [TestMethod]
        public void PlayerBlackPawnShouldEatOpponent()
        {
            var logic = new Logic(false);
            logic.Board = CreateMockPawnSetup();
            var playerMove = logic.CreateMoveWithDepth(1);

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("e4");
        }

        [TestMethod]
        public void PlayerWhiteRookShouldEatOpponentRook()
        {
            var logic = new Logic(true);
            logic.Board = CreateMockPawnRookSetup();
            var playerMove = logic.CreateMoveWithDepth(2);

            // Let's see if the best move selected
            // Diagnostics: time elapsed 12ms
            playerMove.Move.EndPosition.ShouldBe("a8");
            Logger.LogMessage($"Test: {nameof(PlayerWhiteRookShouldEatOpponentRook)}, diagnostics: {playerMove.Diagnostics}");
        }

        [TestMethod]
        public void PlayerBlackRookShouldEatOpponentRook()
        {
            var logic = new Logic(false);
            logic.Board = CreateMockPawnRookSetup();
            var playerMove = logic.CreateMoveWithDepth(1);

            // Let's see if the best move selected
            playerMove.Move.EndPosition.ShouldBe("a1");
        }

        [TestMethod]
        public void PlayerWhiteRookShouldEatPawnNotDefended()
        {
            // Requires to calculate more than next move to spot

            // 8   P
            // 7  P P
            // 6 P   P
            // 5P  R  P
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH
            var logic = new Logic(true);
            logic.Board = new Board();

            // Pawns
            var pawnPositions = new List<string> { "a5", "b6", "c7", "d8", "e7", "f6", "g5" };
            var asTuples = pawnPositions.Select(p => p.ToTuple()).ToList();
            CreatePawns(asTuples, logic.Board, false);

            // 
            var whiteRook = new Rook(true, "d5");
            logic.Board.AddNew(whiteRook);

            var playerMove = logic.CreateMoveWithDepth(3);

            // Let's see if the best move selected
            // Seems like all d columns are equally good in depth 3
            playerMove.Move.EndPosition.ShouldContain('d');
            Logger.LogMessage($"Test: {nameof(PlayerWhiteRookShouldEatPawnNotDefended)}, diagnostics: {playerMove.Diagnostics}");
        }

        public void CreatePawns(IEnumerable<(int,int)> coordinateList, Board board, bool isWhite)
        {
            foreach(var coordinates in coordinateList)
            {
                var pawn = new Pawn(isWhite, coordinates);
                board.AddNew(pawn);
            }
        }
    }
}
