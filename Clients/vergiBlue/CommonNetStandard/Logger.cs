using System;

namespace CommonNetStandard
{
    public static class Logger
    {
        public static void Log(string message, bool writeToConsole = true)
        {
            if (writeToConsole)
            {
                Console.WriteLine(message);
            }
            else throw new NotImplementedException();
        }
    }
}
