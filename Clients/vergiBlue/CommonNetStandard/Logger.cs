using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CommonNetStandard
{
    public static class Logger
    {
        // This is minimal low effort log+console
        public static void LogWithConsole(string message, ILog sourceLogger)
        {
            Console.WriteLine(message);
            sourceLogger.Info(message);
        }
    }

    public static class LogExtensions
    {
        public static void InfoConsole(this ILog logger, string message)
        {
            Console.WriteLine(message);
            logger.Info(message);
        }
    }
}
