using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class TranspositionTablesTests
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

            var pieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "c3"),
                new King(false, "c6")
            };
            var whiteBoard = BoardFactory.CreateFromPieces(pieces);

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
            var expectedPieces = new List<PieceBase>
            {
                new Pawn(false, "a4"),
                new King(true, "b4"),
                new King(false, "c6")
            };
            var expectedBoard = BoardFactory.CreateFromPieces(expectedPieces);
            expectedBoard.BoardHash = expectedBoard.Shared.Transpositions.ChangeSideToMove(expectedBoard.BoardHash);

            // Refresh hash
            //expectedBoard = BoardFactory.CreateClone(expectedBoard);

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

            board1.ExecuteMove(new SingleMove("d3", "c4"));
            board1.ExecuteMove(new SingleMove("c4", "d5"));
            board2.ExecuteMove(new SingleMove("d3", "d4"));
            board2.ExecuteMove(new SingleMove("d4", "d5"));

            board1.BoardHash.ShouldBe(board2.BoardHash);
        }

        [TestMethod]
        public void Castling_AsOneMove_OrSeparated_SameHash()
        {
            // 8    K
            // 7   
            // 6    
            // 5
            // 4
            // 3   
            // 2
            // 1    K  R
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "e1"),
                new King(false, "e8"),
                new Rook(true, "h1"),
            };

            var board1 = BoardFactory.CreateFromPieces(pieces);
            var board2 = BoardFactory.CreateClone(board1);

            var castling = new SingleMove("e1", "g1")
            {
                Castling = true
            };
            board1.ExecuteMove(castling);

            // Move pieces one by one in board2
            var move1 = new SingleMove("e1", "f2");
            var move2 = new SingleMove("h1", "f1");
            var move3 = new SingleMove("f2", "g1");

            board2.ExecuteMove(move1);
            board2.ExecuteMove(move2);
            board2.ExecuteMove(move3);

            board1.BoardHash.ShouldBe(board2.BoardHash);
        }

        [TestMethod]
        public void Promotions_SameHash()
        {
            // Start situation
            // 8    
            // 7P P P P   
            // 6    
            // 5       K
            // 4
            // 3   
            // 2
            // 1       K
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "h1"),
                new King(false, "h5"),
                new Pawn(true, "a7"),
                new Pawn(true, "c7"),
                new Pawn(true, "e7"),
                new Pawn(true, "g7"),
            };
            var baseline = BoardFactory.CreateFromPieces(pieces);


            // End
            // 8Q R N B    
            // 7   
            // 6    
            // 5       K
            // 4
            // 3   
            // 2
            // 1       K
            //  ABCDEFGH
            var pieces2 = new List<PieceBase>
            {
                new King(true, "h1"),
                new King(false, "h5"),
                new Queen(true, "a8"),
                new Rook(true, "c8"),
                new Knight(true, "e8"),
                new Bishop(true, "g8"),
            };
            var result = BoardFactory.CreateFromPieces(pieces2);

            baseline.ExecuteMove(new SingleMove("a7", "a8"){PromotionType = PromotionPieceType.Queen});
            baseline.ExecuteMove(new SingleMove("c7", "c8"){PromotionType = PromotionPieceType.Rook});
            baseline.ExecuteMove(new SingleMove("e7", "e8"){PromotionType = PromotionPieceType.Knight});
            baseline.ExecuteMove(new SingleMove("g7", "g8"){PromotionType = PromotionPieceType.Bishop});

            baseline.BoardHash.ShouldBe(result.BoardHash);
        }

        [TestMethod]
        public void EnPassant_SameHash()
        {
            // Start situation
            // 8    
            // 7 e
            // 6Pp  
            // 5       K
            // 4
            // 3   
            // 2
            // 1       K
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "h1"),
                new King(false, "h5"),
                new Pawn(true, "a6"),
                new Pawn(false, "b6"),
            };
            var baseline = BoardFactory.CreateFromPieces(pieces);
            baseline.Strategic.EnPassantPossibility = "b7".ToTuple();

            var pieces2 = new List<PieceBase>
            {
                new King(true, "h1"),
                new King(false, "h5"),
                new Pawn(true, "b7"),
            };
            var result = BoardFactory.CreateFromPieces(pieces2);
            result.BoardHash = result.Shared.Transpositions.ChangeSideToMove(result.BoardHash);

            baseline.ExecuteMove(new SingleMove("a6", "b7", true) { EnPassant = true });

            baseline.BoardHash.ShouldBe(result.BoardHash);
        }
    }
}
