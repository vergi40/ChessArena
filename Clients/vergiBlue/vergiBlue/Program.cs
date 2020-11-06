using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using CommonNetStandard;
using CommonNetStandard.Connection;
using CommandLine;
using vergiBlue.ConsoleTools;

namespace vergiBlue
{
    class Program
    {
        private static string? _currentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        private static string _aiName = "vergiBlue";

        private static void Log(string message, bool writeToConsole = true) => Logger.Log(message, writeToConsole);
        static void RunOptions(Options options)
        {
            //handle options
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            _stopArgsGiven = true;
        }

        private static bool _stopArgsGiven = false;

        static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);

            if (_stopArgsGiven) return;

            Log($"Chess ai vergiBlue [{_currentVersion}]");

            while (true)
            {
                Log(Options.PrintGameModes());
                //Log("[2] Edit player name and start game");
                //Log("[3] Start local game with two vergiBlues against each other");
                //Log("[4] Start local game with two vergiBlues against each other. Delay between moves");
                //Log("[5] Custom local game");
                //Log("[9] Connection testing game");
                Log("[Any] Exit");

                Console.Write(" > ");
                var input = Console.ReadKey();
                if (input.KeyChar.ToString() == "1")
                {
                    var connection = new ConnectionModule(GetAddress());
                    NetworkGame.Start(connection, _aiName, false);
                    connection.CloseConnection();
                }
                else if (input.KeyChar.ToString() == "2")
                {
                    Log(Environment.NewLine);
                    Log("Give player name: ");
                    Console.Write(" > "); 
                    var playerName = Console.ReadLine() ?? "testplayer";
                    Log($"Chess ai {playerName} [{_currentVersion}]");
                    var connection = new ConnectionModule(GetAddress());
                    NetworkGame.Start(connection, playerName, false);
                    connection.CloseConnection();
                }
                else if (input.KeyChar.ToString() == "3")
                {
                    LocalGame.Start(0, null);
                }
                else if (input.KeyChar.ToString() == "4")
                {
                    LocalGame.Start(1000, null);
                }
                else if (input.KeyChar.ToString() == "5")
                {
                    LocalGame.CustomStart();
                }
                else if (input.KeyChar.ToString() == "9")
                {
                    var connection = new ConnectionModule(GetAddress());
                    NetworkGame.Start(connection, "Connection test AI", true);
                    connection.CloseConnection();
                }
                else break;

                Log(Environment.NewLine);
            }
        }

        static string GetAddress()
        {
            return ConfigurationManager.AppSettings["Address"] + ":" + ConfigurationManager.AppSettings["Port"];
        }
    }
}
