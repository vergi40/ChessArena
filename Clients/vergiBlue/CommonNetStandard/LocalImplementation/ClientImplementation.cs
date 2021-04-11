using System;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard.Client;
using GameManager;
using Grpc.Core;

namespace CommonNetStandard.LocalImplementation
{
    internal class ClientImplementation
    {
        readonly GameService.GameServiceClient _client;

        public ClientImplementation(GameService.GameServiceClient client)
        {
            this._client = client;
        }

        public async Task<GameStartInformation> Initialize(string clientName)
        {
            var information = new GameInformation()
            {
                Name = clientName,
                Chess = ""
            };

            Console.WriteLine("Initializing client... Getting start information from server.");
            var gameStartInformation = await _client.InitializeAsync(information);

            Console.WriteLine($"Received info: start player: {gameStartInformation.Start}.");
            return gameStartInformation;

        }

        public async Task CreateMovements(LogicBase ai)
        {
            try
            {
                using (var call = _client.Act())
                {
                    await call.RequestStream.WriteAsync(Mapping.ToGrpc(ai.CreateMove()));

                    // Continuos bidirectional streaming
                    var responseReaderTask = Task.Run((Func<Task>) (async () =>
                    {
                        while (await call.ResponseStream.MoveNext(CancellationToken.None))
                        {
                            var opponentMove = call.ResponseStream.Current;
                            if(opponentMove == null || opponentMove.Chess == null)
                            {
                                // Error or game end
                                return;
                            }
                            else if (opponentMove.Chess.CheckMate)
                            {
                                // TODO should we trust this
                                // TODO for now CheckMate also used to sign any game ending and errors
                                return;
                            }

                            Logger.Log($"Received opponent move: {opponentMove.Chess.StartPosition} to {opponentMove.Chess.EndPosition}");

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
                Logger.Log($"DEBUG: {e.ToString()}");
                throw new GameEndedException(e);
            }
        }
    }
    
    public class GameEndedException : Exception
    {
        public RpcException E { get; set; }

        public GameEndedException(RpcException e)
        {
            E = e;
        }
    }
}
