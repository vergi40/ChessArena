﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace vergiBlue.Connection
{
    class ConnectionModule
    {
        private string _aiName;
        private Channel _channel;

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
            var client = new ClientImplementation(new MovementStream.MovementStreamClient(_channel));

            Logger.Log($"Opening gRPC channel to {address}");

            var startInformation = await client.Initialize(_aiName);
            return startInformation;
        }

        public void CloseConnection()
        {
            _channel.ShutdownAsync().Wait();
        }
    }
}
