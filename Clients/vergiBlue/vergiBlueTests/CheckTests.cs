using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
            var king = new King(false, board);
            king.CurrentPosition = "g8".ToTuple();
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

            var opponent = new Logic(false);
            opponent.Phase = GamePhase.EndGame;
            opponent.TurnCount = 20;
            opponent.SearchDepth = 4;

            var board = new Board();
            // 
            var rookPositions = new List<string> { "a6", "b7" };
            var asTuples = rookPositions.Select(p => p.ToTuple()).ToList();
            CreateRooks(asTuples, board, true);

            // 
            var blackKing = new King(false, board);
            blackKing.CurrentPosition = "g8".ToTuple();
            board.AddNew(blackKing);

            var whiteKing = new King(true, board);
            whiteKing.CurrentPosition = "a5".ToTuple();
            board.AddNew(whiteKing);

            board.Kings = (whiteKing, blackKing);

            player.Board = new Board(board);
            opponent.Board = new Board(board);

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
            var blackKing = new King(false, board);
            blackKing.CurrentPosition = "g8".ToTuple();
            board.AddNew(blackKing);

            var whiteKing = new King(true, board);
            whiteKing.CurrentPosition = "a5".ToTuple();
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

        public void CreateRooks(IEnumerable<(int, int)> coordinateList, Board board, bool isWhite)
        {
            foreach (var coordinates in coordinateList)
            {
                var pawn = new Rook(isWhite, board);
                pawn.CurrentPosition = coordinates;
                board.AddNew(pawn);
            }
        }
    }
}
