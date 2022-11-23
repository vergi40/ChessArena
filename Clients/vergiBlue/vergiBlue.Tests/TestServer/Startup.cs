using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestServer.Services;

namespace TestServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddGrpc();
            // TODO
            //services.AddControllers();

            // DI
            services.AddSingleton<IServiceShared, ServiceShared>();

            // Lesson learned: logger not actuated yet, can't use in Startup
            //_logger.LogInformation("Services added");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseHttpsRedirection();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ChessGameService>();

                // Configure the HTTP request pipeline.
                // TODO
                // endpoints.MapControllers();

            });

            //_logger.LogInformation("Services configured");
        }
    }
}
