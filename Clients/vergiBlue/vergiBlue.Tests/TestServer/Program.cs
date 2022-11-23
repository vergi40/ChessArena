using System;
using System.Collections.Generic;
using GameManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args);
            var builder = CreateHostBuilder(args);
            var host = builder.Build();
            host.Run();

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        }

        // https://stackoverflow.com/questions/58649775/can-i-combine-a-grpc-and-webapi-app-into-a-net-core-3-0-in-c
        // https://learn.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-3.0#unable-to-start-aspnet-core-grpc-app-on-macos-2
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-7.0#listenoptionsusehttps
                    webBuilder.ConfigureKestrel(options =>
                    {
                        // Setup a HTTP/2 endpoint without TLS. Grpc
                        options.ListenLocalhost(5001, o => o.Protocols =
                            HttpProtocols.Http2);

                        // WebAPI
                        options.ListenLocalhost(8001, o => o.Protocols =
                            HttpProtocols.Http2);
                    });
                    webBuilder.UseStartup<Startup>();
                });

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
