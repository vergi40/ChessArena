using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace IntegrationTests
{
    /// <summary>
    /// Wrapper for using <see cref="System.Diagnostics.Process"/> to test high level functionality through
    /// console input/output stream
    /// </summary>
    internal sealed class ConsoleTester : IDisposable
    {
        private readonly string _exeName;
        private readonly string _exePath;
        private readonly bool _skipDebugLines;

        private Process? _console;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exePath">Absolute path to executable</param>
        /// <param name="skipDebugLines">Skip console input lines starting with DEBUG</param>
        public ConsoleTester(string exePath, bool skipDebugLines = true)
        {
            _exeName = Path.GetFileName(exePath);
            _exePath = exePath;
            _skipDebugLines = skipDebugLines;
        }

        public void Run()
        {
            //Trace.Listeners.Add(new ConsoleTraceListener());

            if (!File.Exists(_exePath)) throw new AssertionException($"Target exe does not exist in {_exePath}");

            var startInfo = new ProcessStartInfo(_exePath);
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            _console = Process.Start(startInfo) ?? throw new AssertionException("Failed to start process");
            TestContext.WriteLine($"Process {_exeName} started");
        }

        /// <summary>
        /// Read until EOL
        /// </summary>
        public string Read()
        {
            if (_console == null) throw new AssertionException("Console process exited unexpectedly");

            var input = _console.StandardOutput.ReadLine() ?? throw new AssertionException("End of input stream reached");
            while (input.StartsWith("DEBUG"))
            {
                input = _console.StandardOutput.ReadLine() ?? throw new AssertionException("End of input stream reached");
            }

            TestContext.WriteLine($"{_exeName} > {input}");
            return input;
        }

        /// <summary>
        /// Write line to console
        /// </summary>
        public void Write(string message)
        {
            if (_console == null) throw new AssertionException("Console process exited unexpectedly");
            TestContext.WriteLine($"{_exeName} < {message}");
            _console.StandardInput.WriteLine(message);
        }

        public void AssertExit(int waitInMs = 1000)
        {
            if (_console == null) throw new AssertionException("Console process exited unexpectedly");
            _console.WaitForExit(waitInMs);
            Assert.IsTrue(_console.HasExited);
            TestContext.WriteLine($"Process {_exeName} exited gracefully");
        }

        public void Dispose()
        {
            _console?.Dispose();
        }
    }
}
