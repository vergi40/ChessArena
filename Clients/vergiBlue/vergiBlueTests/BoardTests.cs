using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vergiBlue;

namespace vergiBlueTests
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void InitializeBoard()
        {
            var board = new Board();
            var pawn1 = new Pawn(false, true, board);
            pawn1.CurrentPosition = (0, 1);

            var pawn2 = new Pawn(false, true, board);
            pawn2.CurrentPosition = (1, 1);

            board.AddNew(pawn1);
            board.AddNew(pawn2);

            pawn1.MoveTo((0,2));

        }
    }
}
