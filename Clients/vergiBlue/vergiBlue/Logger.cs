using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiBlue
{
    static class Logger
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
