using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CommonNetStandard.Interface;
using NUnit.Framework;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace IntegrationTests
{
    [TestFixture]
    public class EndGameTests
    {
        /// <summary>
        /// This game situation is where most of regression bugs appear
        /// </summary>
        [Test]
        [Ignore("Ignore until endgame evals perfected")]
        public void DoubleRook_DesktopCase_ShouldPlayTillCheckMate()
        {
            // 8   k
            // 7   r
            // 6
            // 5   r
            // 4
            // 3
            // 2    K
            // 1
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new Rook(false, "d5"),
                new Rook(false, "d7"),
                new King(true, "e2"),
                new King(false, "d8")
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var white = LogicFactory.CreateForTest(true, board);
            var black = LogicFactory.CreateForTest(false, board);

            var timeLimit = 2000;
            var turnLimit = 30;
            var settings = new LogicSettings() {TimeLimitInMs = timeLimit};
            white.Settings = settings;
            black.Settings = settings;
            
            PlayUntilEndAssert(white, black, timeLimit, turnLimit);
        }

        private void PlayUntilEndAssert(Logic white, Logic black, int timeLimit, int turnLimit)
        {
            bool checkMate = false;
            for (int i = 0; i < turnLimit; i++)
            {
                var whiteMove = CreateMoveAndThrowIfTimeExceeded(white, timeLimit);
                if (whiteMove.CheckMate)
                {
                    checkMate = true;
                    break;
                }

                black.ReceiveMove(whiteMove);


                var blackMove = CreateMoveAndThrowIfTimeExceeded(black, timeLimit);
                if (blackMove.CheckMate)
                {
                    checkMate = true;
                    break;
                }

                white.ReceiveMove(blackMove);
            }

            Assert.IsTrue(checkMate, $"Game didn't end after {turnLimit} turns");
        }

        private IMove CreateMoveAndThrowIfTimeExceeded(Logic logic, int timeLimit)
        {
            var timer = new Timer(timeLimit + 100);
            timer.Elapsed += async (sender, e) => await ThrowIfExceeded();
            timer.Start();
            var move = logic.CreateMove();

            timer.Close();

            var player = logic.IsPlayerWhite ? "white" : "black";
            Debug.WriteLine($"{player}: {move.Move}");
            return move.Move;
        }

        private static Task ThrowIfExceeded()
        {
            throw new TimeoutException("Didn't receive move in configured timeout");
        }


        [Test, Timeout(Utils.TestTimeoutMs)]
        public void DoubleRook_Distance1_ShouldCheckMateGracefully()
        {
            // Start situation
            // 8       K  
            // 7 r 
            // 6r      
            // 5    
            // 4
            // 3
            // 2
            // 1       k
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "h8"),
                new King(false, "a8"),
                new Rook(false, "b7"),
                new Rook(false, "a6"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var white = LogicFactory.CreateForTest(true, board);
            var black = LogicFactory.CreateForTest(false, board);

            var timeLimit = 2000;
            var turnLimit = 5;
            var settings = new LogicSettings() { TimeLimitInMs = timeLimit };
            white.Settings = settings;
            black.Settings = settings;

            PlayUntilEndAssert(white, black, timeLimit, turnLimit);
        }

        [Test]
        [Ignore("Ignore until endgame evals perfected")]
        public void DoubleRook_Distance2_ShouldCheckMateGracefully()
        {
            // Start situation
            // 8       K  
            // 7  
            // 6r      
            // 5 r   
            // 4
            // 3
            // 2
            // 1       k
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "h8"),
                new King(false, "a8"),
                new Rook(false, "b5"),
                new Rook(false, "a6"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var white = LogicFactory.CreateForTest(true, board);
            var black = LogicFactory.CreateForTest(false, board);

            var timeLimit = 2000;
            var turnLimit = 7;
            var settings = new LogicSettings() { TimeLimitInMs = timeLimit };
            white.Settings = settings;
            black.Settings = settings;

            PlayUntilEndAssert(white, black, timeLimit, turnLimit);
        }

        [Test]
        [Ignore("Ignore until endgame evals perfected")]
        public void DoubleRook_Distance3_ShouldCheckMateGracefully()
        {
            // Start situation
            // 8       K  
            // 7 
            // 6     
            // 5 r   
            // 4r
            // 3
            // 2
            // 1       k
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "h8"),
                new King(false, "a8"),
                new Rook(false, "b5"),
                new Rook(false, "a4"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var white = LogicFactory.CreateForTest(true, board);
            var black = LogicFactory.CreateForTest(false, board);

            var timeLimit = 2000;
            var turnLimit = 9;
            var settings = new LogicSettings() { TimeLimitInMs = timeLimit };
            white.Settings = settings;
            black.Settings = settings;

            PlayUntilEndAssert(white, black, timeLimit, turnLimit);
        }
    }
}
