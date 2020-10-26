using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using CommonNetStandard;
using CommonNetStandard.Connection;
using vergiBlue.Algorithms;

namespace vergiBlue
{
    class Program
    {
        // Console coloring
        // https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);


        private static string _currentVersion = "v0.02";
        private static string _aiName = "vergiBlue";

        private static void Log(string message, bool writeToConsole = true) => Logger.Log(message, writeToConsole);

        static void Main(string[] args)
        {
            Console.SetWindowSize(180, 40);
            var handle = GetStdHandle(-11);
            int mode;
            GetConsoleMode(handle, out mode);
            SetConsoleMode(handle, mode | 0x4);


            Log($"Chess ai {_aiName} [{_currentVersion}]");

            while (true)
            {
                Log("[1] Start game");
                Log("[2] Edit player name and start game");
                Log("[3] Start local game with two vergiBlues against each other");
                Log("[4] Start local game with two vergiBlues against each other. Delay between moves");
                Log("[5] Custom local game");
                Log("[9] Connection testing game");
                Log("[Any] Exit");

                Console.Write(" > ");
                var input = Console.ReadKey();
                if (input.KeyChar.ToString() == "1")
                {
                    var connection = new ConnectionModule();
                    StartGame(connection, _aiName, false);
                    connection.CloseConnection();
                }
                else if (input.KeyChar.ToString() == "2")
                {
                    Log(Environment.NewLine);
                    Log("Give player name: ");
                    Console.Write(" > "); 
                    var playerName = Console.ReadLine();
                    Log($"Chess ai {playerName} [{_currentVersion}]");
                    var connection = new ConnectionModule();
                    StartGame(connection, playerName, false);
                    connection.CloseConnection();
                }
                else if (input.KeyChar.ToString() == "3")
                {
                    StartLocalGame(0, null);
                }
                else if (input.KeyChar.ToString() == "4")
                {
                    StartLocalGame(1000, null);
                }
                else if (input.KeyChar.ToString() == "5")
                {
                    CustomLocalGame();
                }
                else if (input.KeyChar.ToString() == "9")
                {
                    var connection = new ConnectionModule();
                    StartGame(connection, "Connection test AI", true);
                    connection.CloseConnection();
                }
                else break;

                Log(Environment.NewLine);
            }
        }

        static void CustomLocalGame()
        {
            Log(Environment.NewLine);
            Log("[1] Normal game with delay and more dum opponent");
            Log("[Any] Exit");

            Console.Write(" > ");
            var input = Console.ReadKey();
            if (input.KeyChar.ToString() == "1")
            {
                StartLocalGame(1000, 3);
            }
        }

        static void StartGame(ConnectionModule connection, string playerName, bool connectionTesting)
        {
            Log(Environment.NewLine);
            // TODO async
            var startInformation = connection.Initialize(GetAddress(), playerName);
            startInformation.Wait();

            Log($"Received game start information.");
            if (startInformation.Result.WhitePlayer) Log($"{_aiName} starts the game.");
            else Log($"Opponent starts the game.");

            Log(Environment.NewLine);

            Log("Starting logic...");
            var ai = new Logic(startInformation.Result, connectionTesting);

            Log("Start game loop");

            // Inject ai to connection module and play game
            var playTask = connection.Play(ai);
            playTask.Wait();
        }

        static void StartLocalGame(int minDelayInMs, int? overrideOpponentMaxDepth)
        {
            Log(Environment.NewLine);
            // TODO async
            var moveHistory = new List<PlayerMove>();
            var info1 = new GameStartInformation() {WhitePlayer = true};

            var player1 = new Logic(info1, false);
            var board = new SimpleBoard(player1.Board);
            
            var firstMove = player1.CreateMove();
            moveHistory.Add(firstMove);
            PrintMove(firstMove, "white");
            PrintBoardAfterMove(firstMove, "", board);

            var info2 = new GameStartInformation() {WhitePlayer = false, OpponentMove = firstMove.Move};
            var player2 = new Logic(info2, false, overrideOpponentMaxDepth);
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


        static void PrintMove(PlayerMove move, string playerName)
        {
            var message = $"{playerName.PadRight(10)}- {move.Move.StartPosition} to ";
            //if (move.Move.Capture) info += "x";
            message += $"{move.Move.EndPosition}";
            if (move.Move.Check) message += "+";
            if (move.Move.CheckMate) message += "#";
            message += ". ";
            //message += Environment.NewLine;
            message += $"{move.Diagnostics}";
            Log( message);
        }

        static void PrintBoardAfterMove(PlayerMove move, string playerName, SimpleBoard board)
        {
            var piece = board.Get(move.Move.StartPosition.ToTuple());
            if (move.Move.PromotionResult != Move.Types.PromotionPieceType.NoPromotion)
            {
                if (piece.Contains("w")) piece = "wQ ";
                else piece = "bQ ";
            }

            // Clear previous move
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board.Get((i, j)) == SimpleBoard.PreviousTileValue)
                    {
                        board.Set((i, j), "   ");
                        break;
                    }
                }
            }

            board.Set(move.Move.StartPosition.ToTuple(), SimpleBoard.PreviousTileValue);
            board.Set(move.Move.EndPosition.ToTuple(), piece);

            board.Print();
        }

        static string GetAddress()
        {
            return ConfigurationManager.AppSettings["Address"] + ":" + ConfigurationManager.AppSettings["Port"];
        }
    }

    class SimpleBoard
    {
        public const string PreviousTileValue = "[ ]";

        public string[,] Tiles { get; set; }
        public SimpleBoard(Board board)
        {
            Tiles = new string[8,8];
            foreach (var piece in board.PieceList)
            {
                var color = 'w';
                if (!piece.IsWhite) color = 'b';
                Set(piece.CurrentPosition, color.ToString() + piece.Identity.ToString() + " ");
            }
        }

        public string Get((int, int) target)
        {
            return Tiles[target.Item1, target.Item2];
        }

        public void Set((int, int) target, string identity)
        {
            Tiles[target.Item1, target.Item2] = identity;
        }

        public void Print()
        {
            // 
            var blackBackground = "\x1b[48;5;0m";
            var whiteForeground = "\x1b[38;5;255m";
            
            for (int row = 7; row >= 0; row--)
            {
                var columnString = $"{row + 1}| ";
                for (int column = 0; column < 8; column++)
                {
                    columnString += DrawPiece(Get((column, row)));
                    columnString += blackBackground + whiteForeground;
                }
                Logger.Log(columnString);
            }
            Logger.Log("    A  B  C  D  E  F  G  H ");
        }

        private string DrawPiece(string value)
        {
            if (string.IsNullOrEmpty(value)) return "   ";
            if (value == PreviousTileValue)
            {
                return value;
            }

            // Console coloring magic
            // https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
            if (value.Contains("w"))
            {
                // 0-255
                var whiteBackground = "\x1b[48;5;255m";
                var blackForeground = "\x1b[38;5;0m";
                value = whiteBackground + blackForeground + value;
            }
            else
            {
                var blackBackground = "\x1b[48;5;0m";
                var whiteForeground = "\x1b[38;5;255m";
                value = blackBackground + whiteForeground + value;
            }
            return value;
        }
    }
}
