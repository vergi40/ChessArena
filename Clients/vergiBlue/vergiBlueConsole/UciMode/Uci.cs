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
        private static UciInput _input = new UciInput(null);

        private static Task<SearchResult>? _currentSearch { get; set; }
        private static CancellationTokenSource _searchCancellation { get; set; } = new CancellationTokenSource();

        public static void Run(StreamReader? inputStream = null)
        {
            _input = new UciInput(inputStream);
            WriteLine($"id name vergiBlue v{GetVergiBlueVersion()}, console v{GetConsoleVersion()}");
            WriteLine("id author Teemu Laine");

            WriteLine("uciok");

            if (!RunOptions()) return;

            // Initialize logic
            var logic = LogicFactory.CreateForUci();
            WriteLine("readyok");

            while (true)
            {
                var gameCommand = _input.ReadLine();
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

                    _logger.Debug("Search task start");
                    _searchCancellation = new CancellationTokenSource();
                    _currentSearch = Task.Run(() => logic.CreateSearchTask(parameters, SearchInfoUpdate, _searchCancellation.Token));
                    _currentSearch.ContinueWith(asd =>
                    {
                        _logger.Debug($"Search finished with status {asd.Status}");
                        WriteLine($"bestmove {asd.Result.BestMove.ToCompactString()}");
                    });
                }
                else if (gameCommand.Contains("stop"))
                {
                    if (_currentSearch == null)
                    {
                        _logger.Debug("Stop called even though no search running.");
                    }
                    else if (_currentSearch.IsCompleted)
                    {
                        _logger.Debug("Search has already ran to completion");
                    }
                    else
                    {
                        _logger.Debug("Search task stop requested");
                        _searchCancellation.Cancel();
                    }
                }
                else if (gameCommand.Contains("ponderhit"))
                {
                    // TODO
                }
                else if (gameCommand.Equals("exit"))
                {
                    break;
                }
                else
                {
                    throw new ArgumentException($"Unknown command: {gameCommand}");
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
                var nextInput = _input.ReadLine();
                // When adding available options, set here

                if (nextInput.Equals("isready"))
                {
                    break;
                }
                else if (nextInput.Equals("exit"))
                {
                    return false;
                }
                else
                {
                    throw new ArgumentException($"Unknown command: {nextInput}");
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
            _logger.Info($"Output << {message}");
            Console.WriteLine(message);
        }
    }

    class UciInput
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Uci));
        private bool UseStreamAsInput { get; set; }
        private StreamReader InputStream { get; }

        public UciInput(StreamReader? inputStream)
        {
            if (inputStream != null)
            {
                InputStream = inputStream;
                UseStreamAsInput = true;
            }
            else
            {
                InputStream = StreamReader.Null;
            }
        }

        public string ReadLine()
        {
            if (UseStreamAsInput)
            {
                var lineFromStream = InputStream.ReadLine();
                if (lineFromStream != null)
                {
                    _logger.Info($"Input  >> {lineFromStream}");
                    return lineFromStream;
                }
                // Else stream ended, change to console read
                UseStreamAsInput = false;
            }

            var line = Console.ReadLine();
            if (line == null)
            {
                throw new ArgumentException($"Received end of stream from Console.ReadLine. Exiting in error state.");
            }

            _logger.Info($"Input  >> {line}");
            return line;
        }
    }
}
