using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CommonNetStandard.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace UnitTests
{
    [TestClass]
    public class IntegrationTests
    {
        public void ClearCheckMate_ShouldEndGracefully()
        {
            //
        }

        /// <summary>
        /// This game situation is where most of regression bugs appear
        /// </summary>
        [TestMethod]
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

            checkMate.ShouldBeTrue($"Game didn't end after {turnLimit} turns");
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

    }
}
