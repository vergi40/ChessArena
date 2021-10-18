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

        /// <summary>
        /// https://stackoverflow.com/questions/16336917/can-you-configure-log4net-in-code-instead-of-using-a-config-file
        /// </summary>
        public static void Setup()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = true;
            // roller.File = @"Logs\EventLog.txt";
            roller.File = "vergiBlue.log";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "10MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            MemoryAppender memory = new MemoryAppender();
            memory.ActivateOptions();
            hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
        }
    }
}
