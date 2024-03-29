﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue.BoardModel;
using vergiBlue.Pieces;

namespace UnitTests.MoveGeneration
{
    [TestClass]
    public class MoveGeneration_King_Tests
    {


        [TestMethod]
        public void King_PlainMoves()
        {
            // 7    
            // 6           
            // 5       
            // 4         P
            // 3       K
            // 2       p p
            // 1       
            // 0          
            //   0 1 2 3 4 5 6 7  
            var sut = PieceFactory.Create('K', (3, 3));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(false, (3,2)),
                new Pawn(false, (4,2)),
                new Pawn(true, (4,4)),

                sut
            };

            board.AddNew(pieces);

            var moves = sut.Moves(board).ToList();
            moves.Count.ShouldBe(7);
            var expected = new List<(int, int)>
            {
                (3, 2), (4,2),
                (2,2), (2,3), (2,4), (3,4), (4,3)
            };

            CommonAsserts.Assert_ContainsPositions(moves.Select(m => m.NewPos), expected);
            CommonAsserts.Assert_ContainsCaptures(moves, (3, 2), (4,2));
        }

        [TestMethod]
        public void KingCastling_PawnAttack_ShouldNotCastle()
        {
            // 8r   k  r
            // 7p   P  p
            // 6      
            // 5    
            // 4
            // 3
            // 2P   P  P
            // 1R   K  R       
            //  ABCDEFGH

            var pieces = new List<PieceBase>
            {
                new King(true, "e1"),
                new King(false, "e8"),
                new Rook(true, "a1"),
                new Rook(true, "h1"),
                new Rook(false, "a8"),
                new Rook(false, "h8"),

                new Pawn(true, "a2"),
                new Pawn(true, "e2"),
                new Pawn(true, "h2"),
                new Pawn(true, "e7"),
                new Pawn(false, "a7"),
                new Pawn(false, "h7"),
            };

            var board = BoardFactory.CreateFromPieces(pieces);
            var moves = board.MoveGenerator.MovesQuick(false, true).ToList();

            moves.ShouldNotContain(m => m.Castling);
        }

        [TestMethod]
        public void King_SlidingBishop()
        {
            // 7    
            // 6           
            // 5       
            // 4 
            // 3 
            // 2         b
            // 1 P P P x    
            // 0   K x        
            //   0 1 2 3 4 5 6 7 
            var sut = PieceFactory.Create('K', (1,0));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, (0,1)),
                new Pawn(true, (1,1)),
                new Pawn(true, (2,1)),
                new Bishop(false, (4,2)),

                sut
            };

            board.AddNew(pieces);

            var moves = board.MoveGenerator.MovesQuick(true, true);
            var kingMoves = moves.Where(m => m.PrevPos == sut.CurrentPosition).ToList();
            kingMoves.Count.ShouldBe(1);

            CommonAsserts.Assert_ContainsPositions(kingMoves.Select(m => m.NewPos), (0,0));
        }

        [TestMethod]
        public void King_SlidingRook()
        {
            // 7    
            // 6           
            // 5       
            // 4     r
            // 3     x
            // 2     x     
            // 1 P P x   
            // 0   K x        
            //   0 1 2 3 4 5 6 7  
            var sut = PieceFactory.Create('K', (1, 0));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, (0,1)),
                new Pawn(true, (1,1)),
                new Rook(false, (2,4)),

                sut
            };

            board.AddNew(pieces);

            var moves = board.MoveGenerator.MovesQuick(true, true);
            var kingMoves = moves.Where(m => m.PrevPos == sut.CurrentPosition).ToList();
            kingMoves.Count.ShouldBe(1);

            CommonAsserts.Assert_ContainsPositions(kingMoves.Select(m => m.NewPos), (0, 0));
        }

        [TestMethod]
        public void King_PawnCannotOpenDirectAttackLine()
        {
            // 7    
            // 6           
            // 5       
            // 4     
            // 3 q           b
            // 2   x       x 
            // 1 P P P P P P P  
            // 0       K        
            //   0 1 2 3 4 5 6 7  
            var sut = PieceFactory.Create('K', (3, 0));

            var board = BoardFactory.CreateEmptyBoard();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, (0,1)),
                new Pawn(true, (1,1)),
                new Pawn(true, (2,1)),
                new Pawn(true, (3,1)),
                new Pawn(true, (4,1)),
                new Pawn(true, (5,1)),
                new Pawn(true, (6,1)),
                new Queen(false, (0,3)),
                new Bishop(false, (6,3)),

                sut
            };

            board.AddNew(pieces);

            var moves = board.MoveGenerator.MovesQuick(true, true).ToList();
            var p1Moves = moves.Where(m => m.PrevPos == (2,1)).ToList();
            var p2Moves = moves.Where(m => m.PrevPos == (4,1)).ToList();
            
            p1Moves.ShouldBeEmpty();
            p2Moves.ShouldBeEmpty();
        }
    }
}
