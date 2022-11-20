using CommandLine;
using CommandLine.Text;

namespace vergiBlueConsole.ConsoleTools
{
    // https://github.com/commandlineparser/commandline

    [Verb("uci", HelpText = "Read given file line by line for uci input stream")]
    internal class UciOptions
    {
        [Value(0, MetaName = "filePath", Required = true, HelpText = "Uci input stream file")]
        public string FilePath { get; set; }
    }

    [Verb("chessarena", HelpText = "ChessArena implementation. See help for configurations and commands")]
    internal class ChessArenaOptions
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
                        new ChessArenaOptions { GameMode  = 1, Address = "127.0.0.1", Port = "30052"}),

                    new Example("Start network game in given url and port with custom player name",
                        new ChessArenaOptions { GameMode  = 1, Address = "127.0.0.1", Port = "30052", PlayerName = "whiteplayer"})
                };
            }
        }
    }
}
