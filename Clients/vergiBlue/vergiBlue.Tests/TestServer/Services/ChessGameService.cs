using System.Threading.Tasks;
using GameManager;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace TestServer.Services
{
    public class ChessGameService : GameService.GameServiceBase
    {
        private readonly IServiceShared _shared;
        private readonly ILogger _logger;

        public ChessGameService(IServiceShared shared, ILogger<ChessGameService> logger)
        {
            _shared = shared;
            _logger = logger;
            _logger.LogInformation("Chess game service created");
        }

        public override Task<PingMessage> Ping(PingMessage request, ServerCallContext context)
        {
            _logger.LogInformation($"Client {context.Peer} sent {request.Message}");
            _logger.LogInformation($"Responding with \"pong\"");

            var response = new PingMessage()
            {
                Message = "Pong"
            };

            return Task.FromResult(response);
        }
    }
}
