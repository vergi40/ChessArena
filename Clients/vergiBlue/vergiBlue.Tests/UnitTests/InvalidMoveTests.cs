using System;
using CommonNetStandard.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;

namespace vergiBlueTests
{
    [TestClass]
    public class InvalidMoveTests
    {
        [TestMethod]
        public void MoveOutside_Pawn_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((7, 1), (8, 2));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void MoveOutside_WhiteRook_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((0, 0), (0, -1));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void MoveOutside_BlackRook_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((0, 7), (0, 8));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void MoveOnTopOfOwn_WhiteBishop_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((2, 0), (3, 1));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void MoveOnTopOfOwn_BlackBishop_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((2, 7), (3, 6));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void InvalidMoveThroughPiece_Rook_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((0, 0), (0, 2));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void InvalidMoveThroughPiece_Bishop_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((2, 0), (4, 2));
                board.ExecuteMoveWithValidation(move);
            });
        }

        [TestMethod]
        public void InvalidMoveThroughPiece_Queen_Throw()
        {
            Should.Throw<InvalidMoveException>(() =>
            {
                var board = BoardFactory.CreateDefault();

                var move = new SingleMove((3, 0), (3, 2));
                board.ExecuteMoveWithValidation(move);
            });
        }
    }
}
