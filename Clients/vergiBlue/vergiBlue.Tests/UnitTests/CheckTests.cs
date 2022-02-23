using System;
using System.Collections.Generic;
using System.Linq;
using CommonNetStandard.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace UnitTests
{
    [TestClass]
    public class CheckTests
    {
        [TestMethod]
        public void KingLocationsUpdateOk()
        {
            var white = new King(true, "a1");
            var black = new King(false, "a8");

            var board = BoardFactory.Create();
            board.AddNew(white, black);
            board.Kings = (white, black);

            var move = new SingleMove("a1", "a2");
            //player1.Board.ExecuteMove(move);
            var player1 = LogicFactory.CreateForTest(true, BoardFactory.CreateFromMove(board, move));

            var kingReference = player1.Board.Kings.white;
            kingReference.ShouldNotBeNull();
            if (kingReference == null) throw new ArgumentException();
            kingReference.CurrentPosition.ToAlgebraic().ShouldBe("a2");

            var listReference = player1.Board.PieceList.First(p => p.IsWhite);
            listReference.CurrentPosition.ToAlgebraic().ShouldBe("a2");

            var arrayReference = player1.Board.ValueAtDefinitely("a2".ToTuple());
            arrayReference.CurrentPosition.ToAlgebraic().ShouldBe("a2");
        }
        
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

            var board = BoardFactory.Create();
            board.Shared.GameTurnCount = 20;
            // 
            var rookPositions = new List<string> { "a8", "b7" };
            var asTuples = rookPositions.Select(p => p.ToTuple()).ToList();
            CreateRooks(asTuples, board, true);

            // 
            var king = new King(false, "g8");
            board.AddNew(king);
            board.Kings = (null, king);

            var player = LogicFactory.CreateForTest(true, board);
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

            var board = BoardFactory.Create();
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
            board.InitializeHashing();

            var player = LogicFactory.CreateForTest(true, board);
            var playerMove = player.CreateMoveWithDepth(4);
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
            var player = LogicFactory.CreateWithoutBoardInit(true);

            var opponent = LogicFactory.CreateWithoutBoardInit(false);

            var board = BoardFactory.Create();
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
            board.InitializeHashing();

            player.Board = BoardFactory.CreateClone(board);
            opponent.Board = BoardFactory.CreateClone(board);

            var playerMove = player.CreateMoveWithDepth(4);
            opponent.ReceiveMove(playerMove.Move);

            var opponentMove = opponent.CreateMoveWithDepth(4);
            player.ReceiveMove(opponentMove.Move);


            var playerMove2 = player.CreateMoveWithDepth(4);
            playerMove2.Move.CheckMate.ShouldBe(true);
            Logger.LogMessage($"Test: {nameof(WhiteRooksShouldCheckMateInTwoTurns)}, diagnostics: {playerMove2.Diagnostics}");
        }

        [TestMethod]
        public void KingShouldMoveAwayFromEaten()
        {
            // King can only go to southeast

            //        F
            // 8R     K
            // 7R     P
            // 6
            // 5
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH
            var player = LogicFactory.CreateWithoutBoardInit(true);
            var opponent = LogicFactory.CreateWithoutBoardInit(false);
            opponent.LatestOpponentMove = new MoveImplementation(){Check = true};

            var board = BoardFactory.Create();
            // 
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "f7"),
                new Rook(true, "a7"),
                new Rook(true, "a8")
            };
            
            board.AddNew(pieces);

            // 
            var blackKing = new King(false, "f8");
            board.AddNew(blackKing);

            var whiteKing = new King(true, "a5");
            board.AddNew(whiteKing);

            board.Kings = (whiteKing, blackKing);

            player.Board = BoardFactory.CreateClone(board);
            opponent.Board = BoardFactory.CreateClone(board);

            var opponentMove = opponent.CreateMoveWithDepth(4);
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
            var player = LogicFactory.CreateWithoutBoardInit(true);

            var previousMove = new MoveImplementation()
            {
                StartPosition = "c4",
                EndPosition = "e4"
            };
            player.LatestOpponentMove = previousMove;
            player.GameHistory.Add(previousMove);

            var board = BoardFactory.Create();
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

            player.Board = BoardFactory.CreateClone(board);

            var playerMove = player.CreateMoveWithDepth(3);
            playerMove.Move.EndPosition.ShouldNotBe("f6");
        }

        public void CreateRooks(IEnumerable<(int, int)> coordinateList, IBoard board, bool isWhite)
        {
            foreach (var coordinates in coordinateList)
            {
                var rook = new Rook(isWhite, coordinates);
                board.AddNew(rook);
            }
        }
    }
}
