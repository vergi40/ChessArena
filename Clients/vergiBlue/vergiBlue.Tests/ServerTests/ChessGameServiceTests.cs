using GameManager;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TestServer.Services;

namespace ServerTests
{
    public class ChessGameServiceTests
    {
        private Mock<IServiceShared> _shared;
        private ChessGameService _service;

        [SetUp]
        public void Setup()
        {
            _shared = new Mock<IServiceShared>();

            _service = new ChessGameService(_shared.Object, NullLogger<ChessGameService>.Instance);
        }

        [Test]
        public async Task Ping_MessageWithPing_ShouldReturnPong()
        {
            var request = new PingMessage()
            {
                Message = "ping"
            };

            var response = await _service.Ping(request, TestServerCallContext.Create());

            Assert.That(response.Message, Is.EqualTo("Pong"));
        }

        [Test]
        public async Task Ping_AnyContent_ShouldReturnPong()
        {
            var request = new PingMessage()
            {
                Message = "testing with any content"
            };

            var response = await _service.Ping(request, TestServerCallContext.Create());

            Assert.That(response.Message, Is.EqualTo("Pong"));
        }
    }
}