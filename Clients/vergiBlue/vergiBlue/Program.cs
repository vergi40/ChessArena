﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using CommonNetStandard;
using CommandLine;
using CommonNetStandard.Client;
using vergiBlue.ConsoleTools;

namespace vergiBlue
{
    class Program
    {
        // Program is singleton static so static properties should be ok
        private static string? _currentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2);
        private static string _playerName { get; set; } = "vergiBlue";
        private static int _gameMode { get; set; }
        private static string _address { get; set; } = ConfigurationManager.AppSettings["Address"] ?? "";
        private static string _port { get; set; } = ConfigurationManager.AppSettings["Port"] ?? "";
        private static int _minimumDelayBetweenMoves { get; set; } = 0;

        private static void Log(string message, bool writeToConsole = true) => Logger.Log(message, writeToConsole);

        private static string _fullAddress => $"{_address}:{_port}"; 

        /// <summary>
        /// Optional arguments given in command line
        /// </summary>
        /// <param name="options"></param>
        static void RunOptions(Options options)
        {
            //handle options
            if (options.GameMode > 0) _gameMode = options.GameMode;
            if (options.PlayerName != "") _playerName = options.PlayerName;
            if (options.Address != "") _address = options.Address;
            if (options.Port != "") _port = options.Port;
            if (options.MinDelay > 0) _minimumDelayBetweenMoves = options.MinDelay;
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            _stopArgsGiven = true;
        }

        /// <summary>
        /// User queried version of info from the app - stop execution
        /// </summary>
        private static bool _stopArgsGiven = false;

        static void Main(string[] args)
        {
            // Given arguments saved to private properties
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);

            if (_stopArgsGiven) return;

            Log($"Chess ai vergiBlue [{_currentVersion}]");

            while (true)
            {
                if (_gameMode <= 0)
                {
                    // User did not explicitly set gamemode in command line arguments
                    _gameMode = InputGameMode();
                }

                if (_gameMode < 0)
                {
                    break;
                }

                if (_gameMode == 1)
                {
                    using var connection = new grpcClientConnection(_fullAddress);
                    NetworkGame.Start(connection, _playerName, false);
                    break;
                }
                else if (_gameMode == 2)
                {
                    Log(Environment.NewLine);
                    Log("Give player name: ");
                    Console.Write(" > "); 
                    var playerName = Console.ReadLine() ?? _playerName;
                    Log($"Chess ai {playerName} [{_currentVersion}]");
                    using var connection = new grpcClientConnection(_fullAddress);
                    NetworkGame.Start(connection, playerName, false);
                    break;
                }
                else if (_gameMode == 3)
                {
                    LocalGame.Start(_minimumDelayBetweenMoves, null);
                }
                else if (_gameMode == 4)
                {
                    LocalGame.Start(Math.Max(1000, _minimumDelayBetweenMoves), null);
                }
                else if (_gameMode == 5)
                {
                    LocalGame.CustomStart();
                }
                else if (_gameMode == 9)
                {
                    using var connection = new grpcClientConnection(_fullAddress);
                    NetworkGame.Start(connection, "Connection test AI", true);
                    break;
                }
                else break;

                Log(Environment.NewLine);
            }
        }

        private static int InputGameMode()
        {
            Log(Options.PrintGameModes());
            //Log("[2] Edit player name and start game");
            //Log("[3] Start local game with two vergiBlues against each other");
            //Log("[4] Start local game with two vergiBlues against each other. Delay between moves");
            //Log("[5] Custom local game");
            //Log("[9] Connection testing game");
            Log("[Any] Exit");

            Console.Write(" > ");
            var input = Console.ReadKey().KeyChar;
            if (char.IsDigit(input))
            {
                return int.Parse(input.ToString());
            }

            // Error
            return -1;
        }
    }
}
