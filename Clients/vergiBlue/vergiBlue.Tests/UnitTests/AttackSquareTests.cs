using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.BoardModel.SubSystems;

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
    }
}
