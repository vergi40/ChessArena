﻿using System.Collections.Generic;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Shouldly;
using vergiBlue;
using vergiBlue.Algorithms;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace UnitTests
{
    [TestClass]
    public class GeneralTests
    {
        
        
        [TestMethod]
        public void PawnWhite_EnPassant()
        {
            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b5"),
                new Pawn(false, "c7"),
            };

            board.AddNew(pieces);
            board.Shared.Testing = true;
            board.ExecuteMove(new SingleMove("c7", "c5"));
            board.Strategic.EnPassantPossibility.ShouldBe("c6".ToTuple());

            var boardMoves = board.MoveGenerator.MovesQuick(true, false);

            var expected = new SingleMove("b5", "c6", true);

            boardMoves.ShouldContain(m => m.EqualPositions(expected));
        }

        [TestMethod]
        public void PawnBlack_EnPassant()
        {
            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b2"),
                new Pawn(false, "c4"),
            };

            board.AddNew(pieces);
            board.Shared.Testing = true;
            board.ExecuteMove(new SingleMove("b2", "b4"));
            board.Strategic.EnPassantPossibility.ShouldBe("b3".ToTuple());

            var boardMoves = board.MoveGenerator.MovesQuick(false, false);

            var expected = new SingleMove("c4", "b3", true);

            boardMoves.ShouldContain(m => m.EqualPositions(expected));
        }

        [TestMethod]
        public void PawnWhite_Promotion()
        {
            var board = BoardFactory.CreateFromFen("n1n5/PPPk4/8/8/1Pp5/8/4Kppp/5N1N b - b3 0 1", out _);

            var promotion = new SingleMove("b7", "a8", true, PromotionPieceType.Queen);
            board.ExecuteMove(promotion);

            board.PieceList.ShouldContain(p => p.Identity == 'Q');
        }

        [TestMethod]
        public void PawnBlack_Promotion()
        {
            var board = BoardFactory.CreateFromFen("n1n5/PPPk4/8/8/1Pp5/8/4Kppp/5N1N b - b3 0 1", out _);

            var promotion = new SingleMove("g2", "h1", true, PromotionPieceType.Queen);
            board.ExecuteMove(promotion);

            board.PieceList.ShouldContain(p => p.Identity == 'Q');
        }

        [TestMethod]
        public void PawnBlack_Promotion_FromInterface()
        {
            var board = BoardFactory.CreateFromFen("n1n5/PPPk4/8/8/1Pp5/8/4Kppp/5N1N b - b3 0 1", out _);
            var logic = new Logic(true, board, 2);


            var promotion = new SingleMove("g2", "h1", true, PromotionPieceType.Queen);
            logic.ReceiveMove(promotion.ToInterfaceMove());
            logic.Board.PieceList.ShouldContain(p => p.Identity == 'Q');

            Should.NotThrow(() =>
            {
                var move = logic.CreateMove();
            });
        }

        [TestMethod]
        public void TestAlgebraicToIntArrayConversions()
        {
            var startCorner = "a1";
            var intArray = startCorner.ToTuple();
            intArray.ShouldBe((0,0));

            intArray.ToAlgebraic().ShouldBe("a1");

            var endCorner = "h8";
            intArray = endCorner.ToTuple();
            intArray.ShouldBe((7, 7));

            intArray.ToAlgebraic().ShouldBe("h8");


        }

        [TestMethod]
        public void ReadUciPosition_Fen()
        {
            var fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            var command1 = "position fen r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            var command2 = "position fen r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1 moves";

            var result1 = InputSupport.ReadUciPosition(command1);
            var result2 = InputSupport.ReadUciPosition(command2);

            result1.startPosOrFenBoard.ShouldBe(fen);
            result2.startPosOrFenBoard.ShouldBe(fen);

            result1.moves.ShouldBeEmpty();
            result2.moves.ShouldBeEmpty();
        }
    }
}
