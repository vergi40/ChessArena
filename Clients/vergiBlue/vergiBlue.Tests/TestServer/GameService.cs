using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameManager;
using Grpc.Core;

namespace TestServer
{
    /// <summary>
    /// Test one player interactions by providing mock data for another player
    /// </summary>
    class TestServer : GameService.GameServiceBase
    {
        private static readonly Logger _logger = new Logger(typeof(TestServer));
        public PlayerClient? Player1 { get; set; }
        public PlayerClient? Player2 { get; set; }
        public SharedData _shared { get; }

        public TestServer(SharedData shared)
        {
            _shared = shared;
            
            // Keep alive indefinetily
            Task.Run(() => MainHosting());
        }
        
        private async Task MainHosting()
        {
            while (true)
            {
                DebugLog($"Main game loop hosting started.");
                try
                {
                    await RetrieveBothPlayerStreamsToSameContext();
                    DebugLog("Main game loop hosting ended");
                }
                catch (Exception e)
                {
                    DebugLog($"Main game loop hosting ended to exception. {e.ToString()}");
                }

                // TODO must be better way to end stream
                if (Player1?.ResponseStream != null)
                {
                    await Player1.ResponseStream.WriteAsync(new Move()
                    {
                        Chess = new ChessMove()
                        {
                            CheckMate = true
                        }
                    });
                }
                if (Player2?.ResponseStream != null)
                {
                    await Player2.ResponseStream.WriteAsync(new Move()
                    {
                        Chess = new ChessMove()
                        {
                            CheckMate = true
                        }
                    });
                }

                Player1 = null;
                Player2 = null;
                _shared.ResetGame();
                DebugLog("Game reset and players initialized, ready for next game");
            }
        }

        public override Task<PingMessage> Ping(PingMessage request, ServerCallContext context)
        {
            _logger.Info($"Client {context.Peer} sent {request.Message}");
            _logger.Info($"Responding with \"pong\"");

            var response = new PingMessage()
            {
                Message = "Pong"
            };

            return Task.FromResult( response );
        }

