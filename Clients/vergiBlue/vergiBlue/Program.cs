using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Common;
using Common.Connection;

namespace vergiBlue
{
    class Program
    {
        private static string _currentVersion = "v0.02";
        private static string _aiName = "vergiBlue";

        private static void Log(string message, bool writeToConsole = true) => Logger.Log(message, writeToConsole);

        static void Main(string[] args)
        {
            Log($"Chess ai {_aiName} [{_currentVersion}]");
            var connection = new ConnectionModule();

            while (true)
            {
                Log("[1] Start game");
                Log("[2] Edit player name and start game");
                Log("[3] Connection testing game");
                Log("[4] Exit");

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

        static string GetAddress()
        {
            return ConfigurationManager.AppSettings["Address"] + ":" + ConfigurationManager.AppSettings["Port"];
        }
    }
}
