namespace IntegrationTests
{
    internal class Utils
    {
        public static string GetConsoleExePath()
        {
            var consoleAssembly = typeof(vergiBlueConsole.Program).Assembly;
            var exePath = consoleAssembly.Location.Replace(".dll", ".exe");
            return exePath;
        }

        public static string GetServerExePath()
        {
            var serverAssembly = typeof(TestServer.Program).Assembly;
            var exePath = serverAssembly.Location.Replace(".dll", ".exe");
            return exePath;
        }
    }
}
