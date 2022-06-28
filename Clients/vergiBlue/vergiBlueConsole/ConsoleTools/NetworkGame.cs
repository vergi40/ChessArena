using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonNetStandard;
using CommonNetStandard.Client;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using CommonNetStandard.LocalImplementation;
using CommonNetStandard.Logging;
using Microsoft.Extensions.Logging;
using vergiBlue.Analytics;
using vergiBlue.Logic;

namespace vergiBlue.ConsoleTools
{
    class NetworkGame
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<NetworkGame>();
        private static void Log(string message) => _logger.LogInformation(message);
        public static void Start(IGrpcClientConnection grpcClientConnection, string playerName, bool connectionTesting)
        {
            // We could use while(true) to play games indefinitely. But probably better to play single game per opened client
            try
            {
                // https://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c
                AsyncHelper.RunSync(() => MainGameLoop(grpcClientConnection, playerName, connectionTesting));

                // Exception deadlock
                //var task = MainGameLoop(grpcClientConnection, playerName, connectionTesting);
                //task.Wait();

                Log("Game ended, reason: ____TODO____");
            }
            catch (GameEndedException e)
            {
                Log($"Game ended, reason: {e.E.Status.Detail}");
            }
            catch (Exception e)
            {
                Log($"Main game loop failed: {e.ToString()}");
            }
        }

        public static async Task MainGameLoop(IGrpcClientConnection grpcClientConnection, string playerName, bool connectionTesting)
        {
            Log(Environment.NewLine);
            var startInformation = await grpcClientConnection.Initialize(playerName);
            
            Log($"Received game start information.");
            if (startInformation.WhitePlayer) Log($"{playerName} starts the game.");
            else Log($"Opponent starts the game.");

            Log(Environment.NewLine);

            Log("Starting logic...");
            LogicBase ai;
            if (connectionTesting) ai = new ConnectionTesterLogic(startInformation.WhitePlayer);
            else ai = LogicFactory.Create(startInformation);

            Log("Start game loop");

            // Inject ai to connection module and play game
            // TODO cancellation token here
            await grpcClientConnection.Play(ai);
        }

        internal static class AsyncHelper
        {
            private static readonly TaskFactory _myTaskFactory = new
                TaskFactory(CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);

            public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            {
                return AsyncHelper._myTaskFactory
                    .StartNew<Task<TResult>>(func)
                    .Unwrap<TResult>()
                    .GetAwaiter()
                    .GetResult();
            }

            public static void RunSync(Func<Task> func)
            {
                AsyncHelper._myTaskFactory
                    .StartNew<Task>(func)
                    .Unwrap()
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }

    /// <summary>
    /// Sends dummy moves
    /// </summary>
    class ConnectionTesterLogic : LogicBase
    {
        private int _currentIndex;
        private readonly int _direction;

        private int NextIndex
        {
            get
            {
                var value = _currentIndex + _direction;
                _currentIndex += _direction;
                return value;
            }
        }

        public ConnectionTesterLogic(bool isPlayerWhite) : base(isPlayerWhite)
        {
            if (isPlayerWhite)
            {
                _currentIndex = 1;
                _direction = 1;
            }
            else
            {
                _currentIndex = 6;
                _direction = -1;
            }
        }

        public override IPlayerMove CreateMove()
        {
            var diagnostics = Collector.Instance.CollectAndClear();
            // Dummy moves for connection testing
            var move = new PlayerMoveImplementation(
                new MoveImplementation()
                {
                    StartPosition = $"a{_currentIndex}",
                    EndPosition = $"a{NextIndex}",
                    PromotionResult = PromotionPieceType.NoPromotion
                },
                diagnostics.ToString());

            return move;
        }

        public override void ReceiveMove(IMove opponentMove)
        {
            // Do nothing
        }
    }
}
