using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using vergiBlue;

namespace vergiBlueTests
{
    /// <summary>
    /// Test how much time and evaluations are needed for various situations. Take notes and compare later when
    /// optimizing and logic is improved.
    ///
    /// Manually run after each optimization update. Laptop power mode -> best performance
    /// </summary>
    [TestClass]
    public class BenchMarking
    {
        [TestMethod]
        public void RuyLopez_SearchDepth5_Black()
        {
            // 8R BQ|KBNR
            // 7PPPP| PPP
            // 6  N |
            // 5 B  |P
            // 4    |P
            // 3    | N
            // 2PPPP| PPP
            // 1RNBQ|K  R
            //  ABCD EFGH
            

            var board = new Board();
            board.InitializeEmptyBoard();

            // 1.e4 e5 2.Nf3 Nc6 3.Bb5
            board.ExecuteMove(new SingleMove("e2", "e4"));
            board.ExecuteMove(new SingleMove("e7", "e5"));
            board.ExecuteMove(new SingleMove("g1", "f3"));
            board.ExecuteMove(new SingleMove("b8", "c6"));
            board.ExecuteMove(new SingleMove("f1", "b5"));
            
            var player = new Logic(false);
            player.Board = new Board(board);

            var playerMove = player.CreateMoveWithDepth(5);
            var diagnostics = playerMove.Diagnostics;
            Logger.LogMessage($"Test: {nameof(RuyLopez_SearchDepth5_Black)}. Move: {playerMove.Move.StartPosition} to {playerMove.Move.EndPosition}. {diagnostics.ToString()}");

            // 24.10. depth 4
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to b8. Board evaluations: 2025886. Check evaluations: 1023. Time elapsed: 31392 ms. Available moves found: 31.

            // 24.10. depth 5
            // Test: RuyLopez_SearchDepth5_Black. Board evaluations: 19889371. Check evaluations: 1022. Time elapsed: 318569 ms. Available moves found: 31.

            // 24.10. depth 4. Strategy - class
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to b8. Board evaluations: 2025886. Check evaluations: 1023. Time elapsed: 28803,7124 ms. Available moves found: 31.


            // ------- Order prioritizing
            // 24.10. Depth 4 with order prioritize. Edit Board.Moves() - order capture to be first. Seems like really boosts with alpha-beta pruning
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to b8. Board evaluations: 305303. Check evaluations: 1023. Time elapsed: 5451 ms. Available moves found: 31.

            // 24.10. Depth 5 with order prioritize
            // Test: RuyLopez_SearchDepth5_Black. Move: f7 to f6. Board evaluations: 1532427. Check evaluations: 1022. Time elapsed: 26794 ms. Available moves found: 31.


            // ------- Main allMoves-loop parallelized
            // 25.10. Depth 4. All counter methods commented
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to b8. Board evaluations: 305303. Check evaluations: 1023. Time elapsed: 1387 ms. Available moves found: 31.

            // 25.10. Depth 5. All counter methods commented
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to d4. Board evaluations: 1532427. Check evaluations: 1030. Time elapsed: 7002 ms. Available moves found: 31.


            // -------
            // 29.10. depth 4 
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to b8. Board evaluations: 191032982. Check evaluations: 1023. Time elapsed: 676506 ms. Available moves found: 31.

            // negate
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to a5. Board evaluations: 13251801. Check evaluations: 1024. Time elapsed: 54043 ms. Available moves found: 31.

            // Skip ordering in check checks
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to a5. Board evaluations: 13250777. Check evaluations: 1024. Time elapsed: 25777 ms. Available moves found: 31.

            // Only reorder moves by evaluation at start depth
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to a5. Board evaluations: 2025917. Check evaluations: 1024. Time elapsed: 10514 ms. Available moves found: 31.


            // Only reorder moves by evaluation at start depth. Otherwise prioritize by capture.
            // Depth 4
            // Test: RuyLopez_SearchDepth5_Black. Move: d8 to f6. Board evaluations: 305334. Check evaluations: 1033. Time elapsed: 1568 ms. Available moves found: 31.
            // Depth 5
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to d4. Board evaluations: 1532458. Check evaluations: 1030. Time elapsed: 7147 ms. Available moves found: 31.
            // --> disabled for now


            // Fix pawn bug
            // 10.1. Depth 5
            // Test: RuyLopez_SearchDepth5_Black. Move: c6 to d4. Board evaluations: 1345201. Check evaluations: 967. Time elapsed: 5653 ms. Available moves found: 30.
        }
    }
}
