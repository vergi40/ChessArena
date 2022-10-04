using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using vergiCommon;

namespace IntegrationTests
{
    [TestFixture]
    public class UciTests
    {
        [Test]
        public void Uci_BasicCommunication_Test()
        {
            var solutionPath = GetPath.ThisSolution();
            var exePath = Path.Combine(solutionPath, @"vergiBlueConsole\bin\Release\net6.0\vergiBlueConsole.exe");
            //var exePath = Path.Combine(solutionPath, @"vergiBlueConsole\bin\Debug\net6.0\vergiBlueConsole.exe");
            if (!File.Exists(exePath)) throw new AssertionException($"Target exe does not exist in {exePath}");
            
            var startInfo = new ProcessStartInfo(exePath);
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            var console = Process.Start(startInfo);
            Write(console, "uci");

            while (true)
            {
                var next = Read(console);
                if (next == "uciok") break;
                else if (next.StartsWith("id")) continue;
                else if (next.StartsWith("option")) continue;
                throw new AssertionException($"Unknown uci command: {next}");
            }

            Write(console, "isready");
            var readyResponse = Read(console);
            Assert.AreEqual("readyok", readyResponse);

            Write(console, "position startpos moves e2e4");
            Write(console, "go movetime 2000");

            while (true)
            {
                var next = Read(console);
                if (next.StartsWith("bestmove")) break;
                else if (next.StartsWith("info")) continue;
                throw new AssertionException($"Unknown uci command: {next}");
            }

            Write(console, "exit");

        }

        private string Read(Process app)
        {
            return app.StandardOutput.ReadLine();
        }

        private void Write(Process app, string message)
        {
            app.StandardInput.WriteLine(message);
        }
    }
}
