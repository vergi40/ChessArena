using System.Collections.Generic;
using System.Configuration;
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
        public void RunAll_IterativeDeepening()
        {
            var settings = new LogicSettings()
            {
                UseParallelComputation = false,
                UseTranspositionTables = true,
                UseIterativeDeepening = true,
                UseFullDiagnostics = true
            };
            RunAll(5, settings);

            // 17.4 sec. 22.1.2021 initial algorithm
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1447494. Check evaluations: 968. Time elapsed: 6706 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 3118588. Check evaluations: 1471. Time elapsed: 10654 ms. Available moves found: 40.


            // 45 sec. transpositions
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 2193664. Check evaluations: 968. Alpha cutoffs: 85401. Beta cutoffs: 172214. Priority moves found: 213821.
            // Transpositions used: 59818. Time elapsed: 12252 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 6602273. Check evaluations: 1471. Alpha cutoffs: 744282. Beta cutoffs: 181376. Priority moves found: 594487.
            // Transpositions used: 246214. Time elapsed: 35217 ms. Available moves found: 40.

            // DEBUG Depth 4 totals
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 4
            // Board evaluations: 221522. Check evaluations: 968. Alpha cutoffs: 44341. Beta cutoffs: 8942. Priority moves found: 59691.
            // Transpositions used: 5950. Time elapsed: 1804 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 4
            // Board evaluations: 630001. Check evaluations: 1471. Alpha cutoffs: 14300. Beta cutoffs: 86228. Priority moves found: 32468.
            // Transpositions used: 30463. Time elapsed: 3666 ms. Available moves found: 40.

            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 2194191. Check evaluations: 968. Alpha cutoffs: 85609. Beta cutoffs: 172275. Priority moves found: 214735.
            // Transpositions used: 59079. Time elapsed: 12294 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 6598637. Check evaluations: 1471. Alpha cutoffs: 743992. Beta cutoffs: 181305. Priority moves found: 590768.
            // Transpositions used: 245099. Time elapsed: 33549 ms. Available moves found: 40.

            // 7.8 sec. 24.1.2021 - Keep tabs on alpha&beta values on the minimax launching function
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 223201. Check evaluations: 968. Alpha cutoffs: 5931. Beta cutoffs: 63272. Priority moves found: 37437.
            // Transpositions used: 4810. Time elapsed: 2087 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 882658. Check evaluations: 1471. Alpha cutoffs: 212237. Beta cutoffs: 17395. Priority moves found: 75884.
            // Transpositions used: 25212. Time elapsed: 5729 ms. Available moves found: 40.
        }

        [TestMethod]
        public void RunAll_IterativeDeepening_NoTranspositions()
        {
            var settings = new LogicSettings()
            {
                UseParallelComputation = false,
                UseTranspositionTables = false,
                UseIterativeDeepening = true,
                UseFullDiagnostics = true
            };
            RunAll(5, settings);

            // 17-18 sec. 
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1447494. Check evaluations: 968. Alpha cutoffs: 103126. Beta cutoffs: 76564. Time elapsed: 6762 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 3118588. Check evaluations: 1471. Alpha cutoffs: 130994. Beta cutoffs: 180534. Time elapsed: 10460 ms. Available moves found: 40.

            // 3.6 sec. 24.1.2021 - Keep tabs on alpha&beta values on the minimax launching function
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 231069. Check evaluations: 968. Alpha cutoffs: 62084. Beta cutoffs: 5624. Time elapsed: 1530 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 423044. Check evaluations: 1471. Alpha cutoffs: 11542. Beta cutoffs: 106093. Time elapsed: 2018 ms. Available moves found: 40.

            // 5.4 sec. 19.2. add timer and midresult check
            // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
            // Board evaluations: 448738. Check evaluations: 967. Alpha cutoffs: 66099. Beta cutoffs: 16213. Time elapsed: 2978 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 330887. Check evaluations: 1471. Alpha cutoffs: 11189. Beta cutoffs: 100620. Time elapsed: 2412 ms. Available moves found: 40.
        }

        [TestMethod]
        public void RunAll_Parallel_NoTranspositions()
        {
            var settings = new LogicSettings()
            {
                UseParallelComputation = true,
                UseTranspositionTables = false,
                UseIterativeDeepening = false,
                UseFullDiagnostics = true
            };
            RunAll(5, settings);

            // 4.9 sec. 22.1.2021
            // Test: RuyLopez_Black. Move: g8 to e7. Depth 5
            // Board evaluations: 1265055. Check evaluations: 953. Time elapsed: 1929 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: e4 to e2. Depth 5
            // Board evaluations: 2765487. Check evaluations: 1472. Time elapsed: 3004 ms. Available moves found: 40.
        }

        [TestMethod]
        public void RunAll_NoParallel_NoTranspositions()
        {
            var settings = new LogicSettings()
            {
                UseParallelComputation = false,
                UseTranspositionTables = false,
                UseIterativeDeepening = false,
                UseFullDiagnostics = true
            };
            RunAll(5, settings);

            // 14-15 sec. 22.1.2021
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1265055. Check evaluations: 968. Time elapsed: 5705 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 2765487. Check evaluations: 1471. Time elapsed: 9276 ms. Available moves found: 40.

            // 2.7 sec. 24.1.2021 - Keep tabs on alpha&beta values on the minimax launching function
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 155504. Check evaluations: 968. Alpha cutoffs: 58928. Beta cutoffs: 2906. Time elapsed: 1238 ms. Available moves found: 30. 
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 239509. Check evaluations: 1471. Alpha cutoffs: 3275. Beta cutoffs: 100822. Time elapsed: 1482 ms. Available moves found: 40.
        }

        [TestMethod]
        public void RunAll_NoParallel_Transpositions()
        {
            var settings = new LogicSettings()
            {
                UseParallelComputation = false,
                UseTranspositionTables = true,
                UseIterativeDeepening = false,
                UseFullDiagnostics = true
            };
            RunAll(5, settings);

            // 22 sec. 22.1.2021
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1723082. Check evaluations: 968. Alpha cutoffs: 43644. Beta cutoffs: 140662. Priority moves found: 107279.
            // Transpositions used: 55172. Time elapsed: 9151 ms. Available moves found: 30. Transposition tables saved: 907124
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 2970965. Check evaluations: 1471. Alpha cutoffs: 222308. Beta cutoffs: 64828. Priority moves found: 81349.
            // Transpositions used: 124315. Time elapsed: 13191 ms. Available moves found: 40. Transposition tables saved: 1441064

            // 22 sec. Added minimax breaks
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 1500087. Check evaluations: 968. Alpha cutoffs: 41516. Beta cutoffs: 132503. Priority moves found: 96840.
            // Transpositions used: 36296. Time elapsed: 8382 ms. Available moves found: 30. Transposition tables saved: 864456
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 2969534. Check evaluations: 1471. Alpha cutoffs: 221404. Beta cutoffs: 64788. Priority moves found: 79907.
            // Transpositions used: 124164. Time elapsed: 13357 ms. Available moves found: 40. Transposition tables saved: 1440379

            // 4.2 sec. 24.1.2021 - Keep tabs on alpha&beta values on the minimax launching function
            // Test: RuyLopez_Black. Move: d8 to f6. Depth 5
            // Board evaluations: 152653. Check evaluations: 968. Alpha cutoffs: 2900. Beta cutoffs: 59817. Priority moves found: 32683.
            // Transpositions used: 2800. Time elapsed: 1660 ms. Available moves found: 30. Transposition tables saved: 115932
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 315657. Check evaluations: 1471. Alpha cutoffs: 127543. Beta cutoffs: 4328. Priority moves found: 12446.
            // Transpositions used: 6965. Time elapsed: 2569 ms. Available moves found: 40. Transposition tables saved: 211831

            // 3.7 sec. 16.2.2021 - small move ordering tweak
            // Test: RuyLopez_Black. Move: c6 to d4. Depth 5
            // Board evaluations: 162426. Check evaluations: 967. Alpha cutoffs: 4019. Beta cutoffs: 40293. Priority moves found: 17948.
            // Transpositions used: 3488. Time elapsed: 1570 ms. Available moves found: 30. Transposition tables saved: 127679
            // Test: GreatestEndings_1_MidGame. Move: c4 to d5. Depth 5
            // Board evaluations: 194290. Check evaluations: 1471. Alpha cutoffs: 77853. Beta cutoffs: 3667. Priority moves found: 8410.
            // Transpositions used: 4316. Time elapsed: 2149 ms. Available moves found: 40. Transposition tables saved: 165471
        }

        private void RunAll(int searchDepth, LogicSettings settings)
        {
            // Ruy lopez opening
            RuyLopez_Black(searchDepth, settings);

            // https://thechessworld.com/articles/endgame/7-greatest-chess-endings/
            // #1 at pair23
            GreatestEndings_1_MidGame(searchDepth, settings);

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

        public void RuyLopez_Black(int searchDepth, LogicSettings settings)
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

            player.Settings = settings;

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

        public void GreatestEndings_1_MidGame(int searchDepth, LogicSettings settings)
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

            player.Settings = settings;

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
