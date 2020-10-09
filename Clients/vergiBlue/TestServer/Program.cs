using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 30052;

            Server server = new Server
            {
                Services = { MovementStream.BindService(new InterfaceImplementation()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("RouteGuide server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    class InterfaceImplementation : MovementStream.MovementStreamBase
    {
        public override Task<GameStartInformation> Initialize(ClientInformation request, ServerCallContext context)
        {
            Console.WriteLine($"Client {request.Name} requested initialize.");

            var response = new GameStartInformation()
            {
                WhitePlayer = true
            };
            return Task.FromResult(response);
        }
    }
}
