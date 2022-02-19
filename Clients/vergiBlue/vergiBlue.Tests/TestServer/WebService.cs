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
        private int _sentMoveCount = 0;

        public WebServer(SharedData shared)
        {
            _shared = shared;
            shared.MoveHistory.OnAdd += HandleNewMove;
        }

        void HandleNewMove(object? sender, EventArgs e)
        {
            // Would be better to use events to signal new moves to stream.
            // But design not clear yet.
        }

        public override Task<PingMessage> Ping(PingMessage request, ServerCallContext context)
        {
            _logger.Info("Ping request received.");
            _pingReceived = true;

            var response = new PingMessage { Message = "pong" };
            return Task.FromResult(response);
        }

        public override async Task ListenMoveUpdates(PingMessage request, IServerStreamWriter<Move> responseStream, ServerCallContext context)
        {
            _logger.Info($"{nameof(ListenMoveUpdates)} request received.");
            if (!_pingReceived)
            {
                _logger.Info($"Did not receive initializing ping request before {nameof(ListenMoveUpdates)}. Cancelling stream.");
                return;
            }

            _logger.Info("Starting move streaming to web backend...");
            while (true)
            {
                try
                {
                    if (_shared.CurrentMoveCount > _sentMoveCount)
                    {
                        var move = _shared.MoveHistory[_sentMoveCount];
                        await responseStream.WriteAsync(move);
                        _logger.Info($"Sent to backend: {PrintMove(move)}");
                        _sentMoveCount++;
                    }

                    await Task.Delay(_shared.CycleDelayInMs);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error occured, stopping streaming to web client.");
                    break;
                }
            }

            return;
        }

        public string PrintMove(Move move)
        {
            var message = $"{move.Chess.StartPosition} to {move.Chess.EndPosition}";
            return message;
        }
    }
}
