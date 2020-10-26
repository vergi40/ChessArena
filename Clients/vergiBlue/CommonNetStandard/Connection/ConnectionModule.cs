using System.Threading.Tasks;
using CommonNetStandard.Interface;
using CommonNetStandard.Local_implementation;
using Grpc.Core;

namespace CommonNetStandard.Connection
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

        public ConnectionModule()
        {
        }

        /// <summary>
        /// Open channel and send initialization request
        /// </summary>
        /// <param name="address">ip:port</param>
        /// <param name="playerName"></param>
        public async Task<IGameStartInformation> Initialize(string address, string playerName)
        {
            _aiName = playerName;
            _channel = new Channel(address, ChannelCredentials.Insecure);
            _client = new ClientImplementation(new ChessArena.ChessArenaClient(_channel));

            Logger.Log($"Opening gRPC channel to {address}");

            var startInformation = await _client.Initialize(playerName);
            var localInformation = new StartInformationImplementation()
            {
                WhitePlayer = startInformation.WhitePlayer,
                OpponentMove = Mapping.ToCommon(startInformation.OpponentMove)
            };
            return localInformation;
        }

        public async Task Play(LogicBase ai)
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
