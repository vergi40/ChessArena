using System;
using System.Collections.Generic;
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
            var firstMoveHash = board.Shared.Transpositions.GetNewBoardHash(firstPawnMove, board, board.BoardHash);
            
            board.ExecuteMove(firstPawnMove);
            firstMoveHash.ShouldBe(board.BoardHash);

            //
            var second = new SingleMove("e7", "e5");
            var secondMoveHash = board.Shared.Transpositions.GetNewBoardHash(second, board, board.BoardHash);

            board.ExecuteMove(second);
            secondMoveHash.ShouldBe(board.BoardHash);

            //
            var capture = new SingleMove("d4", "e5", true);
            var captureMoveHash = board.Shared.Transpositions.GetNewBoardHash(capture, board, board.BoardHash);

            board.ExecuteMove(capture);
            captureMoveHash.ShouldBe(board.BoardHash);
        }

        [TestMethod]
        public void Transpositions_Depth1()
        {
            var searchDepth = 1;
            var player = new Logic(false, BenchMarking.CreateRuyLopezOpeningBoard());

            player.Settings = new LogicSettings()
            {
                UseParallelComputation = false,
                UseTranspositionTables = true,
                UseIterativeDeepening = false,
                UseFullDiagnostics = true
            };

            var playerMove = player.CreateMoveWithDepth(searchDepth);
            var diagnostics = playerMove.Diagnostics;
            Logger.LogMessage($"// Test: {nameof(Transpositions_Depth1)}. Move: {playerMove.Move.StartPosition} to {playerMove.Move.EndPosition}. Depth {searchDepth}");
            Logger.LogMessage($"// {diagnostics.ToString()}");

            // Test: Transpositions_Depth1. Move: c6 to a5. Depth 1
            // Board evaluations: 962. Check evaluations: 961. Time elapsed: 21 ms. Available moves found: 30. Transposition tables saved: 962
        }

        [TestMethod]
        public void Transpositions_BoardHashTests()
        {
            // 8
            // 7
            // 6  K
            // 5
            // 4P
            // 3  K
            // 2
            // 1
            //  ABCDEFGH
            
            // Obvious board.
            // If white starts, pawn is sure to be lost

            var whiteBoard = new Board();
            var pieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "c3"),
                new King(false, "c6")
            };
            whiteBoard.AddNew(pieces);

            var white = new Logic(true, whiteBoard);

            white.Settings = new LogicSettings()
            {
                UseParallelComputation = false,
                UseTranspositionTables = true,
                UseIterativeDeepening = false,
                UseFullDiagnostics = true
            };
            
            // TODO to be implemented
            //var move = white.CreateMoveWithDepth(depth);
            white.Board.ExecuteMove(new SingleMove("c3", "b4"));

            // Calculate optimal move hash beforehand and compare to choice made
            var expectedBoard = new Board();
            var expectedPieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "b4"),
                new King(false, "c6")
            };
            expectedBoard.AddNew(expectedPieces);
            
            // Refresh hash
            expectedBoard = new Board(expectedBoard);
            
            white.Board.BoardHash.ShouldBe(expectedBoard.BoardHash);

            //var diagnostics = move.Diagnostics;
            //Logger.LogMessage($"// Test: {nameof(Transpositions_BoardHashTests)}. Move: {move.Move.StartPosition} to {move.Move.EndPosition}. Depth {depth}");
            //Logger.LogMessage($"// {diagnostics.ToString()}");

            // Test: Transpositions_BoardHashTests. Move: c3 to c4. Depth 5
            // Board evaluations: 3814. Check evaluations: 72. Alpha cutoffs: 854. Beta cutoffs: 683. Priority moves found: 2900.
            // Transpositions used: 3307. Time elapsed: 35 ms. Available moves found: 7. Transposition tables saved: 1294


        }

        [TestMethod]
        public void Transposition_DifferentMoves_SameHash()
        {
            // 8   K
            // 7   
            // 6    
            // 5
            // 4P
            // 3   K
            // 2
            // 1
            //  ABCDEFGH

            var board1 = new Board();
            var board2 = new Board();
            var pieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "d3"),
                new King(false, "d8")
            };
            board1.AddNew(pieces);
            board2.AddNew(pieces);

            var move1a = new SingleMove("d3", "c4");
            board1.ExecuteMove(move1a);
            var move2a = new SingleMove("c4", "d5");
            board1.ExecuteMove(move2a);

            var move1b = new SingleMove("d3", "d4");
            board2.ExecuteMove(move1b);
            var move2b = new SingleMove("d4", "d5");
            board2.ExecuteMove(move2b);
            
            board1.BoardHash.ShouldBe(board2.BoardHash);

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
