using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonNetStandard;
using CommonNetStandard.Client;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;

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
            LogicBase ai;
            if (connectionTesting) ai = new ConnectionTesterLogic(startInformation.Result.WhitePlayer);
            else  ai = new Logic(startInformation.Result);

            Log("Start game loop");

            // Inject ai to connection module and play game
            var playTask = grpcClientConnection.Play(ai);
            playTask.Wait();
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
            var diagnostics = Diagnostics.CollectAndClear();
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
