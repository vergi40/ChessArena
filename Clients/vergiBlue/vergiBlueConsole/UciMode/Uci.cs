using System.Reflection;
using CommonNetStandard.Common;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue;
using vergiBlue.Logic;

namespace vergiBlueConsole.UciMode
{
    /// <summary>
    /// Discuss with chess gui using standard input/output stream
    /// </summary>
    internal class Uci
    {
        private static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#endif
                return false;
            }
        }

        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<Uci>();
        private static UciInput _input = new UciInput(null);

        private static Task<SearchResult>? _currentSearch { get; set; }
        private static CancellationTokenSource _searchCancellation { get; set; } = new CancellationTokenSource();

        public static void Run(StreamReader? inputStream = null)
        {
            _input = new UciInput(inputStream);
            WriteLine($"id name vergiBlue v{Program.GetVergiBlueVersion()}, console v{Program.GetConsoleVersion()}");
            WriteLine("id author Teemu Laine");

            WriteLine("uciok");

            if (IsDebug)
            {
                // Use direct Console instead of logging writeline method
                Console.WriteLine("DEBUG Command legend: isready, ucinewgame, position [startpos/fen] moves ..., stop, exit ");
                Console.WriteLine("DEBUG Command legend: go [infinite, depth n, movetime n, nodes n, mate n, winc n, binc n]");
            }

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
                    logic.NewGame();
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

                    _logger.LogDebug("Search task start");
                    _searchCancellation = new CancellationTokenSource();
                    _currentSearch = Task.Run(() => logic.CreateSearchTask(parameters, SearchInfoUpdate, _searchCancellation.Token));
                    _currentSearch.ContinueWith(asd =>
                    {
                        _logger.LogDebug($"Search finished with status {asd.Status}");
                        WriteLine($"bestmove {asd.Result.BestMove.ToCompactString()}");
                    });
                }
                else if (gameCommand.Contains("stop"))
                {
                    if (_currentSearch == null)
                    {
                        _logger.LogDebug("Stop called even though no search running.");
                    }
                    else if (_currentSearch.IsCompleted)
                    {
                        _logger.LogDebug("Search has already ran to completion");
                    }
                    else
                    {
                        _logger.LogDebug("Search task stop requested");
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

        

        private static void WriteLine(string message)
        {
            _logger.LogInformation($"{message}");
            _logger.LogDebug($"Output << {message}");
            //Console.WriteLine(message);
        }
    }
}
