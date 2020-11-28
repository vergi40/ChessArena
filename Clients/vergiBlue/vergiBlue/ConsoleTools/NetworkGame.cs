using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard;
using CommonNetStandard.Client;

namespace vergiBlue.ConsoleTools
{
    class NetworkGame
    {
        private static void Log(string message, bool writeToConsole = true) => Logger.Log(message, writeToConsole);
        public static void Start(grpcClientConnection grpcClientConnection, string playerName, bool connectionTesting)
        {
            Log(Environment.NewLine);
            // TODO async
            var startInformation = grpcClientConnection.Initialize(playerName);

            // TODO exception catching here
            // DebugException="Grpc.Core.Internal.CoreErrorDetailException: 
            startInformation.Wait();

            Log($"Received game start information.");
            if (startInformation.Result.WhitePlayer) Log($"{playerName} starts the game.");
            else Log($"Opponent starts the game.");

            Log(Environment.NewLine);

            Log("Starting logic...");
            var ai = new Logic(startInformation.Result, connectionTesting);

            Log("Start game loop");

            // Inject ai to connection module and play game
            var playTask = grpcClientConnection.Play(ai);
            playTask.Wait();
        }
    }
}
