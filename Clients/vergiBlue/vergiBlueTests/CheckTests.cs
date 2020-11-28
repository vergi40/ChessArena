using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Common;
using CommonNetStandard.LocalImplementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Shouldly;
using vergiBlue;
using vergiBlue.Pieces;

namespace vergiBlueTests
{
    [TestClass]
    public class CheckTests
    {
        [TestMethod]
        public void ShouldBeCheckMate()
        {
            // Easy double rook checkmate

            // 8R     K
            // 7 R
            // 6
            // 5
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH
            var player = new Logic(true);
            player.Phase = GamePhase.EndGame;
            player.TurnCount = 20;
            player.SearchDepth = 4;

            var board = new Board();
            // 
            var rookPositions = new List<string> { "a8", "b7" };
            var asTuples = rookPositions.Select(p => p.ToTuple()).ToList();
            CreateRooks(asTuples, board, true);

            // 
            var king = new King(false, "g8");
            board.AddNew(king);
            board.Kings = (null, king);

            player.Board = new Board(board);
            player.Board.IsCheckMate(true, false).ShouldBeTrue();

        }
        [TestMethod]
        public void WhiteRooksShouldCheckMateInOneTurn()
        {
            // Easy double rook checkmate

            // 8      K
            // 7 R
            // 6R
            // 5 
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH
            var player = new Logic(true);
            player.Phase = GamePhase.EndGame;
            player.TurnCount = 20;
            player.SearchDepth = 4;

            var board = new Board();
            // 
            var rookPositions = new List<string> { "a6", "b7" };
            var asTuples = rookPositions.Select(p => p.ToTuple()).ToList();
            CreateRooks(asTuples, board, true);

            // 
            var blackKing = new King(false, "g8");
            board.AddNew(blackKing);

            var whiteKing = new King(true, "a5");
            board.AddNew(whiteKing);

            board.Kings = (whiteKing, blackKing);

            player.Board = new Board(board);

            var playerMove = player.CreateMove();
            playerMove.Move.CheckMate.ShouldBeTrue();
            Logger.LogMessage($"Test: {nameof(WhiteRooksShouldCheckMateInOneTurn)}, diagnostics: {playerMove.Diagnostics}");
        }

        [TestMethod]
        public void WhiteRooksShouldCheckMateInTwoTurns()
        {
            // Easy double rook checkmate
            // There are 4 variations resulting to checkmate in 2 turns

            // 8      K
            // 7
            // 6R
            // 5 R
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH
            var player = new Logic(true);
            player.Phase = GamePhase.EndGame;
            player.TurnCount = 20;
            player.SearchDepth = 4;

            var opponent = new Logic(false);
            opponent.Phase = GamePhase.EndGame;
            opponent.TurnCount = 20;
            opponent.SearchDepth = 4;

            var board = new Board();
            // 
            var rookPositions = new List<string> { "a6", "b5" };
            var asTuples = rookPositions.Select(p => p.ToTuple()).ToList();
            CreateRooks(asTuples, board, true);

            // 
            var blackKing = new King(false, "g8");
            board.AddNew(blackKing);

            var whiteKing = new King(true, "a5");
            board.AddNew(whiteKing);

            board.Kings = (whiteKing, blackKing);

            player.Board = new Board(board);
            opponent.Board = new Board(board);

            var playerMove = player.CreateMove();
            opponent.ReceiveMove(playerMove.Move);

            var opponentMove = opponent.CreateMove();
            player.ReceiveMove(opponentMove.Move);


            var playerMove2 = player.CreateMove();
            playerMove2.Move.CheckMate.ShouldBe(true);
            Logger.LogMessage($"Test: {nameof(WhiteRooksShouldCheckMateInTwoTurns)}, diagnostics: {playerMove2.Diagnostics}");
        }

        [TestMethod]
        public void KingShouldMoveAwayFromEaten()
        {
            // King can only go to southeast

            // 8R     K
            // 7R     P
            // 6
            // 5
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH
            var player = new Logic(true);
            player.Phase = GamePhase.EndGame;
            player.TurnCount = 21;
            player.SearchDepth = 4;

            var opponent = new Logic(false);
            opponent.Phase = GamePhase.EndGame;
            opponent.TurnCount = 21;
            opponent.SearchDepth = 4;
            opponent.LatestOpponentMove = new MoveImplementation(){Check = true};

            var board = new Board();
            // 
            var rookPositions = new List<string> { "a7", "a8" };
            var asTuples = rookPositions.Select(p => p.ToTuple()).ToList();
            CreateRooks(asTuples, board, true);

            var pawn = new Pawn(true, "f7");
            board.AddNew(pawn);

            // 
            var blackKing = new King(false, "f8");
            board.AddNew(blackKing);

            var whiteKing = new King(true, "a5");
            board.AddNew(whiteKing);

            board.Kings = (whiteKing, blackKing);

            player.Board = new Board(board);
            opponent.Board = new Board(board);

            var opponentMove = opponent.CreateMove();
            opponentMove.Move.EndPosition.ShouldBe("g7");
            Logger.LogMessage($"Test: {nameof(KingShouldMoveAwayFromEaten)}, diagnostics: {opponentMove.Diagnostics}");
        }

        /// <summary>
        /// Game situations/King moved to be captured.png
        /// </summary>
        [TestMethod]
        public void KingShouldNotMoveToBeCaptured()
        {
            // In local game king moved to f6 and was captured

            // 8    | K
            // 7   N|
            // 6    |
            // 5 PP |  K
            // 4  > |RN
            // 3    |
            // 2   R|
            // 1    |
            //  ABCD EFGH
            var player = new Logic(true);
            player.Phase = GamePhase.EndGame;
            player.TurnCount = 20;
            player.SearchDepth = 3;

            var previousMove = new MoveImplementation()
            {
                StartPosition = "c4",
                EndPosition = "e4"
            };
            player.LatestOpponentMove = previousMove;
            player.GameHistory.Add(previousMove);

            var board = new Board();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b5"),
                new Pawn(false, "c5"),
                new Rook(true, "d2"),
                new Rook(false, "e4"),
                new Knight(true, "f4"),
                new Knight(false, "d7")
            };
            board.AddNew(pieces);
            // 
            var blackKing = new King(false, "f8");
            board.AddNew(blackKing);

            var whiteKing = new King(true, "g5");
            board.AddNew(whiteKing);

            board.Kings = (whiteKing, blackKing);

            player.Board = new Board(board);

            var playerMove = player.CreateMove();
            playerMove.Move.EndPosition.ShouldNotBe("f6");
        }

        public void CreateRooks(IEnumerable<(int, int)> coordinateList, Board board, bool isWhite)
        {
            foreach (var coordinates in coordinateList)
            {
                var rook = new Rook(isWhite, coordinates);
                board.AddNew(rook);
            }
        }
    }
}
