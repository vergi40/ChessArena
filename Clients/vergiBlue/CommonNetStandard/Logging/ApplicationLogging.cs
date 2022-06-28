using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CommonNetStandard.Logging
{
    /// <summary>
    /// Use Microsoft.Extensions.Logging API for logging messages.
    /// MEL API offers support for variety of providers - easy to change later.
    /// Logging provider and configuration can be modified here.
    /// https://stackify.com/net-core-loggerfactory-use-correctly/
    /// https://stackoverflow.com/questions/46483019/logging-from-static-members-with-microsoft-extensions-logging
    /// </summary>
    public class ApplicationLogging
    {
        // Notes: Could figure out, how to write short events in console, but full events in file
        // Or to have own dedicated log files for each
        private static ILoggerFactory? _loggerFactory;

        private static void ConfigureLogger(ILoggerFactory loggerFactory)
        {
            // TODO not sure yet should there be separate configuring for console and desktop project
            // Or have dedicated logger for each
        }

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    // %date [%thread] %level %logger{1} | %message%newline"
                    var serilogLogger = new LoggerConfiguration().MinimumLevel.Debug()
                        .WriteTo.Console(
                            restrictedToMinimumLevel: LogEventLevel.Information, 
                            outputTemplate: "{Message}{NewLine}")
                        .WriteTo.File("Logs/vergiBlue.log", 
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} {Class} | {Method} {Message:lj}{NewLine}{Exception}")
                        .Enrich.WithThreadId()
                        .CreateLogger();

                    _loggerFactory = new LoggerFactory().AddSerilog(serilogLogger);
                    ConfigureLogger(_loggerFactory);
                }

                return _loggerFactory;
            }
            set
            {
                _loggerFactory = value;
            }
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}
