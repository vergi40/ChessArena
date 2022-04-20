using System.Reflection;
using CommonNetStandard.Common;
using log4net;
using vergiBlue.Logic;

namespace vergiBlueConsole.UciMode
{
    /// <summary>
    /// Discuss with chess gui using standard input/output stream
    /// </summary>
    internal class Uci
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Uci));

        public static void Run()
        {
            WriteLine($"id name vergiBlue v{GetVergiBlueVersion()}, console v{GetConsoleVersion()}");
            WriteLine("id author Teemu Laine");

            WriteLine("uciok");

            if (!RunOptions()) return;

            // Initialize logic
            var logic = LogicFactory.CreateForUci();
            WriteLine("readyok");

            while (true)
            {
                var gameCommand = ReadLine();
                if (gameCommand.Equals("isready"))
                {
                    // Always answer isready ping immediately, even though there is search etc. going
                    WriteLine("readyok");
                }
                else if (gameCommand.Equals("ucinewgame"))
                {
                    // In startup: optional
                    // After game x: should clear all and get ready for new game
                }
                else if (gameCommand.Contains("position"))
                {
                    var (startPosOrFenBoard, moves) = InputSupport.ReadUciPosition(gameCommand);

                    // Create board defined
                    logic.SetBoard(startPosOrFenBoard, moves);
                }
                else if (gameCommand.Contains("go"))
                {
                    var parameters = InputSupport.ReadGoParameters(gameCommand);

                    if (!RunSearch(logic, parameters)) return;
                }
                else if (gameCommand.Contains("stop"))
                {
                    // TODO
                }
                else if (gameCommand.Contains("ponderhit"))
                {
                    // TODO
                }
                else if (gameCommand.Equals("exit"))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>False - exit</returns>
        private static bool RunOptions()
        {
            while (true)
            {
                var nextInput = ReadLine();
                // When adding available options, set here

                if (nextInput.Equals("isready"))
                {
                    break;
                }
                else if (nextInput.Equals("exit"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// </summary>
        /// <returns>False - exit</returns>
        private static bool RunSearch(Logic logic, UciGoParameters parameters)
        {
            var cancellationSource = new CancellationTokenSource();
            var searchTask = Task.Run(() => logic.CreateSearchTask(parameters, SearchInfoUpdate, cancellationSource.Token));

            while (true)
            {
                var gameCommand = ReadLine();
                if (gameCommand.Equals("isready"))
                {
                    // Always answer isready ping immediately, even though there is search etc. going
                    WriteLine("readyok");
                }
                else if (gameCommand.Contains("stop"))
                {
                    cancellationSource.Cancel();
                    var result = searchTask.Result;

                    // info before bestmove
                    WriteLine($"bestmove {result.BestMove.ToCompactString()}");

                    break;
                }
                else if (gameCommand.Equals("exit"))
                {
                    return false;
                }
            }

            return true;
        }
        // RunPonder loop

        private static void SearchInfoUpdate(string message)
        {
            WriteLine(message);
        }

        private static string GetVergiBlueVersion()
        {
            var assembly = AssemblyName.GetAssemblyName("vergiBlue.dll");
            var version = assembly.Version.ToString(3);
            return version;
        }

        private static string GetConsoleVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version.ToString(3);
            return version;
        }

        private static void WriteLine(string message)
        {
            _logger.Info(message);
            Console.WriteLine(message);
        }

        private static string ReadLine()
        {
            var line = Console.ReadLine();
            if (line == null)
            {
                throw new ArgumentException($"Received null line. Exiting in error state.");
            }

            _logger.Info($">> Received input: {line}");
            return line;
        }
    }
}
