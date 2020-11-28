using System;
using System.Runtime.InteropServices;

namespace CommonNetStandard.LocalImplementation
{
    /// <summary>
    /// Modify console output foreground and background colors.
    /// </summary>
    internal class ConsoleColors
    {
        // https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
        private readonly bool _isWindowsOS;

        // Color range is 0-255
        const int WhiteInteger = 255;
        const int BlackInteger = 0;

        public string WhiteBackground
        {
            get
            {
                if (_isWindowsOS) return $"\x1b[48;5;{WhiteInteger}m";
                return "";
            }
        }
        public string BlackBackground
        {
            get
            {
                if (_isWindowsOS) return $"\x1b[48;5;{BlackInteger}m";
                return "";
            }
        }
        public string WhiteForeground
        {
            get
            {
                if (_isWindowsOS) return $"\x1b[38;5;{WhiteInteger}m";
                return "";
            }
        }
        public string BlackForeground
        {
            get
            {
                if (_isWindowsOS) return $"\x1b[38;5;{BlackInteger}m";
                return "";
            }
        }


        // Console coloring
        // https://stackoverflow.com/questions/7937256/custom-text-color-in-c-sharp-console-application
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int handle);

        public ConsoleColors(bool isWindows)
        {
            if (isWindows)
            {
                System.Console.SetWindowSize(180, 40);
                
                // Console text color editing
                var handle = GetStdHandle(-11);
                GetConsoleMode(handle, out var mode);
                SetConsoleMode(handle, mode | 0x4);
                _isWindowsOS = true;
            }
            else
            {
                _isWindowsOS = false;
            }
        }
    }
}
