using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using vergiBlue;
using vergiBlue.Pieces;

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
        // Separated different config runs 22.1.2021 
        
        [TestMethod]
        public void RunAll_Parallel_NoTranspositions()
        {
            RunAll(5, true, false);

            // 4.9 sec. 22.1.2021
            // Test: RuyLopez_Black. Move: g8 to e7. Depth 5
            // Board evaluations: 1265055. Check evaluations: 953. Time elapsed: 1929 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: e4 to e2. Depth 5
            // Board evaluations: 2765487. Check evaluations: 1472. Time elapsed: 3004 ms. Available moves found: 40.
        }

        [TestMethod]
        public void RunAll_NoParallel_NoTranspositions()
        {
            RunAll(5, false, false);

            // 15 sec. 22.1.2021
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1265055. Check evaluations: 968. Time elapsed: 5705 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 2765487. Check evaluations: 1471. Time elapsed: 9276 ms. Available moves found: 40.
        }

        [TestMethod]
        public void RunAll_NoParallel_Transpositions()
        {
            RunAll(5, false, true);

            // 13 sec. 22.1.2021
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 565904. Check evaluations: 968. Alpha cutoffs: 33954. Beta cutoffs: 92882. Priority moves found: 54774.
            // Transpositions used: 379962. Time elapsed: 4338 ms. Available moves found: 30. Transposition tables saved: 657111
            // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
            // Board evaluations: 1233950. Check evaluations: 1475. Alpha cutoffs: 221560. Beta cutoffs: 63950. Priority moves found: 103245.
            // Transpositions used: 1033679. Time elapsed: 8738 ms. Available moves found: 40. Transposition tables saved: 1426815
        }

        private void RunAll(int searchDepth, bool parallel, bool transpositions)
        {
            // Ruy lopez opening
            RuyLopez_Black(searchDepth, parallel, transpositions);

            // https://thechessworld.com/articles/endgame/7-greatest-chess-endings/
            // #1 at pair23
            GreatestEndings_1_MidGame(searchDepth, parallel, transpositions);

            // Current standings 17.1.2021 -----------------

            // 15.2 sec normal search
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1265055. Check evaluations: 968. Time elapsed: 5822 ms. Available moves found: 30. Transposition tables saved: 0
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 2765487. Check evaluations: 1471. Time elapsed: 9313 ms. Available moves found: 40. Transposition tables saved: 0

            // 4.3 sec parallel search
            // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
            // Board evaluations: 1265055. Check evaluations: 967. Time elapsed: 1627 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
            // Board evaluations: 2765487. Check evaluations: 1475. Time elapsed: 2634 ms. Available moves found: 40.

            // 21 sec normal search using transposition tables. Still needs some tuning
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1099116. Check evaluations: 968. Alpha cutoffs: 34157. Beta cutoffs: 103867. Priority moves found: 68067. Time elapsed: 6283 ms. Available moves found: 30. Transposition tables saved: 668577
            // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
            // Board evaluations: 2959711. Check evaluations: 1475. Alpha cutoffs: 257178. Beta cutoffs: 65191. Priority moves found: 198480. Time elapsed: 14003 ms. Available moves found: 40. Transposition tables saved: 1569299

            // 20 sec. more logging items
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1099116. Check evaluations: 968. Alpha cutoffs: 34157. Beta cutoffs: 103867. Priority moves found: 68067. Transpositions used: 34501.
            // Time elapsed: 6445 ms. Available moves found: 30. Transposition tables saved: 668577
            // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
            // Board evaluations: 2959711. Check evaluations: 1475. Alpha cutoffs: 257178. Beta cutoffs: 65191. Priority moves found: 198480. Transpositions used: 197693.
            // Time elapsed: 14777 ms. Available moves found: 40. Transposition tables saved: 1569299

            // 13.2 sec. Fixed search depth when saving transposition results
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 565904. Check evaluations: 968. Alpha cutoffs: 33954. Beta cutoffs: 92882. Priority moves found: 54774. Transpositions used: 379962.
            // Time elapsed: 4444 ms. Available moves found: 30. Transposition tables saved: 657111
            // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
            // Board evaluations: 1233950. Check evaluations: 1475. Alpha cutoffs: 221560. Beta cutoffs: 63950. Priority moves found: 103245. Transpositions used: 1033679.
            // Time elapsed: 8690 ms. Available moves found: 40. Transposition tables saved: 1426815

        }

        internal static Board CreateRuyLopezOpeningBoard()
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
            return board;
        }

        public void RuyLopez_Black(int searchDepth, bool parallel, bool transpositions)
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
            var player = new Logic(false);
            player.Board = new Board(CreateRuyLopezOpeningBoard());

            player.UseParallelComputation = parallel;
            player.UseTranspositionTables = transpositions;

            var playerMove = player.CreateMoveWithDepth(searchDepth);
            var diagnostics = playerMove.Diagnostics;
            Logger.LogMessage($"// Test: {nameof(RuyLopez_Black)}. Move: {playerMove.Move.StartPosition} to {playerMove.Move.EndPosition}. Depth {searchDepth}");
            Logger.LogMessage($"// {diagnostics.ToString()}");
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

        public void GreatestEndings_1_MidGame(int searchDepth, bool parallel, bool transpositions)
        {
            var board = new Board();
            var pieces = new List<PieceBase>
            {
                new Pawn(true, "a4"),
                new Pawn(true, "c3"),
                new Pawn(true, "c4"),
                new Pawn(true, "d4"),
                new Pawn(true, "f4"),
                new Pawn(true, "g3"),
                new Pawn(true, "h4"),
                new Pawn(false, "a7"),
                new Pawn(false, "b6"),
                new Pawn(false, "c7"),
                new Pawn(false, "d5"),
                new Pawn(false, "e6"),
                new Pawn(false, "g5"),
                new Pawn(false, "h7"),
                new Pawn(false, "c5"),
                new Rook(true, "a1"),
                new Rook(true, "e1"),
                new Rook(false, "e8"),
                new Rook(false, "f7"),
                new Bishop(true, "d3"),
                new Knight(false, "a5"),
                new Queen(true, "e4"),
                new Queen(false, "d7")
            };
            board.AddNew(pieces);

            // 
            var whiteKing = new King(true, "g2");
            var blackKing = new King(false, "f8");
            board.AddNew(whiteKing, blackKing);
            board.Kings = (whiteKing, blackKing);

            var player = new Logic(true);
            player.Board = new Board(board);

            player.UseParallelComputation = parallel;
            player.UseTranspositionTables = transpositions;

            var playerMove = player.CreateMoveWithDepth(searchDepth);
            var diagnostics = playerMove.Diagnostics;
            Logger.LogMessage($"// Test: {nameof(GreatestEndings_1_MidGame)}. Move: {playerMove.Move.StartPosition} to {playerMove.Move.EndPosition}. Depth {searchDepth}");
            Logger.LogMessage($"// {diagnostics.ToString()}");
        }


        // RUNALL old entries

        // Original dictionary benchmarking ---------------

        // New benchmarking framework with 2 game tests.
        // Test: RuyLopez_Black.Move: c6 to d4.Depth 5
        // Board evaluations: 1345201.Check evaluations: 967.Time elapsed: 5741 ms.Available moves found: 30.
        // Test: GreatestEndings_1_MidGame.Move: e4 to f3.Depth 5
        // Board evaluations: 3372180.Check evaluations: 1473.Time elapsed: 11846 ms.Available moves found: 40.

        // 18sec. Changed sort methods to orderBy. Maybe need to revert?
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1345201. Check evaluations: 967. Time elapsed: 6133 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
        // Board evaluations: 3372180. Check evaluations: 1471. Time elapsed: 11884 ms. Available moves found: 40.

        // 16.1 sec. Sort evaluated moves. OrderBy captured moves.
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1345201. Check evaluations: 967. Time elapsed: 5068 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
        // Board evaluations: 3372180. Check evaluations: 1471. Time elapsed: 10984 ms. Available moves found: 40.

        // 14.2 sec. EvaluationResult + capturelist
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1345201. Check evaluations: 967. Time elapsed: 4752 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to e2. Depth 5
        // Board evaluations: 3372180. Check evaluations: 1472. Time elapsed: 9432 ms. Available moves found: 40.




        // Array benchmarking ----------

        // 17.6 & 16.8 sec. Change board type to array.
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1328787. Check evaluations: 967. Time elapsed: 5275 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
        // Board evaluations: 3331378. Check evaluations: 1471. Time elapsed: 11549 ms. Available moves found: 40.

        // 14.4 & 17.2. Implement EvaluationResult to get constant time min&max
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1328787. Check evaluations: 967. Time elapsed: 5157 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to f3. Depth 5
        // Board evaluations: 3331378. Check evaluations: 1473. Time elapsed: 12081 ms. Available moves found: 40.

        // 12.3 & 12.8 sec. Skip capture OrderBy with capturelist
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1328787. Check evaluations: 967. Time elapsed: 4022 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to e2. Depth 5
        // Board evaluations: 3331378. Check evaluations: 1472. Time elapsed: 8271 ms. Available moves found: 40.

        // 12.6 sec. Array in 2-dimension. 14.1.2021
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1328787. Check evaluations: 967. Time elapsed: 4040 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to e2. Depth 5
        // Board evaluations: 3331378. Check evaluations: 1472. Time elapsed: 8512 ms. Available moves found: 40.

        // 5.7 sec. Small minimax algorithm fix. Don't create moves in leaf0
        // 18.6 sec non-parallel
        // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
        // Board evaluations: 1345201. Check evaluations: 967. Time elapsed: 2255 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to e2. Depth 5
        // Board evaluations: 3372180. Check evaluations: 1472. Time elapsed: 3415 ms. Available moves found: 40.


        // Transposition tables. Buggy ------------

        // 65 ms. Transposition tables on
        // Test: RuyLopez_Black. Move: a7 to a6. Depth 5
        // Board evaluations: 98. Check evaluations: 964. Time elapsed: 31 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
        // Board evaluations: 145. Check evaluations: 1471. Time elapsed: 18 ms. Available moves found: 40.

        // 916 ms. Transposition tables to 64-bit. More logic to updating
        // Test: RuyLopez_Black. Move: a7 to a6. Depth 5
        // Board evaluations: 327. Check evaluations: 964. Time elapsed: 218 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
        // Board evaluations: 279. Check evaluations: 1471. Time elapsed: 684 ms. Available moves found: 40.


        // 35 sec. Multiple bugg fixes. Instead of couple thousand tables, now transition tablecount for ruylopez around 1 500 000
        // Without transposition tables: 48 sec.
        // Original parallel 15.1sec
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1118265. Check evaluations: 968. Time elapsed: 9520 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to f3. Depth 5
        // Board evaluations: 4685260. Check evaluations: 1473. Time elapsed: 25501 ms. Available moves found: 40.

        // 26.6 sec. Improved minimax structure. Table count for ruy lopez around 900 000
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1352189. Check evaluations: 968. Time elapsed: 7235 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
        // Board evaluations: 5148005. Check evaluations: 1475. Time elapsed: 19374 ms. Available moves found: 40.

        // TEMP 27.1 sec. deepervalue
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1417893. Check evaluations: 968. Time elapsed: 7558 ms. Available moves found: 30. 
        // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
        // Board evaluations: 4939343. Check evaluations: 1475. Time elapsed: 19534 ms. Available moves found: 40.
        // TEMP 18 sec. Skip all transposition substitutes
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1290560. Check evaluations: 968. Time elapsed: 6894 ms. Available moves found: 30. Transposition tables saved: 845888
        // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
        // Board evaluations: 3132321. Check evaluations: 1471. Time elapsed: 11262 ms. Available moves found: 40. Transposition tables saved: 1590163

        // 26.8 sec. Replacement strategy
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1419549. Check evaluations: 968. Time elapsed: 7636 ms. Available moves found: 30. Transposition tables saved: 959233
        // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
        // Board evaluations: 4957787. Check evaluations: 1475. Time elapsed: 19119 ms. Available moves found: 40. Transposition tables saved: 2779385


        // 25.5 sec 25.9 (34.1 sec without replace). Order bound-moves to start of the list. Always replace transposition entry if lower or upper bound found.
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1348026. Check evaluations: 968. Time elapsed: 9224 ms. Available moves found: 30. Transposition tables saved: 923675
        // Test: GreatestEndings_1_MidGame. Move: e4 to f3. Depth 5
        // Board evaluations: 3627661. Check evaluations: 1473. Time elapsed: 16621 ms. Available moves found: 40. Transposition tables saved: 1965069

        // 21 sec alpha-beta fix.
        // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
        // Board evaluations: 1268350. Check evaluations: 968. Time elapsed: 7332 ms. Available moves found: 30. Transposition tables saved: 778938
        // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
        // Board evaluations: 2876222. Check evaluations: 1475. Time elapsed: 13712 ms. Available moves found: 40. Transposition tables saved: 1510389
        // 21 sec alpha-beta transpos save depth fix
        // Board evaluations: 1099116. Check evaluations: 968. Time elapsed: 6777 ms. Available moves found: 30. Transposition tables saved: 668577
        // Test: GreatestEndings_1_MidGame. Move: e4 to e5. Depth 5
        // Board evaluations: 2959711. Check evaluations: 1475. Time elapsed: 14284 ms. Available moves found: 40. Transposition tables saved: 1569299
    }
}
