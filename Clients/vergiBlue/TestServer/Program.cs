using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GameManager;
using Grpc.Core;

namespace TestServer
{
    class Program
    {
        private static readonly Logger _logger = new Logger(typeof(Program));
        static void Main(string[] args)
        {
            const int Port = 30052;

            Server server = new Server
            {
                Services = { GameService.BindService(new TestServer()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            _logger.Info("vergiBlue test server listening on port " + Port);
            _logger.Info("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    /// <summary>
    /// Test one player interactions by providing mock data for another player
    /// </summary>
    class TestServer : GameService.GameServiceBase
    {
        private static readonly Logger _logger = new Logger(typeof(TestServer));
        public PlayerClass Player1 { get; set; }
        public PlayerClass Player2 { get; set; }
        public MockClass MockPlayer { get; set; }

        public TestServer()
        {
            MockPlayer = new MockClass()
            {
                Information = new GameInformation()
                {
                    Name = "dumdum"
                }
            };
        }
        
        public override Task<GameStartInformation> Initialize(GameInformation request, ServerCallContext context)
        {
            _logger.Info($"Client {request.Name} requested initialize.");
            bool connectionTest = request.Name.Contains("test");
            GameStartInformation response;

            if (Player1 == null)
            {
                // First connection
                if (connectionTest) Player1 = MockPlayer;
                else Player1 = new PlayerClass() {Information = request};
                response = new GameStartInformation()
                {
                    Start = true
                };
            }
            else if(Player2 == null) 
            {
                if (connectionTest) Player2 = MockPlayer;
                else Player2 = new PlayerClass() { Information = request };
                response = new GameStartInformation()
                {
                    Start = false,
                    ChessMove = Player1.LatestMove
                };
            }
            else
            {
                throw new ArgumentException("Error: Can't have 3 clients playing.");
            }

            return Task.FromResult(response);
        }

        private IAsyncStreamReader<Move> _p1ReqStream = null;
        private IServerStreamWriter<Move> _p1ResStream = null;
        private IAsyncStreamReader<Move> _p2ReqStream = null;
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
        public override async Task Act(IAsyncStreamReader<Move> requestStream, IServerStreamWriter<Move> responseStream, ServerCallContext context)
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
                            _logger.Info("Waiting for second player to initialize");
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
                        _logger.Info($"{(Player1.Information.Name + ":").PadRight(12)} Received move {Player1.PrintLatest()}");
                        _logger.Info($"{(Player1.Information.Name + ":").PadRight(12)} {Player1.LatestMove.Diagnostics}");

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
                        await _p2ResStream.WriteAsync(Player1.LatestMove);
                        _logger.Info($"Sent p1 move to p2");
                        lock (_stateLock) _nextState = GameState.P2Req;
                    }
                    else if (_nextState == GameState.P2Req)
                    {
                        await _p2ReqStream.MoveNext();
                        Player2.LatestMove = _p2ReqStream.Current;
                        _logger.Info($"{(Player2.Information.Name + ":").PadRight(12)} Received move {Player2.PrintLatest()}");
                        _logger.Info($"{(Player2.Information.Name + ":").PadRight(12)} {Player2.LatestMove.Diagnostics}");
                        lock (_stateLock) _nextState = GameState.P1Res;
                    }
                    else if (_nextState == GameState.P1Res)
                    {
                        await _p1ResStream.WriteAsync(Player2.LatestMove);
                        _logger.Info($"Sent p2 move to p1");
                        lock (_stateLock) _nextState = GameState.P1Req;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Game loop ended to exception {e.Message}");
                    return;
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

        public GameInformation Information { get; set; }
        public Move LatestMove { get; set; }

        public string PrintLatest()
        {
            var message = $"{LatestMove.Chess.StartPosition} to {LatestMove.Chess.EndPosition}";
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
                Chess = new ChessMove()
                {

                    StartPosition = $"b{_index--}",
                    EndPosition = $"b{_index}",
                    PromotionResult = ChessMove.Types.PromotionPieceType.NoPromotion
                }
            };

            return move;
        }
    }
}
