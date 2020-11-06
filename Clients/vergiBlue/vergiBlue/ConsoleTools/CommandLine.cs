using System;
using CommandLine;

namespace vergiBlue.ConsoleTools
{
    /// <summary>
    /// https://github.com/commandlineparser/commandline
    /// </summary>
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('g', "gamemode", Required = false, HelpText = "Select the game mode. [1] Normal game. [3] Local game.")]
        public int GameMode { get; set; }

        [Option('n', "playername", Required = false, HelpText = "Set player name.")]
        public string? PlayerName { get; set; }

        [Option('a', "address", Required = false, HelpText = "Set chessarena server ip address.")]
        public string? Address { get; set; }

        [Option('p', "port", Required = false, HelpText = "Set chessarena server port.")]
        public string? Port { get; set; }


        public static string PrintGameModes()
        {
            var info = "[1] Start game" + Environment.NewLine
                                        + "[2] Edit player name and start game" + Environment.NewLine
                                        + "[3] Start local game with two vergiBlues against each other" + Environment.NewLine
                                        + "[4] Start local game with two vergiBlues against each other. Delay between moves" + Environment.NewLine
                                        + "[5] Custom local game" + Environment.NewLine
                                        + "[9] Connection testing game";
            return info;
        }
    }
}
