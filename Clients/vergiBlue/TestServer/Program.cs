using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GameManager;
using Grpc.Core;

namespace TestServer
{
    class Program
    {
        private static readonly Logger _logger = new Logger(typeof(Program));
        static void Main(string[] args)
        {
            const int Port = 30052;
            var data = new SharedData();

            Server server = new Server
            {
                Services =
                {
                    GameService.BindService(new TestServer(data)),
                    WebService.BindService(new WebServer(data))
                },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            _logger.Info("vergiBlue test server listening on port " + Port);
            _logger.Info("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    /// <summary>
    /// By registering to <see cref="OnAdd"/>, list changes can be tracked.
    /// https://stackoverflow.com/questions/1299920/how-to-handle-add-to-list-event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TrackedList<T> : List<T>
    {
        public event EventHandler? OnAdd;

        public new void Add(T item)
        {
            if (OnAdd != null)
            {
                OnAdd(this, EventArgs.Empty);
            }
            base.Add(item);
        }
    }

    /// <summary>
    /// Data shared between services
    /// </summary>
    class SharedData
    {
        public int CycleDelayInMs { get; } = 100;

        public TrackedList<Move> MoveHistory = new TrackedList<Move>();

        public int CurrentMoveCount => MoveHistory.Count;
        public int CurrentWebIndex { get; set; } = 0;

        public void ResetGame()
        {
            MoveHistory = new TrackedList<Move>();
        }
    }

    

    
}
