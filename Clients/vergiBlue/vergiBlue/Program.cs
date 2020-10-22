using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using Common;
using Common.Connection;

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
            var connection = new ConnectionModule();

            while (true)
            {
                Log("[1] Start game");
                Log("[2] Edit player name and start game");
                Log("[3] Start local game with two vergiBlues against each other");
                Log("[4] Start local game with two vergiBlues against each other. Delay between moves");
                Log("[5] Connection testing game");
                Log("[6] Exit");

                Console.Write(" > ");
                var input = Console.ReadKey();
                if (input.KeyChar.ToString() == "1")
                {
                    StartGame(connection, _aiName, false);
                }
                else if (input.KeyChar.ToString() == "2")
                {
                    Log(Environment.NewLine);
                    Log("Give player name: ");
                    Console.Write(" > "); 
                    var playerName = Console.ReadLine();
                    Log($"Chess ai {playerName} [{_currentVersion}]");
                    StartGame(connection, playerName, false);
                }
                else if (input.KeyChar.ToString() == "3")
                {
                    StartLocalGame(0);
                }
                else if (input.KeyChar.ToString() == "4")
                {
                    StartLocalGame(1000);
                }
                else if (input.KeyChar.ToString() == "5")
                {
                    StartGame(connection, "Connection test AI", true);
                }
                else break;

                Log(Environment.NewLine);
            }

            connection.CloseConnection();
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

        static void StartLocalGame(int minDelayInMs)
        {
            Log(Environment.NewLine);
            // TODO async
            var info1 = new GameStartInformation() {WhitePlayer = true};

            var player1 = new Logic(info1, false);
            var board = new SimpleBoard(player1.Board);
            
            var firstMove = player1.CreateMove();
            PrintMove(firstMove, "player1");
            PrintBoardAfterMove(firstMove, "", board);

            var info2 = new GameStartInformation() {WhitePlayer = false, OpponentMove = firstMove.Move};
            var player2 = new Logic(info2, false);
            try
            {

                while (true)
                {
                    var move = player2.CreateMove();
                    PrintMove(move, "player2");
                    PrintBoardAfterMove(move, "", board);
                    if (move.Move.CheckMate) break;
                    player1.ReceiveMove(move.Move);
                    Thread.Sleep(minDelayInMs);

                    move = player1.CreateMove();
                    PrintMove(move, "player1");
                    PrintBoardAfterMove(move, "", board);
                    if (move.Move.CheckMate) break;
                    player2.ReceiveMove(move.Move);
                    Thread.Sleep(minDelayInMs);
                }

                Log("Checkmate");
                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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

            // Clear previous move
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board.Get((i, j)) == 666)
                    {
                        board.Set((i, j), 0);
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
        public const int PreviousTileValue = 666;

        public int[,] Tiles { get; set; }
        public SimpleBoard(Board board)
        {
            Tiles = new int[8,8];
            foreach (var piece in board.PieceList)
            {
                Set(piece.CurrentPosition, (int)piece.RelativeStrength);
            }
        }

        public int Get((int, int) target)
        {
            return Tiles[target.Item1, target.Item2];
        }

        public void Set((int, int) target, int strength)
        {
            Tiles[target.Item1, target.Item2] = strength;
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

        private string DrawPiece(int value)
        {
            if (value == 0) return "   ";

            var icon = "";
            if (Math.Abs(value) == StrengthTable.Pawn)
            {
                icon += "P";
            }
            else if (Math.Abs(value) == StrengthTable.Bishop)
            {
                icon += "B";
            }
            else if (Math.Abs(value) == StrengthTable.Rook)
            {
                icon += "R";
            }
            else if (Math.Abs(value) == StrengthTable.Knight)
            {
                icon += "N";
            }
            else if (Math.Abs(value) == StrengthTable.King)
            {
                icon += "K";
            }
            else if (Math.Abs(value) == StrengthTable.Queen)
            {
                icon += "Q";
            }
            else if (value == PreviousTileValue)
            {
                return "[ ]";
            }

            if (value > 0)
            {
                // 0-255
                var whiteBackground = "\x1b[48;5;255m";
                var blackForeground = "\x1b[38;5;0m";
                icon = whiteBackground + blackForeground + " w" + icon;
            }
            else
            {
                var blackBackground = "\x1b[48;5;0m";
                var whiteForeground = "\x1b[38;5;255m";
                icon = blackBackground + whiteForeground + " b" + icon;
            }
            return icon;
        }
    }
}
