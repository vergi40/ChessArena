using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using vergiBlue.BoardModel;

namespace vergiBlueTests
{
    internal static class CommonAsserts
    {
        public static void ShouldMatch(IBoard board1, IBoard board2)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var tile1 = board1.ValueAt((i, j));
                    var tile2 = board2.ValueAt((i, j));

                    if(tile1 == null) tile2.ShouldBeNull();
                    else
                    {
                        tile1.IsWhite.ShouldBe(tile2.IsWhite);
                        tile1.Identity.ShouldBe(tile2.Identity);
                    }
                }
            }
        }
    }
}
