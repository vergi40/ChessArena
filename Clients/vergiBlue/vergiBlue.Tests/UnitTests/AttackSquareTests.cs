﻿using System.Linq;
using CommonNetStandard.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.SubSystems;

namespace UnitTests
{
    [TestClass]
    public class AttackSquareTests
    {
        [TestMethod]
        public void Initialization_MapperShouldContainKnightAttacks()
        {
            var board = BoardFactory.CreateDefault();

            var instance = new AttackSquareMapper(board);

            instance.IsPositionAttacked((0,2), true).ShouldBeTrue();
            instance.IsPositionAttacked((2,2), true).ShouldBeTrue();
            instance.IsPositionAttacked((5,2), true).ShouldBeTrue();
            instance.IsPositionAttacked((7,2), true).ShouldBeTrue();

            instance.IsPositionAttacked((0, 5), false).ShouldBeTrue();
            instance.IsPositionAttacked((2, 5), false).ShouldBeTrue();
            instance.IsPositionAttacked((5, 5), false).ShouldBeTrue();
            instance.IsPositionAttacked((7, 5), false).ShouldBeTrue();
        }

        [TestMethod]
        public void AfterOpening_MapperShouldContainQueenBishopSquares()
        {
            var board = BoardFactory.CreateDefault();

            var instance = new AttackSquareMapper(board);

            // Open room for queen and bishop
            var move = new SingleMove((4, 1), (4, 2));
            board.ExecuteMove(move);
            instance.Update(board, move);

            // Q
            instance.IsPositionAttacked((4, 1), true).ShouldBeTrue();
            instance.IsPositionAttacked((5, 2), true).ShouldBeTrue();
            instance.IsPositionAttacked((6, 3), true).ShouldBeTrue();
            instance.IsPositionAttacked((7, 4), true).ShouldBeTrue();

            // B
            instance.IsPositionAttacked((3, 2), true).ShouldBeTrue();
            instance.IsPositionAttacked((2, 3), true).ShouldBeTrue();
            instance.IsPositionAttacked((1, 4), true).ShouldBeTrue();
            instance.IsPositionAttacked((0, 5), true).ShouldBeTrue();
        }

        [TestMethod]
        public void AfterOpening_CacheShouldContainQueenBishopSquares()
        {
            var board = BoardFactory.CreateDefault();

            // Open room for queen and bishop
            var move = new SingleMove((4, 1), (4, 2));
            board.ExecuteMove(move);

            board.UpdateAttackCache(true);

            var targets = board.MoveGenerator.GetAttacks(true).CaptureTargets;

            // Q
            targets.ShouldContain((4,1));
            targets.ShouldContain((5,2));
            targets.ShouldContain((6,3));
            targets.ShouldContain((7,4));

            // B
            targets.ShouldContain((3,2));
            targets.ShouldContain((2,3));
            targets.ShouldContain((1,4));
            targets.ShouldContain((0,5));
        }

        [TestMethod]
        public void Cache_AfterExecuteMove_ShouldUpdate()
        {
            // 8 k   
            // 7           
            // 6       
            // 5 
            // 4             q
            // 3           x
            // 2   P P P x    
            // 1   K   x       
            //   A B C D E F G H

            var board = BoardFactory.CreateFromPieces("b1K", "b2P", "c2P", "d2P", "g4q");
            
            var preWhiteMoves = board.GenerateMovesAndUpdateCache(true).ToList();
            CommonAsserts.Assert_ContainsPositions(preWhiteMoves, "c1", "a1", "a2");

            board.ExecuteMove(new SingleMove("g4", "d1"));

            // Only option is king to a2
            // 8 k   
            // 7           
            // 6       
            // 5 
            // 4           
            // 3         
            // 2   P P P   
            // 1   K   q      
            //   A B C D E F G H

            var postWhiteMoves = board.GenerateMovesAndUpdateCache(true).ToList();
            var kingMoves = postWhiteMoves.Where(m => m.PrevPos.Equals((1, 0)));
            var kingMove = kingMoves.ShouldHaveSingleItem("King should only have one place to go");
            kingMove.NewPos.ShouldBe((0, 1));

            board.ExecuteMove(kingMove);
            board.ExecuteMove(new SingleMove("d1", "c2", true));
            // Can't move pawn b2, it's guarding
            // 8 k   
            // 7           
            // 6       
            // 5 
            // 4           
            // 3         
            // 2 K P q P   
            // 1             
            //   A B C D E F G H

            // TODO why a1 is still in capture targets?

            var moves = board.GenerateMovesAndUpdateCache(true).ToList();
            CommonAsserts.Assert_ContainsPositions(moves, "a1", "a3");
            foreach (var move in moves)
            {
                move.PrevPos.ShouldNotBe("b2".ToTuple());
            }
        }

