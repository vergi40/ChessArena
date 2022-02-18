using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
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
    }
}
