using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace vergiBlue.Connection
{
    class ClientImplementation
    {
        readonly MovementStream.MovementStreamClient _client;

        public ClientImplementation(MovementStream.MovementStreamClient client)
        {
            this._client = client;
        }

        public async Task<GameStartInformation> Initialize(string clientName)
        {
            var information = new ClientInformation()
            {
                Name = clientName
            };

            Console.WriteLine("Initializing client... Getting start information from server.");
            var startInformation = await _client.InitializeAsync(information);

            Console.WriteLine($"Received info: start player: {startInformation.WhitePlayer}.");
            return startInformation;

        }

        public async Task CreateMovements(Logic ai)
        {
            try
            {
                using (var call = _client.CreateMovements())
                {
                    // Stream start
                    await call.RequestStream.WriteAsync(ai.CreateMove());

                    // Continuos bidirectional streaming
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext(CancellationToken.None))
                        {
                            var opponentMove = call.ResponseStream.Current;
                            Logger.Log($"Received opponent move: {opponentMove.Coordinates.EndPosition}");

                            // Analyze opponent move
                            ai.ReceiveMove(opponentMove);

                            // Create own move
                            await call.RequestStream.WriteAsync(ai.CreateMove());
                        }
                    });

                    await responseReaderTask;
                    await call.RequestStream.CompleteAsync();
                }
            }
            catch (RpcException e)
            {
                Logger.Log(e.ToString());
            }
        }
    }
}
