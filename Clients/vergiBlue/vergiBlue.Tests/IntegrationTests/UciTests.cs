using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class UciTests
    {
        [Test]
        public void Uci_BasicCommunication_Test()
        {
            var consoleAssembly = typeof(vergiBlueConsole.Program).Assembly;

            var exePath = consoleAssembly.Location.Replace(".dll", ".exe");
            if (!File.Exists(exePath)) throw new AssertionException($"Target exe does not exist in {exePath}");
            
            var startInfo = new ProcessStartInfo(exePath);
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            var console = Process.Start(startInfo) ?? throw new AssertionException("Failed to start process");
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
            while (readyResponse.StartsWith("DEBUG")) readyResponse = Read(console);
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
            console.WaitForExit(1000);
            Assert.IsTrue(console.HasExited);
        }

        private string Read(Process app)
        {
            return app.StandardOutput.ReadLine() ?? throw new AssertionException("End of input stream reached");
        }

        private void Write(Process app, string message)
        {
            app.StandardInput.WriteLine(message);
        }
    }
}
