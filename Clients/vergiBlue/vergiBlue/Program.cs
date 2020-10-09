using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace vergiBlue
{
    class Program
    {
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:30052", ChannelCredentials.Insecure);
            var client = new InterfaceImplementation(new MovementStream.MovementStreamClient(channel));

            // YOUR CODE GOES HERE
            client.Initialize();

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }

    class InterfaceImplementation
    {
        readonly MovementStream.MovementStreamClient client;

        public InterfaceImplementation(MovementStream.MovementStreamClient client)
        {
            this.client = client;
        }

        public async void Initialize()
        {
            var information = new ClientInformation()
            {
                Name = "vergiBlue"
            };

            Console.WriteLine("Initializing client... Getting start information from server.");
            var startInformation = await client.InitializeAsync(information);

            Console.WriteLine($"Received info: start player: {startInformation.WhitePlayer}.");

        }
    }
}
