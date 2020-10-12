using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 30052;

            Server server = new Server
            {
                Services = { ChessArena.BindService(new TestServer()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("vergiBlue test server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    /// <summary>
    /// Test one player interactions by providing mock data for another player
    /// </summary>
    class TestServer : ChessArena.ChessArenaBase
    {
        public PlayerClass Client { get; set; }
        public MockClass MockPlayer { get; set; }

        private bool _gameStarted = false;


        public List<Move> MoveList { get; set; } = new List<Move>();

        public TestServer()
        {
            Client = new PlayerClass();

            MockPlayer = new MockClass()
            {
                Information = new PlayerInformation()
                {
                    Name = "dumdum"
                }
            };
        }
        
        public override Task<GameStartInformation> Initialize(PlayerInformation request, ServerCallContext context)
        {
            Console.WriteLine($"Client {request.Name} requested initialize.");
            Client.Information = request;

            var response = new GameStartInformation()
            {
                WhitePlayer = true
            };

            _gameStarted = true;
            return Task.FromResult(response);
        }

        public override async Task CreateMovements(IAsyncStreamReader<PlayerMove> requestStream, IServerStreamWriter<Move> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                Client.LatestMove = requestStream.Current;
                Console.WriteLine($"Received move from {Client.Information.Name}: {Client.PrintLatest()}");

                await Task.Delay(1000);

                await responseStream.WriteAsync(MockPlayer.CreateOpponentMockMove());
                Console.WriteLine($"Returned opponent move: {MockPlayer.PrintLatest()}");
            }
        }


    }

    class PlayerClass
    {
        public PlayerInformation Information { get; set; }
        public PlayerMove LatestMove { get; set; }

        public string PrintLatest()
        {
            var message = $"{LatestMove.Move.StartPosition} to {LatestMove.Move.EndPosition}";
            return message;
        }
    }

    class MockClass : PlayerClass
    {
        private int _index = 7;
        public Move CreateOpponentMockMove()
        {
            var move = new Move()
            {
                StartPosition = $"b{_index--}",
                EndPosition = $"b{_index}",
                PromotionResult = Move.Types.PromotionPieceType.NoPromotion
            };

            LatestMove = new PlayerMove()
            {
                Move = move
            };

            return move;
        }
    }
}
