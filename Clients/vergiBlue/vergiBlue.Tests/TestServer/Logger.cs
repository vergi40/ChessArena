using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace TestServer
{
    public static class Log4NetLogger
    {
        public static ILog GetLogger(Type type)
        {
            return log4net.LogManager.GetLogger(type);
        }
    }

    /// <summary>
    /// Logs simple messages to console, full logs with timeframe to file
    /// </summary>
    public class Logger
    {
        private readonly ILog _fileLogger;

        /// <summary>
        /// Logs simple messages to console, full logs with timeframe to file
        /// </summary>
        public Logger(Type classType)
        {
            _fileLogger = Log4NetLogger.GetLogger(classType);
        }

        public void Info(string message)
        {
            _fileLogger.Info(message);
            Console.WriteLine(message);
        }

        public void Error(Exception e, string message = "")
        {
            _fileLogger.Error(message, e);
        }
    }
}