        [TestMethod]
        public void CacheRook_TrickyEnPassant_ShouldNotContainEnPassant()
        {
            // niche case
            // En passant will leave king open
            // 8K     K  
            // 7   
            // 6      o
            // 5K   P p      r     
            // 4
            // 3         b
            // 2
            // 1      r
            //  A B C D E F G H
            var board = BoardFactory.CreateFromPieces("a5K", "c5P", "d5p", "g5r", "g8k");
            board.Strategic.EnPassantPossibility = "d6".ToTuple();

            // Re-generate since en passant possibility wasn't applied
            board.UpdateAttackCache(false);

            var moves = board.GenerateMovesAndUpdateCache(true).ToList();
            moves.ShouldNotContain(m => m.EnPassant);
        }

        [TestMethod]
        public void CacheBishop_TrickyEnPassant_ShouldNotContainEnPassant()
        {
            // niche case
            // En passant will leave king open
            // 8K     K  
            // 7   
            // 6      o
            // 5K   P p      r     
            // 4
            // 3         b
            // 2
            // 1      r
            //  A B C D E F G H
            var board = BoardFactory.CreateFromPieces("a8K", "c5P", "d5p", "f3b", "g8k");
            board.Strategic.EnPassantPossibility = "d6".ToTuple();

            // Re-generate since en passant possibility wasn't applied
            board.UpdateAttackCache(false);

            var moves = board.GenerateMovesAndUpdateCache(true).ToList();
            moves.ShouldNotContain(m => m.EnPassant);
        }

        [TestMethod]
        public void Cache_PromotionKnight()
        {
            var fen = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out var whiteStart);

            var move = new SingleMove("g2", "g1", false, PromotionPieceType.Knight);
            board.ExecuteMove(move);

            // King has to move 
            var moves = board.MoveGenerator.GenerateMovesAndUpdateCache(true).ToList();
        }

        [TestMethod]
        public void Cache_PromotionKnight_Simple()
        {
            // 8k
            // 7 
            // 6
            // 5
            // 4
            // 3 x
            // 2pKx
            // 1N 
            //  ABCDEFGH
            var board = BoardFactory.CreateFromPieces("b2K", "a2p", "a8k");

            var move = new SingleMove("a2", "a1", false, PromotionPieceType.Knight);
            board.ExecuteMove(move);

            // King can move or capture
            var moves = board.MoveGenerator.GenerateMovesAndUpdateCache(true).ToList();
            moves.ShouldNotContain(m => m.NewPos.Equals("c2".ToTuple()));
            moves.ShouldNotContain(m => m.NewPos.Equals("b3".ToTuple()));
        }

        [TestMethod]
        public void Cache_PromotionKnight_KingUnderAttack_Simple()
        {
            // 8k
            // 7 
            // 6
            // 5
            // 4  P
            // 3 x
            // 2p K
            // 1N 
            //  ABCDEFGH
            var board = BoardFactory.CreateFromPieces("c2K", "c4P", "a2p", "a8k");

            var move = new SingleMove("a2", "a1", false, PromotionPieceType.Knight);
            board.ExecuteMove(move);

            // King can move or capture
            var moves = board.MoveGenerator.GenerateMovesAndUpdateCache(true).ToList();
            moves.ShouldNotContain(m => m.NewPos.Equals("b3".ToTuple()));
            moves.ShouldNotContain(m => m.NewPos.Equals("c5".ToTuple()));
        }

        [TestMethod]
        public void DoubleRookCheckMate_ShouldNotBeAnyMovesForKing()
        {
            // Easy double rook checkmate

            // 8r     K
            // 7 r
            // 6
            // 5k
            // 4
            // 3
            // 2
            // 1
            //  ABCDEFGH

            var board = BoardFactory.CreateFromPieces("a1k", "a8r", "b7r", "g8K");
            board.Shared.GameTurnCount = 20;

            var moves = board.GenerateMovesAndUpdateCache(true).ToList();
            moves.ShouldBeEmpty();
        }

        [TestMethod]
        public void King_AfterPromotion_ShouldNotCaptureGuarded()
        {
            // 8
            // 7PPPk 
            // 6
            // 5
            // 4
            // 3
            // 2    Kppp
            // 1
            //  ABCDEFGH
            // Just kings and pawns. King does greedy invalid capture moves
            var fen = "8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out _);

            board.ExecuteMove(new SingleMove("g2", "g1", false, PromotionPieceType.Queen));

            var kingMoves = board.GenerateMovesAndUpdateCache(true).ToList();
            kingMoves.ShouldNotContain(m => m.NewPos.Equals("f2".ToTuple()));
        }
    }
}
