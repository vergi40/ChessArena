using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = NLog.LogLevel;

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
        
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = CreateNLogLoggerFactory();
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

        private static ILoggerFactory CreateSerilogLoggerFactory()
        {
            // Example output. Notice the class name will be full name
            // 2022-06-29 22:47:56,405 [1] INF vergiBlueDesktop.Views.MainViewModel | History: 1: e2 to e4
            var serilogLogger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.Console(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Message}{NewLine}")
                .WriteTo.File("Logs/vergiBlue.log",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} {SourceContext} | {Message:lj}{NewLine}{Exception}")
                .Enrich.WithThreadId()
                .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog(serilogLogger);

            return loggerFactory;
        }

        /// <summary>
        /// https://github.com/NLog/NLog/wiki/Configure-from-code
        /// </summary>
        /// <returns></returns>
        private static ILoggerFactory CreateNLogLoggerFactory()
        {
            // 2022-06-29 22:40:32,844 [1] Info MainViewModel.AppendHistory | History: 4: e7 to e5
            // NOTE: method name is huge performance hit. Disable if needed
            var layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss,fff} [${threadid}] ${level} " +
                         "${logger:shortName=True}.${callsite:className=False:fileName=False:includeSourcePath=False:methodName=True} | " +
                         "${message}${exception}";

            // Targets where to log to: File and Console
            var logFileTarget = new NLog.Targets.FileTarget("logfile") { FileName = "Logs/vergiBlue.log", Layout = layout};
            var consoleTarget = new NLog.Targets.ConsoleTarget("logconsole"){Layout = "${message"};

            var config = new LoggingConfiguration();
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            

            var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddNLog(config);
            });
            return loggerFactory;
        }
    }
}
