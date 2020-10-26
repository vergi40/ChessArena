using System;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard.Interface;
using Grpc.Core;

namespace CommonNetStandard.Connection
{
    class ClientImplementation
    {
        readonly ChessArena.ChessArenaClient _client;

        public ClientImplementation(ChessArena.ChessArenaClient client)
        {
            this._client = client;
        }

        public async Task<GameStartInformation> Initialize(string clientName)
        {
            var information = new PlayerInformation()
            {
                Name = clientName
            };

            Console.WriteLine("Initializing client... Getting start information from server.");
            var startInformation = await _client.InitializeAsync(information);

            Console.WriteLine($"Received info: start player: {startInformation.WhitePlayer}.");
            return startInformation;

        }

        public async Task CreateMovements(LogicBase ai)
        {
            try
            {
                using (var call = _client.CreateMovements())
                {
                    await call.RequestStream.WriteAsync(Mapping.ToGrpc(ai.CreateMove()));

                    // Continuos bidirectional streaming
                    var responseReaderTask = Task.Run((Func<Task>) (async () =>
                    {
                        while (await call.ResponseStream.MoveNext(CancellationToken.None))
                        {
                            var opponentMove = call.ResponseStream.Current;
                            Logger.Log($"Received opponent move: {opponentMove.StartPosition} to {opponentMove.EndPosition}");

                            // Analyze opponent move
                            ai.ReceiveMove(Mapping.ToCommon(opponentMove));

                            // Create own move
                            await call.RequestStream.WriteAsync(Mapping.ToGrpc(ai.CreateMove()));
                        }
                    }));

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
