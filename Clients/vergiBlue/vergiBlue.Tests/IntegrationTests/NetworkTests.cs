using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace IntegrationTests
{

    

    [TestFixture]
    public class NetworkTests
    {
        [Test]
        public void Server_SmokeTest_ShouldStartAndShutdown()
        {
            var serverExePath = Utils.GetServerExePath();

            using var server = new ConsoleTester(serverExePath);
            server.Run();

            server.Read();
            server.Read();
            server.Read();
            server.WriteChar('q');

            server.AssertExit(1500);
        }

        // ---
        // Example server communication
        // Press any key to stop the server...
        // Client white requested initialize.
        // [procId:19, threadId:21] Started Act() with ipv4:127.0.0.1:53167
        // [procId:13, threadId:20] Player 1 streaming started
        // [procId:16, threadId:20] white:       Move[1] e2 to e4
        // [procId:13, threadId:20] white:       Check cache utilized count: : 2. Board evaluations: : 20. Time elapsed (ms): : 25.
        //                                       Check evaluations: : 2. Chosen opening strategy: Danish gambit. 20 valid moves found.

        // ---
        // Example client communication

        // Chess ai vergiBlue v0.0
        // 
        // 
        // Opening gRPC channel to 127.0.0.1:30052
        // Initializing client... Getting start information from server.
        // Received info: start player: True.
        // Received game start information.
        // white starts the game.
        // 
        // 
        // Starting logic...
        // Start game loop
        // DEBUG: Status(StatusCode="Unknown", Detail="Stream removed", DebugException="Grpc.Core.Internal.CoreErrorDetailException: 
        // {
        // 	"created":"@1668714656.621000000",
        // 	"description":
        // 	"Error received from peer ipv4:127.0.0.1:30052",
        // 	"file":"..\..\..\src\core\lib\surface\call.cc",
        // 	"file_line":906,
        // 	"grpc_message":"Stream removed",
        // 	"grpc_status":2
        // }")
        // Game ended, reason: Stream removed

        [Test]
        public void Server_ConnectionTest_ClientConnectionShouldSucceed()
        {
            var serverExePath = Utils.GetServerExePath();
            var consoleExePath = Utils.GetConsoleExePath();

            using var server = new ConsoleTester(serverExePath);
            server.Run();

            using var console = new ConsoleTester(consoleExePath);
            console.RunWithArguments("chessarena --gamemode 1 --playername white");


            Task.Delay(1000);


            server.WriteChar('q');
            server.AssertExit(1500);

            console.AssertExit(100);
        }
    }
}
