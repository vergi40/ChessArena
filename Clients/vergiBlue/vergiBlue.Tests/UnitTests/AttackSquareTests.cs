using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using UnitTests;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.SubSystems;
using vergiBlue.Pieces;

namespace vergiBlueTests
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
        public void Cache_TrickyEnPassant_ShouldNotContainEnPassant()
        {
            // niche case
            // Can't en passant since leaves attack open 
            // 8       k  
            // 7   
            // 6      o
            // 5K   P p     q     
            // 4
            // 3   
            // 2
            // 1       
            //  A B C D E F G H
            var board = BoardFactory.CreateFromPieces("a5K", "c5P", "d5p", "g5q", "g8k");
            board.Strategic.EnPassantPossibility = "d6".ToTuple();

            // Re-generate since en passant possibility wasn't applied
            board.UpdateAttackCache(false);

            var moves = board.GenerateMovesAndUpdateCache(true).ToList();
            moves.ShouldNotContain(m => m.EnPassant);
        }
    }
}
