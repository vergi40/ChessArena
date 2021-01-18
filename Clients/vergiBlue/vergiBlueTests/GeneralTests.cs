using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Shouldly;
using vergiBlue;
using vergiBlue.Algorithms;
using vergiBlue.Pieces;

namespace vergiBlueTests
{
    [TestClass]
    public class GeneralTests
    {
        [TestMethod]
        public void InitializeBoardHash()
        {
            var transposition = new TranspositionTables();
            transposition.Initialize();

            var board = new Board();
            board.InitializeEmptyBoard();

            var hash = board.BoardHash;
            // 121398
        }
        
        [TestMethod]
        public void Transposition_ExecuteMove_Tests()
        {
            var transposition = new TranspositionTables();
            transposition.Initialize();

            var board = new Board();
            board.InitializeEmptyBoard();

            // 
            var firstPawnMove = new SingleMove("d2", "d4");
            var firstMoveHash = board.SharedData.Transpositions.GetNewBoardHash(firstPawnMove, board, board.BoardHash);
            
            board.ExecuteMove(firstPawnMove);
            firstMoveHash.ShouldBe(board.BoardHash);

            //
            var second = new SingleMove("e7", "e5");
            var secondMoveHash = board.SharedData.Transpositions.GetNewBoardHash(second, board, board.BoardHash);

            board.ExecuteMove(second);
            secondMoveHash.ShouldBe(board.BoardHash);

            //
            var capture = new SingleMove("d4", "e5", true);
            var captureMoveHash = board.SharedData.Transpositions.GetNewBoardHash(capture, board, board.BoardHash);

            board.ExecuteMove(capture);
            captureMoveHash.ShouldBe(board.BoardHash);
        }

        [TestMethod]
        public void Transpositions_Depth1()
        {
            var searchDepth = 1;
            var player = new Logic(false);
            player.Board = new Board(BenchMarking.CreateRuyLopezOpeningBoard());

            var playerMove = player.CreateMoveWithDepth(searchDepth);
            var diagnostics = playerMove.Diagnostics;
            Logger.LogMessage($"// Test: {nameof(Transpositions_Depth1)}. Move: {playerMove.Move.StartPosition} to {playerMove.Move.EndPosition}. Depth {searchDepth}");
            Logger.LogMessage($"// {diagnostics.ToString()}");

            // Test: Transpositions_Depth1. Move: c6 to a5. Depth 1
            // Board evaluations: 962. Check evaluations: 961. Time elapsed: 21 ms. Available moves found: 30. Transposition tables saved: 962
        }

        [TestMethod]
        public void PawnAllowedMoves()
        {
            var board = new Board();
            var pawn1 = new Pawn(true, (0, 1));

            var pawn2 = new Pawn(false, (1, 2));

            board.AddNew(pawn1);
            board.AddNew(pawn2);

            var moves = pawn1.Moves(board);
            var coordinates = moves.Select(m => m.NewPos.ToAlgebraic());

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
    }
}
