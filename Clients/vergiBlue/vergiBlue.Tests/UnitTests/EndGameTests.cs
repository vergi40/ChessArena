using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.Algorithms;
using vergiBlue.Algorithms.Basic;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace UnitTests
{
    [TestClass]
    public class EndGameTests
    {
        /// <summary>
        /// Benchmarking checkmate calculation count and what AI would do in the situation
        /// See Game situations/Bishop ending.png
        /// </summary>
        [TestMethod]
        public void BishopEnding()
        {
            // https://en.wikipedia.org/wiki/Chess_endgame
            // Bishop and pawn endings
            // 

            // The adjacent diagram, from Molnar–Nagy, Hungary 1966, illustrates the concepts of good bishop versus bad bishop,
            // opposition, zugzwang, and outside passed pawn.
            // White wins with 1. e6! (vacating e5 for his king)
            // 1... Bxe6 2. Bc2! (threatening Bxg6)
            // 2... Bf7 3. Be4! (threatening Bxc6)
            // 3... Be8 4. Ke5! (seizing the opposition [i.e. the kings are two orthogonal squares apart, with the other player on move]
            // and placing Black in zugzwang—he must either move his king, allowing White's king to penetrate, or his bishop, allowing a
            // decisive incursion by White's bishop)
            // 4... Bd7 5. Bxg6!
         
            
            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "b4"),
                new Pawn(true, "c5"),
                new Pawn(true, "e5"),
                new Pawn(true, "g5"),
                new Pawn(true, "h6"),
                new Pawn(false, "b5"),
                new Pawn(false, "c6"),
                new Pawn(false, "g6"),
                new Pawn(false, "h7"),

                new Bishop(true, "b3"),
                new Bishop(false, "f7"),
                new King(true, "f4"),
                new King(false, "e7")
            };
            board.AddNew(pieces);
            var player = LogicFactory.CreateForTest(true, board);
            var opponent = LogicFactory.CreateForTest(false, board);
            
            var playerMove = player.CreateMoveWithDepth(8);
            playerMove.Move.EndPosition.ShouldBe("e6");

        }

        private IBoard CreateBoard_QueensAndKings()
        {
            // Occured when playing around. For some reason black wouldn't capture white queen
            // Test with different settings

            var pieces = new List<PieceBase>
            {
                new Queen(true, "d4"),
                new Queen(false, "a1"),
                new King(true, "f6"),
                new King(false, "f3"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);
            return board;
        }

        [TestMethod]
        public void QueensAndKings_ShouldCaptureQueen()
        {
            var board = CreateBoard_QueensAndKings();

            var ai = LogicFactory.CreateForTest(false, board);
            ai.Settings.UseTranspositionTables = false;
            ai.Settings.UseIterativeDeepening = false;

            var move = ai.CreateMoveWithDepth(5);
            move.Move.EndPosition.ShouldBe("d4");
        }

        [TestMethod]
        public void QueensAndKings_ID_ShouldCaptureQueen()
        {
            var board = CreateBoard_QueensAndKings();

            var ai = LogicFactory.CreateForTest(false, board);
            ai.Settings.UseIterativeDeepening = false;

            var move = ai.CreateMoveWithDepth(5);
            move.Move.EndPosition.ShouldBe("d4");
        }

        [TestMethod]
        public void QueensAndKings_Transpositions_ShouldCaptureQueen()
        {
            var board = CreateBoard_QueensAndKings();

            var ai = LogicFactory.CreateForTest(false, board);
            ai.Settings.UseTranspositionTables = false;

            var move = ai.CreateMoveWithDepth(5);
            move.Move.EndPosition.ShouldBe("d4");
        }

        [TestMethod]
        public void QueensAndKings_ID_Transpositions_ShouldCaptureQueen()
        {
            var board = CreateBoard_QueensAndKings();

            var ai = LogicFactory.CreateForTest(false, board);

            var move = ai.CreateMoveWithDepth(5);
            move.Move.EndPosition.ShouldBe("d4");
        }
        
        
        [TestMethod]
        public void EvaluationResult_AlreadyCheckMate_ReturnBestForMaximizing()
        {
            // Start situation
            // 8r      k  
            // 7 r 
            // 6      
            // 5    
            // 4
            // 3K   
            // 2
            // 1       
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(false, "a3"),
                new King(true, "h8"),
                new Rook(true, "b7"),
                new Rook(true, "a8"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var moves = board.MoveGenerator.MovesQuick(true, false).ToList();
            var orderGuess = MoveOrdering.DebugSortMovesByGuessWeight(moves, board, true);
            var orderEval = MoveOrdering.DebugSortMovesByEvaluation(moves, board, true);

            var eval = new EvaluationResult();
            eval.Add(orderEval);

            var expected = new SingleMove("a8", "a3");
            eval.Best(true).EqualPositions(expected).ShouldBeTrue();
        }

        [TestMethod]
        public void EvaluationResult_AlreadyCheckMate_ReturnBestForMinimizing()
        {
            // Start situation
            // 8r      k  
            // 7 r 
            // 6      
            // 5    
            // 4
            // 3K   
            // 2
            // 1       
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "a3"),
                new King(false, "h8"),
                new Rook(false, "b7"),
                new Rook(false, "a8"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var moves = board.MoveGenerator.MovesQuick(false, false).ToList();
            var orderGuess = MoveOrdering.DebugSortMovesByGuessWeight(moves, board, false);
            var orderEval = MoveOrdering.DebugSortMovesByEvaluation(moves, board, false);

            var eval = new EvaluationResult();
            eval.Add(orderEval);

            var expected = new SingleMove("a8", "a3");
            eval.Best(false).EqualPositions(expected).ShouldBeTrue();
        }
    }
}
