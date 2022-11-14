using System;
using System.Threading.Tasks;
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

            using var console = new ConsoleTester(exePath);
            console.Run();
            console.Write("uci");

            while (true)
            {
                var next = console.Read();
                if (next == "uciok") break;
                else if (next.StartsWith("id")) continue;
                else if (next.StartsWith("option")) continue;
                throw new AssertionException($"Unknown uci command: {next}");
            }

            console.Write("isready");
            var readyResponse = console.Read();
            Assert.AreEqual("readyok", readyResponse);

            console.Write("position startpos moves e2e4");
            console.Write("go movetime 2000");

            RunWithTimeLimit(2000, () =>
            {
                while (true)
                {
                    var next = console.Read();
                    if (next.StartsWith("bestmove")) break;
                    else if (next.StartsWith("info")) continue;
                    throw new AssertionException($"Unknown uci command: {next}");
                }

                return true;
            });
            
            console.Write("exit");
            console.AssertExit();
        }

        private void RunWithTimeLimit(int limitInMs, Func<bool> func)
        {
            var result = Task.Run(func).Wait(limitInMs);

            if (!result) throw new InvalidOperationException("Failed to run action in time limit");
        }
    }
}