        public override Task<GameStartInformation> Initialize(GameInformation request, ServerCallContext context)
        {
            _logger.Info($"Client {request.Name} requested initialize.");
            GameStartInformation response;

            if (Player1 == null)
            {
                // First connection
                Player1 = new PlayerClient()
                {
                    PlayerIndex = 0,
                    RequestStream = null,
                    ResponseStream = null,
                    PeerName = context.Peer,
                    Information = request
                };
                response = new GameStartInformation()
                {
                    Start = true
                };

            }
            else if (Player2 == null)
            {
                Player2 = new PlayerClient()
                {
                    PlayerIndex = 1,
                    RequestStream = null,
                    ResponseStream = null,
                    PeerName = context.Peer,
                    Information = request
                };
                
                // TODO logic to wait p1 first move
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
        
        private enum GameState
        {
            P1NotInitialized,
            P2NotInitialized,
            P1Req,
            P1Res,
            P2Req,
            P2Res
        }
        
        private void DebugLog(string message)
        {
            var processorId = Thread.GetCurrentProcessorId();
            var threadName = Thread.CurrentThread.ManagedThreadId;
            
            _logger.Info($"[procId:{processorId}, threadId:{threadName}] {message}");
        }
        
        // These are used to get players safely to same context
        private readonly ConcurrentQueue<PlayerClient> _activations = new ConcurrentQueue<PlayerClient>();
        private readonly SemaphoreSlim _actSemaphore = new SemaphoreSlim(0);

        /// <summary>
        /// P1 initializes.
        /// P1 sends start move.
        /// P2 initializes.
        /// Send P1 move to P2.
        /// Send P2 move to P1.
        /// </summary>
        /// <returns></returns>
        private async Task RetrieveBothPlayerStreamsToSameContext()
        {
            // Waiting for both players
            await _actSemaphore.WaitAsync().ConfigureAwait(false);

            // Got first player stream
            var gotFirst = _activations.TryDequeue(out var player1);
            if (player1 == null) throw new ArgumentException($"Dequeue returned null player1");
            DebugLog($"Player 1 streaming started");

            // ... stuff
            await ReceivePlayerMove(player1);

            // P2 received move info for initialization and sent first move
            await _actSemaphore.WaitAsync().ConfigureAwait(false);
            var gotSecond = _activations.TryDequeue(out var player2);
            if (player2 == null) throw new ArgumentException($"Dequeue returned null player2");
            DebugLog($"Player 2 streaming started");

            await ReceivePlayerMove(player2);


            // Main game
            var firstPlayerTurn = false;
            while (player1.StreamOpened && player2.StreamOpened)
            {
                var sender = player1;
                var receiver = player2;
                if (!firstPlayerTurn)
                {
                    sender = player2;
                    receiver = player1;
                }

                await SendMove(sender, receiver);
                await ReceivePlayerMove(receiver);

                firstPlayerTurn = !firstPlayerTurn;
                //await Task.Delay(_shared.CycleDelayInMs);
            }

            DebugLog($"Stream was closed");
            player1.StreamingTask = Task.FromResult(Task.CompletedTask);
            player2.StreamingTask = Task.FromResult(Task.CompletedTask);
        }

        private async Task ReceivePlayerMove(PlayerClient player)
        {
            await player.RequestStream.MoveNext();

            if (player.RequestStream == null) throw new ArgumentException($"{player.Information.Name} stream was down.");
            player.LatestMove = player.RequestStream.Current;
            _shared.MoveHistory.Add(player.LatestMove);
            
            DebugLog($"{(player.Information.Name + ":").PadRight(12)} Move[{_shared.CurrentMoveCount}] {player.PrintLatest()}");
            DebugLog($"{(player.Information.Name + ":").PadRight(12)} {player.LatestMove.Diagnostics}");
        }

        private async Task SendMove(PlayerClient sender, PlayerClient receiver)
        {
            if (receiver.ResponseStream == null)
                throw new ArgumentException($"{receiver.Information.Name} stream was down.");
            await receiver.ResponseStream.WriteAsync(sender.LatestMove);
            
            DebugLog($"Sent p{sender.PlayerIndex+1} move to p{receiver.PlayerIndex+1}");
        }
        
        /// <summary>
        /// Two instances of this will be open simultaneously
        /// </summary>
        public override async Task Act(IAsyncStreamReader<Move> requestStream, IServerStreamWriter<Move> responseStream, ServerCallContext context)
        {
            DebugLog($"Started Act() with {context.Peer}");

            PlayerClient actingPlayer;
            if (Player1 != null && context.Peer == Player1.PeerName)
            {
                Player1.RequestStream = requestStream;
                Player1.ResponseStream = responseStream;
                Player1.Context = context;
                actingPlayer = Player1;
                
                _activations.Enqueue(Player1);
            }
            else if (Player2 != null && context.Peer == Player2.PeerName)
            {
                Player2.RequestStream = requestStream;
                Player2.ResponseStream = responseStream;
                Player2.Context = context;
                actingPlayer = Player2;

                _activations.Enqueue(Player2);
            }
            else
            {
                throw new ArgumentException("Unknown Act-call");
            }
            
            // Inform that player streams received
            _actSemaphore.Release();
            await actingPlayer.StreamingTask;
            DebugLog($"Streaming task finished for {actingPlayer.Information.Name}");
        }
    }

    internal class PlayerClient
    {
        /// <summary>
        /// 0 or 1
        /// </summary>
        public int PlayerIndex { get; set; }

        public IAsyncStreamReader<Move>? RequestStream { get; set; } = null;
        public IServerStreamWriter<Move>? ResponseStream { get; set; } = null;
        public ServerCallContext? Context { get; set; } = null;
        public bool StreamOpened => RequestStream != null && ResponseStream != null;
        public string PeerName { get; set; } = "";

        /// <summary>
        /// Void task that finishes when game is finished.
        /// TODO: this infinite loop is not the proper way
        /// </summary>
        public Task StreamingTask { get; set; } = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                // 
            }
        });

        //private TaskCompletionSource _tcs = new TaskCompletionSource();
        
        
        
        // Temp
        public GameInformation Information { get; set; } = new GameInformation();
        public Move LatestMove { get; set; } = new Move();
        public string PrintLatest()
        {
            var message = $"{LatestMove.Chess.StartPosition} to {LatestMove.Chess.EndPosition}";
            return message;
        }
    }
}
