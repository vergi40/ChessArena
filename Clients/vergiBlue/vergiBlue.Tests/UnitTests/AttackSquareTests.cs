using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;

namespace vergiBlueTests
{
    [TestClass]
    public class AttackSquareTests
    {
        [TestMethod]
        public void Initialization_ShouldContainKnightAttacks()
        {
            var board = BoardFactory.CreateDefault();

            var instance = new AttackSquares(board);

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
        public void AfterOpening_ShouldContainQueenBishopSquares()
        {
            var board = BoardFactory.CreateDefault();

            var instance = new AttackSquares(board);

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
    }
}
