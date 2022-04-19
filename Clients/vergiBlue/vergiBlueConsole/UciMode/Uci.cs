using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        private static string? _currentVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);

        public static void Run()
        {
            WriteLine($"id name vergiBlue v{_currentVersion}");
            WriteLine("id author Teemu Laine");

            WriteLine("uciok");

            while (true)
            {
                var nextInput = ReadLine();
                // When adding available options, set here

                if (nextInput.Equals("isready"))
                {
                    break;
                }
            }

            // Initialize logic
            var logic = LogicFactory.CreateForUci();
            
            throw new NotImplementedException();
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

            _logger.Info($"Received input: {line}");
            return line;
        }
    }
}
