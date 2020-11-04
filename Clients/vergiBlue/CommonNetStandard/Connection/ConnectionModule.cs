using System.Threading.Tasks;
using CommonNetStandard.Interface;
using CommonNetStandard.Local_implementation;
using GameManager;
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
        private string _aiName = "";
        private string _address;
        private Channel _channel;
        private ClientImplementation _client;

        public ConnectionModule(string address)
        {
            _address = address;
            _channel = new Channel(address, ChannelCredentials.Insecure);
            _client = new ClientImplementation(new GameService.GameServiceClient(_channel));
        }

        /// <summary>
        /// Open channel and send initialization request
        /// </summary>
        /// <param name="address">ip:port</param>
        /// <param name="playerName"></param>
        public async Task<IGameStartInformation> Initialize(string playerName)
        {
            _aiName = playerName;
            Logger.Log($"Opening gRPC channel to {_address}");
            var startInformation = await _client.Initialize(playerName);
            var localInformation = new StartInformationImplementation()
            {
                WhitePlayer = startInformation.Start
            };

            // Has move data only if player is black
            if (!localInformation.WhitePlayer)
            {
                localInformation.OpponentMove = Mapping.ToCommon(startInformation.ChessMove);
            }
            

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
