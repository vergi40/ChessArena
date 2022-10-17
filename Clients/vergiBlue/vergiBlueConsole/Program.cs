using System.Diagnostics;
using System.Reflection;
using CommandLine;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlueConsole;
using vergiBlueConsole.ConsoleTools;
using vergiBlueConsole.UciMode;

namespace vergiBlue
{
    enum ArgsResult
    {
        /// <summary>
        /// Continue with interactive mode, let user choose actions to take
        /// </summary>
        NoArguments,
        UciVerb,
        ChessArenaVerb,
        BadArguments,
        Error
    }

    class Program
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<Program>();

        /// <summary>
        /// User queried version of info from the app - stop execution
        /// </summary>
        private static bool _stopArgsGiven = false;

        static void Main(string[] args)
        {
            if(!args.Any())
            {
                RunNoArguments();
                return;
            }

            var parseResult = Parser.Default.ParseArguments<UciOptions, ChessArenaOptions>(args);
            var mapResult = parseResult.MapResult(
                (UciOptions opts) =>
                {
                    // Start uci game with file inputs
                    var path = opts.FilePath;
                    if (File.Exists(path))
                    {
                        var input = File.OpenText(path);
                        var firstLine = input.ReadLine();
                        if (firstLine != null && firstLine.Equals("uci"))
                        {
                            Uci.Run(input);
                        }
                        else
                        {
                            Console.WriteLine("Input file should start with line containing text \"uci\"");
                        }
                    }

                    return ArgsResult.UciVerb;
                },
                (ChessArenaOptions opts) =>
                {
                    var instance = new ChessArena();
                    instance.Run(opts);
                    return ArgsResult.ChessArenaVerb;
                },
                _ => ArgsResult.NoArguments);
            
        }

        static void RunNoArguments()
        {
            Debug.WriteLine("Start console by selecting mode:");
            Debug.WriteLine("  uci");
            Debug.WriteLine("  arena");
            var startCommand = Console.ReadLine();

            if (startCommand != null && startCommand.Equals("uci"))
            {
                Uci.Run();
            }
            else if (startCommand != null && startCommand.ToLower().Equals("arena"))
            {
                var instance = new ChessArena();
                instance.RunWithoutOptions();
                Console.WriteLine("Stop by pressing any key...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Unknown command");
                Console.WriteLine("Stop by pressing any key...");
                Console.ReadKey();
            }
            // 
        }

        internal static string GetVergiBlueVersion()
        {
            try
            {
                var assembly = AssemblyName.GetAssemblyName("vergiBlue.dll");
                var version = assembly?.Version?.ToString(3);
                return version ?? "0.0";
            }
            catch (Exception)
            {
                return "0.0";
            }
        }

        internal static string GetConsoleVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly?.GetName()?.Version?.ToString(3);
            return version ?? "0.0";
        }
    }
}
