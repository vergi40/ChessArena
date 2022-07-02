using System;
using System.Collections.Generic;
using System.Threading;
using CommonNetStandard;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue.Algorithms;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;


namespace vergiBlue.ConsoleTools
{
    class LocalGame
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<LocalGame>();
        private static void Log(string message) => _logger.LogInformation(message);

        public static void Start(int minDelayInMs, int? overrideOpponentMaxDepth, IBoard? overrideBoard = null)
        {
            _logger.LogInformation(Environment.NewLine);
            // TODO async
            var moveHistory = new List<IPlayerMove>();
            var info1 = new StartInformationImplementation() { WhitePlayer = true };

            var player1 = LogicFactory.Create(info1, null, overrideBoard);
            var board = new BoardPrinter(player1.Board.InterfacePieces, OperatingSystem.IsWindows());

            var firstMove = player1.CreateMove();
            moveHistory.Add(firstMove);
            PrintMove(firstMove, "white");
            PrintBoardAfterMove(firstMove, "", board);

            var info2 = new StartInformationImplementation() { WhitePlayer = false, OpponentMove = firstMove.Move };
            var player2 = LogicFactory.Create(info2, overrideOpponentMaxDepth, overrideBoard);
            try
            {

                while (true)
                {
                    var move = player2.CreateMove();
                    moveHistory.Add(move);
                    PrintMove(move, "black");
                    PrintBoardAfterMove(move, "", board);
                    if (move.Move.CheckMate)
                    {
                        Log("Checkmate");
                        break;
                    }
                    if (MoveHistory.IsDraw(moveHistory))
                    {
                        Log("Draw");
                        break;
                    }
                    player1.ReceiveMove(move.Move);
                    Thread.Sleep(minDelayInMs);

                    move = player1.CreateMove();
                    moveHistory.Add(move);
                    PrintMove(move, "white");
                    PrintBoardAfterMove(move, "", board);
                    if (move.Move.CheckMate)
                    {
                        Log("Checkmate");
                        break;
                    }
                    if (MoveHistory.IsDraw(moveHistory))
                    {
                        Log("Draw");
                        break;
                    }
                    player2.ReceiveMove(move.Move);
                    Thread.Sleep(minDelayInMs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.Read();
        }


        public static void CustomStart()
        {
            Log(Environment.NewLine);
            Log("[1] Normal game with delay and more dum opponent");
            Log("[2] Endgame. QR vs -");
            Log("[3] Endgame. RR vs R");
            Log("[4] Endgame. PPPP vs PPPP");
            Log("[Any] Exit");

            Console.Write(" > ");
            var input = Console.ReadKey();
            if (input.KeyChar.ToString() == "1")
            {
                Start(2000, 3);
            }
            else if (input.KeyChar.ToString() == "2")
            {
                var board = BoardFactory.CreateEmptyBoard();
                var pieces = new List<PieceBase>
                {
                    new Rook(true, "a1"),
                    new Queen(true, "h1")
                };
                board.AddNew(pieces);
                // 
                var whiteKing = new King(true, "e1");
                board.AddNew(whiteKing);

                var blackKing = new King(false, "e8");
                board.AddNew(blackKing);

                Start(1500, null, board);
            }
            else if (input.KeyChar.ToString() == "3")
            {
                var board = BoardFactory.CreateEmptyBoard();
                var pieces = new List<PieceBase>
                {
                    new Rook(true, "a1"),
                    new Rook(true, "h1"),
                    new Rook(false, "d8")
                };
                board.AddNew(pieces);
                // 
                var whiteKing = new King(true, "e1");
                board.AddNew(whiteKing);

                var blackKing = new King(false, "e8");
                board.AddNew(blackKing);

                Start(800, null, board);
            }
            else if (input.KeyChar.ToString() == "4")
            {
                var board = BoardFactory.CreateEmptyBoard();
                var pieces = new List<PieceBase>
                {
                    new Pawn(true, "c2"),
                    new Pawn(true, "d2"),
                    new Pawn(true, "e2"),
                    new Pawn(true, "f2"),
                    new Pawn(false, "c7"),
                    new Pawn(false, "d7"),
                    new Pawn(false, "e7"),
                    new Pawn(false, "f7")
                };
                board.AddNew(pieces);
                // 
                var whiteKing = new King(true, "e1");
                board.AddNew(whiteKing);

                var blackKing = new King(false, "e8");
                board.AddNew(blackKing);

                Start(1000, null, board);
            }
        }

        static void PrintMove(IPlayerMove move, string playerName)
        {
            var message = $"{playerName.PadRight(10)}- {move.Move.StartPosition} to ";
            //if (move.Move.Capture) info += "x";
            message += $"{move.Move.EndPosition}";
            if (move.Move.CheckMate) message += "#";
            else if (move.Move.Check) message += "+";
            message += ". ";
            //message += Environment.NewLine;
            message += $"{move.Diagnostics}";
            Log(message);
        }

        static void PrintBoardAfterMove(IPlayerMove move, string playerName, BoardPrinter board)
        {
            var piece = board.Get(move.Move.StartPosition.ToTuple());
            if (move.Move.PromotionResult != PromotionPieceType.NoPromotion)
            {
                if (piece.Contains("w")) piece = "wQ ";
                else piece = "bQ ";
            }

            // Clear previous move
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board.Get((i, j)) == BoardPrinter.PreviousTileValue)
                    {
                        board.Set((i, j), "   ");
                        break;
                    }
                }
            }

            board.Set(move.Move.StartPosition.ToTuple(), BoardPrinter.PreviousTileValue);
            board.Set(move.Move.EndPosition.ToTuple(), piece);

            board.Print();
        }
    }
}
