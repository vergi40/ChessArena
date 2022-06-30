using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
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

        private const string _fileName = "Logs/vergiBlue.log";
        
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = CreateSerilogLoggerFactory();
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
            // Example output. Use ExpressionTemplate to parse only class name
            // 2022-06-30 08:09:38,584 [1] INF MainViewModel | History: 3: d1 to b3
            var template = new ExpressionTemplate("{@t:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {@l:u3} " +
                                                    "{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)} " +
                                                    "| {@m}\n{@x}");

            var originalTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} {SourceContext} | {Message:lj}{NewLine}{Exception}";
            
            var serilogLogger = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.Console(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Message}{NewLine}")
                .WriteTo.File(template, _fileName)
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
            var logFileTarget = new NLog.Targets.FileTarget("logfile") { FileName = _fileName, Layout = layout};
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
