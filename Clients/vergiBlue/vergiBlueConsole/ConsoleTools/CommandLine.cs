using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandLine;
using CommandLine.Text;

namespace vergiBlue.ConsoleTools
{
    /// <summary>
    /// https://github.com/commandlineparser/commandline
    /// </summary>
    public class Options
    {
        [Option('g', "gamemode", Required = false, HelpText = "Select the game mode. [1] Network game. [3] Local game. [5] Custom local game.")]
        public int GameMode { get; set; }

        [Option('n', "playername", Required = false, HelpText = "Set player name. Default name 'vergiBlue'")]
        public string PlayerName { get; set; } = "";//Usage-attribute does not support nullable properties yet

        [Option('r', "address", Required = false, HelpText = "Set ChessArena server url address. Default address in app.config.")]
        public string Address { get; set; } = "";//Usage-attribute does not support nullable properties yet

        [Option('p', "port", Required = false, HelpText = "Set ChessArena server port. Default port in app.config.")]
        public string Port { get; set; } = "";//Usage-attribute does not support nullable properties yet

        [Option('m', "mindelay", Required = false, HelpText = "Set minimum delay (ms) between moves to be used in local games. Default value is 1000 ms.")]
        public int MinDelay { get; set; }

        [Usage(ApplicationAlias = "vergiBlue")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Start network game in given url and port",
                        new Options { GameMode  = 1, Address = "127.0.0.1", Port = "30052"}),

                    new Example("Start network game in given url and port with custom player name",
                        new Options { GameMode  = 1, Address = "127.0.0.1", Port = "30052", PlayerName = "whiteplayer"})
                };
            }
        }


        public static string PrintGameModes()
        {
            var info = "[1] Start network game" + Environment.NewLine
                                        + "[2] Edit player name and start network game" + Environment.NewLine
                                        + "[3] Start local game with two vergiBlues against each other" + Environment.NewLine
                                        + "[4] Start local game with two vergiBlues against each other. Delay between moves" + Environment.NewLine
                                        + "[5] Custom local game" + Environment.NewLine
                                        + "[9] Connection testing game";
            return info;
        }
    }
}
