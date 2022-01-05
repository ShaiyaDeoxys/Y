using Imgeneus.Database;
using Imgeneus.Login.Packets;
using Imgeneus.Network.Server;
using Imgeneus.Network.Server.Crypto;
using InterServer.Server;
using InterServer.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sylver.HandlerInvoker;

namespace Imgeneus.Login
{
    public class LoginServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add options.
            services.AddOptions<ImgeneusServerOptions>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("TcpServer").Bind(settings));
            services.AddOptions<DatabaseConfiguration>()
               .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Database").Bind(settings));

            services.RegisterDatabaseServices();
            services.AddSignalR();
            services.AddHandlers();

            services.AddSingleton<IInterServer, ISServer>();
            services.AddSingleton<ILoginServer, LoginServer>();
            services.AddSingleton<ILoginPacketFactory, LoginPacketFactory>();
            services.AddTransient<ICryptoManager, CryptoManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoginServer loginServer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ISHub>("/inter_server");
            });

            loginServer.Start();
        }
    }
}
