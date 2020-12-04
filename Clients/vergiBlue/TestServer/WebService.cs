using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameManager;
using Grpc.Core;

namespace TestServer
{
    class WebServer : WebService.WebServiceBase
    {
        private static readonly Logger _logger = new Logger(typeof(WebServer));
        public SharedData _shared { get; }

        private bool _pingReceived = false;

        public WebServer(SharedData shared)
        {
            _shared = shared;
            shared.MoveHistory.OnAdd += HandleNewMove;
        }

        void HandleNewMove(object? sender, EventArgs e)
        {
            // 
        }

        public override Task<PingMessage> Ping(PingMessage request, ServerCallContext context)
        {
            _logger.Info("Ping request received.");
            _pingReceived = true;

            var response = new PingMessage { Message = "pong" };
            return Task.FromResult(response);
        }

        public override Task ListenMoveUpdates(PingMessage request, IServerStreamWriter<Move> responseStream, ServerCallContext context)
        {
            _logger.Info($"{nameof(ListenMoveUpdates)} request received.");
            if (!_pingReceived)
            {
                _logger.Info($"Did not receive initializing ping request before {nameof(ListenMoveUpdates)}. Cancelling stream.");
                return Task.CompletedTask;
            }

            _logger.Info("Starting move streaming to web backend...");


            return base.ListenMoveUpdates(request, responseStream, context);
        }
    }
}
