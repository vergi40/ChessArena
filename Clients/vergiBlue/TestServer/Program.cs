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
        public PlayerClass Player1 { get; set; }
        public PlayerClass Player2 { get; set; }
        public MockClass MockPlayer { get; set; }

        public TestServer()
        {
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
            bool connectionTest = request.Name.Contains("test");
            GameStartInformation response;

            if (Player1 == null)
            {
                // First connection
                if (connectionTest) Player1 = MockPlayer;
                else Player1 = new PlayerClass() {Information = request};
                response = new GameStartInformation()
                {
                    WhitePlayer = true
                };
            }
            else if(Player2 == null) 
            {
                if (connectionTest) Player2 = MockPlayer;
                else Player2 = new PlayerClass() { Information = request };
                response = new GameStartInformation()
                {
                    WhitePlayer = false,
                    OpponentMove = Player1.LatestMove.Move
                };
            }
            else
            {
                throw new ArgumentException("Error: Can't have 3 clients playing.");
            }

            return Task.FromResult(response);
        }

        private IAsyncStreamReader<PlayerMove> _p1ReqStream = null;
        private IServerStreamWriter<Move> _p1ResStream = null;
        private IAsyncStreamReader<PlayerMove> _p2ReqStream = null;
        private IServerStreamWriter<Move> _p2ResStream = null;

        private enum GameState
        {
            P1NotInitialized,
            P2NotInitialized,
            P1Req,
            P1Res,
            P2Req,
            P2Res
        }

        private GameState _nextState = GameState.P1NotInitialized;
        private readonly object _stateLock = new object();
        private int _debugInstanceCount = 0;

        private Task _mainLoop = null;

        /// <summary>
        /// Two instances of this will be open simultaneously
        /// </summary>
        public override async Task CreateMovements(IAsyncStreamReader<PlayerMove> requestStream, IServerStreamWriter<Move> responseStream, ServerCallContext context)
        {
            _debugInstanceCount++;

            if (_mainLoop == null)
            {
                // P1
                if (!Player1.StreamOpened)
                {
                    Player1.StreamOpened = true;
                    Player1.PeerName = context.Peer;
                    _p1ReqStream = requestStream;
                    _p1ResStream = responseStream;
                }

                lock(_stateLock) _nextState = GameState.P1Req;
                _mainLoop = MainLoop();
            }
            else
            {
                if (!Player2.StreamOpened && context.Peer != Player1.PeerName)
                {
                    Player2.StreamOpened = true;
                    Player2.PeerName = context.Peer;
                    _p2ReqStream = requestStream;
                    _p2ResStream = responseStream;

                    lock (_stateLock) _nextState = GameState.P2Req;
                    // TODO what happens to second stream?
                    //return;//Close second instance
                }
            }

            await _mainLoop;
        }

        private async Task MainLoop()
        {
            while (true)
            {
                try
                {
                    if (_nextState == GameState.P2NotInitialized)
                    {
                        if (Player2 == null || !Player2.StreamOpened)
                        {
                            Console.WriteLine("Waiting for second player to initialize");
                            while (Player2 == null)
                            {
                                await Task.Delay(50);
                            }
                        }
                    }
                    else if (_nextState == GameState.P1Req)
                    {
                        await _p1ReqStream.MoveNext();
                        Player1.LatestMove = _p1ReqStream.Current;
                        Console.WriteLine($"Received move [{Player1.Information.Name}]: {Player1.PrintLatest()}");
                        Console.WriteLine($"{Player1.LatestMove.Diagnostics}");

                        if (Player2 == null || !Player2.StreamOpened)
                        {
                            lock (_stateLock) _nextState = GameState.P2NotInitialized;
                        }
                        else
                        {
                            lock (_stateLock) _nextState = GameState.P2Res;
                        }
                    }
                    else if (_nextState == GameState.P2Res)
                    {
                        await _p2ResStream.WriteAsync(Player1.LatestMove.Move);
                        Console.WriteLine($"Sent p1 move to p2");
                        lock (_stateLock) _nextState = GameState.P2Req;
                    }
                    else if (_nextState == GameState.P2Req)
                    {
                        await _p2ReqStream.MoveNext();
                        Player2.LatestMove = _p2ReqStream.Current;
                        Console.WriteLine($"Received move [{Player2.Information.Name}]: {Player2.PrintLatest()}");
                        Console.WriteLine($"{Player2.LatestMove.Diagnostics}");
                        lock (_stateLock) _nextState = GameState.P1Res;
                    }
                    else if (_nextState == GameState.P1Res)
                    {
                        await _p1ResStream.WriteAsync(Player2.LatestMove.Move);
                        Console.WriteLine($"Sent p2 move to p1");
                        lock (_stateLock) _nextState = GameState.P1Req;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                await Task.Delay(100);
            }
        }
    }

    class PlayerClass
    {
        /// <summary>
        /// Will be set when bidirectional stream is opened
        /// </summary>
        public string PeerName { get; set; } = "";
        public bool StreamOpened { get; set; } = false;

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
