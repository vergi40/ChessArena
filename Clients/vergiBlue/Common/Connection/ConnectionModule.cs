using System.Threading.Tasks;
using Grpc.Core;

namespace Common.Connection
{
    /// <summary>
    /// Reference this class to create and maintain new grpc connection.
    /// 1. <see cref="Initialize"/> server that you are ready to start a game
    /// 2. <see cref="Play"/> to start pingpong with <see cref="LogicBase.CreateMove"/> and <see cref="LogicBase.ReceiveMove"/>
    /// </summary>
    public class ConnectionModule
    {
        private string _aiName;
        private Channel _channel;
        private ClientImplementation _client;

        public ConnectionModule(string aiName)
        {
            _aiName = aiName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">ip:port</param>
        public async Task<GameStartInformation> Initialize(string address)
        {

            _channel = new Channel(address, ChannelCredentials.Insecure);
            _client = new ClientImplementation(new ChessArena.ChessArenaClient(_channel));

            Logger.Log($"Opening gRPC channel to {address}");

            var startInformation = await _client.Initialize(_aiName);
            return startInformation;
        }

        public async void Play(LogicBase ai)
        {
            // TODO handle exceptions and game end
            await _client.CreateMovements(ai);
        }

        public void CloseConnection()
        {
            // TODO implement dispose
            _channel.ShutdownAsync().Wait();
        }
    }
}
