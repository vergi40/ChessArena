using System.Net.NetworkInformation;
using CommonNetStandard.Client;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using vergiBlue;
using vergiBlue.ConsoleTools;
using vergiBlueConsole.ConsoleTools;
using System.Reflection;
using System.Security.Cryptography;

namespace vergiBlueConsole
{
    internal class ChessArena
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<Program>();
        private static void Log(string message) => _logger.LogInformation(message);

        private string _playerName { get; set; } = "vergiBlue";
        private int _gameMode { get; set; }

        /// <summary>
        /// Use http prefix for insecure connection (ok for local testing)
        /// </summary>
        private string _address { get; set; } = "http://localhost";
        private string _port { get; set; } = "30052";
        private int _minimumDelayBetweenMoves { get; set; } = 0;

        private string _fullAddress => $"{_address}:{_port}";

        private void LoadConfiguration()
        {
            // https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration#basic-example
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var settingsSection = config.GetRequiredSection("Settings");
            _address = settingsSection["Address"] ?? "";
            _port = settingsSection["Port"] ?? "";
        }

        public void Run(ChessArenaOptions options)
        {
            LoadConfiguration();

            // Override with given args
            if (options.GameMode > 0) _gameMode = options.GameMode;
            if (options.PlayerName != "") _playerName = options.PlayerName;
            if (options.Address != "") _address = options.Address;
            if (options.Port != "") _port = options.Port;
            if (options.MinDelay > 0) _minimumDelayBetweenMoves = options.MinDelay;

            RunGameLoop();
        }

        /// <summary>
        /// Ask user for game mode etc
        /// </summary>
        public void RunWithoutOptions()
        {
            LoadConfiguration();
            RunGameLoop();
        }

        private void RunGameLoop()
        {
            Log($"Chess ai vergiBlue v{Program.GetVergiBlueVersion()}");

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
                    using var connection = GrpcClientConnectionFactory.Create(_fullAddress);
                    NetworkGame.Start(connection, _playerName, false);
                    break;
                }
                else if (_gameMode == 2)
                {
                    Log(Environment.NewLine);
                    Log("Give player name: ");
                    Console.Write(" > ");
                    var playerName = Console.ReadLine() ?? _playerName;
                    using var connection = GrpcClientConnectionFactory.Create(_fullAddress);
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
                else if (_gameMode == 8)
                {
                    using var connection = GrpcClientConnectionFactory.Create(_fullAddress);
                    var randomId = RandomNumberGenerator.GetInt32(1000);
                    NetworkGame.Ping(connection, $"Client pinger id{randomId}");
                    break;
                }
                else if (_gameMode == 9)
                {
                    using var connection = GrpcClientConnectionFactory.Create(_fullAddress);
                    NetworkGame.Start(connection, "Connection test AI", true);
                    break;
                }
                else break;

                Log(Environment.NewLine);
            }
        }

        private static int InputGameMode()
        {
            Log(PrintGameModes());
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



        private static string PrintGameModes()
        {
            var info = "[1] Start network game" + Environment.NewLine
                                                + "[2] Edit player name and start network game" + Environment.NewLine
                                                + "[3] Start local game with two vergiBlues against each other" + Environment.NewLine
                                                + "[4] Start local game with two vergiBlues against each other. Delay between moves" + Environment.NewLine
                                                + "[5] Custom local game" + Environment.NewLine
                                                + "[8] Ping test to GameManager"
                                                + "[9] Connection testing game";
            return info;
        }
    }
}
