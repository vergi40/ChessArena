using System;
using System.Collections.Generic;
using System.Text;
using CommonNetStandard.Client;
using CommonNetStandard.Interface;

namespace CommonNetStandard
{
    /// <summary>
    /// How to utilize Common-project to create your own client
    /// </summary>
    class Example
    {
        static void Main()
        {
            using var connection = GrpcClientConnectionFactory.Create("127.0.0.1:30052");
            var startInformation = connection.Initialize("example player");

            // Wait for the server to respond
            startInformation.Wait();

            // Server tells the client is either starting player (white),
            // or black. If black, info also contains white players first move
            var result = startInformation.Result;

            // Initialize your own ai
            var ai = new ExampleAiLogic();
            ai.Setup(true);

            // Inject ai to connection module and play game
            var playTask = connection.Play(ai);
            playTask.Wait();

            // Game finished
            // Dispose should handle connection closing
            // connection.CloseConnection();
        }

        /// <summary>
        /// Implement inherited <see cref="IAiClient"/> methods with own AI logic
        /// </summary>
        class ExampleAiLogic : IAiClient
        {
            public void Setup(bool isClientWhite)
            {
                throw new NotImplementedException();
            }

            public IPlayerMove CreateMove()
            {
                // TODO
                throw new NotImplementedException();
            }

            public void ReceiveMove(IMove opponentMove)
            {
                // TODO
                throw new NotImplementedException();
            }
        }
    }
}
