﻿using System;
using System.Threading.Tasks;
using CommonNetStandard.Common;
using CommonNetStandard.Interface;
using CommonNetStandard.LocalImplementation;
using CommonNetStandard.Logging;
using GameManager;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace CommonNetStandard.Client
{
    /// <summary>
    /// Reference this to create and maintain new grpc connection.
    /// 1. <see cref="Initialize"/> server that you are ready to start a game
    /// 2. <see cref="Play"/> to start pingpong with <see cref="IAiClient.CreateMove"/> and <see cref="IAiClient.ReceiveMove"/>
    /// </summary>
    public interface IGrpcClientConnection : IDisposable
    {
        /// <summary>
        /// Open channel and send initialization request
        /// </summary>
        /// <param name="playerName"></param>
        Task<IGameStartInformation> Initialize(string playerName);

        Task Play(IAiClient ai);

        Task<PingMessage> Ping();
    }

    public static class GrpcClientConnectionFactory
    {
        public static IGrpcClientConnection Create(string address)
        {
            return new GrpcClientConnection(address);
        }
    }
    
    public sealed class GrpcClientConnection : IGrpcClientConnection
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<GrpcClientConnection>();
        private string _aiName = "";
        private readonly string _address;
        private readonly GrpcChannel _channel;
        private readonly ClientImplementation _client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">ip:port</param>
        public GrpcClientConnection(string address)
        {
            _address = address;
            _channel = GrpcChannel.ForAddress(address);
            _client = new ClientImplementation(new GameService.GameServiceClient(_channel));
        }

        public async Task<PingMessage> Ping()
        {
            var pingRequest = new PingMessage()
            {
                Message = "Ping"
            };

            return await _client.Ping(pingRequest);
        }

        /// <summary>
        /// Open channel and send initialization request
        /// </summary>
        /// <param name="playerName"></param>
        public async Task<IGameStartInformation> Initialize(string playerName)
        {
            _aiName = playerName;
            _logger.LogInformation($"Opening gRPC channel to {_address}");
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

        public async Task Play(IAiClient ai)
        {
            // TODO handle exceptions and game end
            await _client.CreateMovements(ai);
        }

        private void CloseConnection()
        {
            _channel.ShutdownAsync().Wait();
        }

        public void Dispose()
        {
            CloseConnection();
        }
    }
}
