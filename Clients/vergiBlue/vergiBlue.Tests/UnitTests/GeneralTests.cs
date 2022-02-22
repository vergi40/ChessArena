using System.Collections.Generic;
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
        public void InitializeBoardHash()
        {
            var transposition = new TranspositionTables();
            transposition.Initialize();

            var board = BoardFactory.Create();
            board.InitializeDefaultBoard();

            var hash = board.BoardHash;
            // 121398
        }
        
        [TestMethod]
        public void Transposition_ExecuteMove_Tests()
        {
            var transposition = new TranspositionTables();
            transposition.Initialize();

            var board = BoardFactory.Create();
            board.InitializeDefaultBoard();

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
            var player = LogicFactory.CreateForTest(false, BenchMarking.CreateRuyLopezOpeningBoard());

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

            var whiteBoard = BoardFactory.Create();
            var pieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "c3"),
                new King(false, "c6")
            };
            whiteBoard.AddNew(pieces);

            var white = LogicFactory.CreateForTest(true, whiteBoard);

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
            var expectedBoard = BoardFactory.Create();
            var expectedPieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "b4"),
                new King(false, "c6")
            };
            expectedBoard.AddNew(expectedPieces);
            
            // Refresh hash
            expectedBoard = BoardFactory.CreateClone(expectedBoard);
            
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

            var board1 = BoardFactory.Create();
            var board2 = BoardFactory.Create();
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
        public void PawnWhite_EnPassant()
        {
            var board = BoardFactory.Create();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b5"),
                new Pawn(false, "c7"),
            };

            board.AddNew(pieces);
            board.ExecuteMove(new SingleMove("c7", "c5"));
            board.Strategic.EnPassantPossibility.ShouldBe("c6".ToTuple());

            var boardMoves = board.MoveGenerator.MovesQuick(true, false);

            var expected = new SingleMove("b5", "c6", true);

            boardMoves.ShouldContain(m => m.EqualPositions(expected));
        }

        [TestMethod]
        public void PawnBlack_EnPassant()
        {
            var board = BoardFactory.Create();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b2"),
                new Pawn(false, "c4"),
            };

            board.AddNew(pieces);
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
    }
}
