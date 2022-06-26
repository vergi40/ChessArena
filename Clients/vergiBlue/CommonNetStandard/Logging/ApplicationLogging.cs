using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CommonNetStandard.Logging
{
    /// <summary>
    /// Use Microsoft.Extensions.Logging API for logging messages.
    /// Logging provider and configuration can be modified here.
    /// https://stackify.com/net-core-loggerfactory-use-correctly/
    /// </summary>
    public class ApplicationLogging
    {
        private static ILoggerFactory? _loggerFactory;

        private static void ConfigureLogger(ILoggerFactory loggerFactory)
        {
            // TODO serilog

        }

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    var serilogLogger = new LoggerConfiguration().MinimumLevel.Debug()
                        .WriteTo.Console()
                        .WriteTo.File("Logs/test.log")
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
