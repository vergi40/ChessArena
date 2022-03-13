using System.Collections.Generic;
using System.Linq;
using Shouldly;
using vergiBlue;
using vergiBlue.BoardModel;

namespace UnitTests
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

        internal static void Assert_ContainsPositions(IEnumerable<(int column, int row)> result, List<(int, int)> expected)
        {
            Assert_ContainsPositions(result, expected.ToArray());
        }
        internal static void Assert_ContainsPositions(IEnumerable<(int column, int row)> result, params (int, int)[] expected)
        {
            foreach (var (column, row) in expected)
            {
                result.ShouldContain(r => r.column == column && r.row == row);
            }
        }

        internal static void Assert_ContainsPositions(List<SingleMove> result, params (int, int)[] expected)
        {
            foreach (var (column, row) in expected)
            {
                var move = result.First(r => r.NewPos.column == column && r.NewPos.row == row);
                move.ShouldNotBeNull();
            }
        }

        internal static void Assert_ContainsPositions(List<SingleMove> result, params string[] expected)
        {
            var toTuple = expected.Select(p => p.ToTuple()).ToList();
            Assert_ContainsPositions(result.Select(r => r.NewPos), toTuple);
        }

        internal static void Assert_ContainsCaptures(List<SingleMove> result, params (int, int)[] expected)
        {
            foreach (var (column, row) in expected)
            {
                var move = result.First(r => r.NewPos.column == column && r.NewPos.row == row);
                move.ShouldNotBeNull();
                move.Capture.ShouldBeTrue();
            }
        }
    }


}
