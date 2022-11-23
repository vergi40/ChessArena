using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GameManager;
using Grpc.Core;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestServer
{
    public static class Program
    {
        private static readonly Logger _logger = new Logger(typeof(Program));
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<TestServer>();
            app.MapGrpcService<WebServer>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
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
