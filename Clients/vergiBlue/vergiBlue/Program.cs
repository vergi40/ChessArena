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
        private static string _currentVersion = "v0.01";
        private static string _aiName = "vergiBlue";

        private static void Log(string message, bool writeToConsole = true) => Logger.Log(message, writeToConsole);

        static void Main(string[] args)
        {
            Log($"Chess ai vergiBlue [{_currentVersion}]");
            var connection = new ConnectionModule(_aiName);

            while (true)
            {
                Log("[1] Start game");
                Log("[2] Exit");

                var input = Console.ReadKey();
                if (input.KeyChar.ToString() == "1")
                {
                    Log(Environment.NewLine);
                    // TODO async
                    var startInformation = connection.Initialize(GetAddress());

                    Log($"Received game start information.");
                    if(startInformation.Result.WhitePlayer) Log($"{_aiName} starts the game.");
                    else Log($"Opponent starts the game.");

                    Log(Environment.NewLine);

                    Log("Starting logic...");
                    var ai = new Logic(startInformation.Result);
                    
                    Log("Start game loop");

                    // Inject ai to connection module and play game
                    connection.Play(ai);

                }
                else break;

                Log(Environment.NewLine);
            }

            connection.CloseConnection();
        }

        static string GetAddress()
        {
            return ConfigurationManager.AppSettings["Address"] + ":" + ConfigurationManager.AppSettings["Port"];
        }
    }
}
