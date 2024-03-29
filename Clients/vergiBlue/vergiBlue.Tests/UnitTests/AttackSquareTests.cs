﻿using System.Linq;
using CommonNetStandard.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;

namespace UnitTests
{
    // Board template
    // 8
    // 7
    // 6
    // 5
    // 4
    // 3
    // 2
    // 1
    //  A B C D E F G H

    [TestClass]
    public class AttackSquareTests
    {
        [TestMethod]
        public void AfterOpening_CacheShouldContainQueenBishopSquares()
        {
            var board = BoardFactory.CreateDefault();

            // Open room for queen and bishop
            var move = new SingleMove((4, 1), (4, 2));
            board.ExecuteMove(move);

            var targets = board.MoveGenerator.AttackMoves(true).Select(m => m.NewPos).ToList();

            // Q
            targets.ShouldContain((4, 1));
            targets.ShouldContain((5, 2));
            targets.ShouldContain((6, 3));
            targets.ShouldContain((7, 4));

            // B
            targets.ShouldContain((3, 2));
            targets.ShouldContain((2, 3));
            targets.ShouldContain((1, 4));
            targets.ShouldContain((0, 5));
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

            var preWhiteMoves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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

            var postWhiteMoves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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

            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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
            
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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
            
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
            moves.ShouldNotContain(m => m.EnPassant);
        }

        [TestMethod]
        public void Cache_BishopPromotion()
        {
            var board = BoardFactory.CreateFromFen("8/PPPk4/8/8/8/8/4Kppp/8 w - - 0 1", out var whiteStarts);
            var promotion = new SingleMove("c7", "c8") { PromotionType = PromotionPieceType.Bishop };
            var next = BoardFactory.CreateFromMove(board, promotion);

            // 8    B
            // 7P P   k   
            // 6     
            // 5
            // 4
            // 3
            // 2        K p p p
            // 1
            //  A B C D E F G H
            var moves = next.MoveGenerator.ValidMovesQuick(false).ToList();

            // d7c8 should be invalid
            moves.Count.ShouldBe(6);
        }

        [TestMethod]
        public void Cache_r3k2r_h3g2()
        {
            // position fen "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1" moves e1f1 h3g2
            // go perft 1
            // f3g2: 1
            // f1e1: 1
            // f1g1: 1
            // f1g2: 1
            // 
            // Nodes searched: 4
            var board = BoardFactory.CreateFromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", out var whiteStarts);
            board.ExecuteMove(SingleMoveFactory.Create("e1f1"));
            board.ExecuteMove(SingleMoveFactory.Create("h3g2", true));

            // 3          Q
            // 2        B P p P  
            // 1          K   R
            //  A B C D E F G H
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();

            // f3g2x missing
            // KingDirectAttackMap.AllAttackers().ToList();
            // Contains all promotions as different attackers
            moves.Count.ShouldBe(4);
        }

        [TestMethod]
        public void Cache_r3k2r_c7c5()
        {
            var board = BoardFactory.CreateFromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", out var whiteStarts);
            board.ExecuteMove(SingleMoveFactory.Create("e1f1"));
            board.ExecuteMove(SingleMoveFactory.Create("c7c5"));

            // En passant
            // 8r
            // 7p   p p  
            // 6b n     
            // 5      P
            //  A B C D E F G H
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();

            // d5c6 missing
            moves.Count.ShouldBe(46);

            // TODO depth 4 fix missing
            // Perft failed for [r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1] depth 4
        }

        [TestMethod]
        public void Cache_PromotionKnight()
        {
            var fen = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1";
            var board = BoardFactory.CreateFromFen(fen, out var whiteStart);

            var move = new SingleMove("g2", "g1", false, PromotionPieceType.Knight);
            board.ExecuteMove(move);

            // King has to move 
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
            moves.Count.ShouldBe(5);
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
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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

            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();
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

            var kingMoves = board.MoveGenerator.ValidMovesQuick(true).ToList();
            kingMoves.ShouldNotContain(m => m.NewPos.Equals("f2".ToTuple()));
        }

        [TestMethod]
        public void Slider_AfterObstacleMovedAway_ShouldCheck()
        {
            // 8
            // 7      x
            // 6k     P   R
            // 5   
            // 4      x
            // 3      P
            // 2        B
            // 1K
            //  A B C D E F G H
            var startBoard = BoardFactory.CreateFromPieces("a1K", "a6k", "d3P", "d6P", "e2B", "f6R");

            var caseBishop = BoardFactory.CreateFromMove(startBoard, SingleMoveFactory.Create("d3d4"));

            var moves = caseBishop.MoveGenerator.ValidMovesQuick(false);
            moves.ShouldNotContain(m => m.NewPos.Equals("b5".ToTuple()));
        }

        [TestMethod]
        public void KingThreatened_CantCaptureGuarded()
        {
            // Board template
            // 8k
            // 7
            // 6
            // 5
            // 4
            // 3    p
            // 2  p  
            // 1K
            //  A B C D E F G H
            var board = BoardFactory.CreateFromPieces("a1K", "a8k", "b2p", "c3p");
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();

            moves.ShouldNotContain(m => m.NewPos.Equals("b2".ToTuple()));
        }

        [TestMethod]
        public void KingNotThreatened_CantCaptureGuarded()
        {
            // Board template
            // 8k
            // 7
            // 6
            // 5
            // 4
            // 3    p
            // 2  p  
            // 1  K
            //  A B C D E F G H
            var board = BoardFactory.CreateFromPieces("b1K", "a8k", "b2p", "c3p");
            var moves = board.MoveGenerator.ValidMovesQuick(true).ToList();

            moves.ShouldNotContain(m => m.NewPos.Equals("b2".ToTuple()));
        }
    }
}
