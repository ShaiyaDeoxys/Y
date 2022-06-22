using Imgeneus.Database;
using Imgeneus.Database.Context;
using Imgeneus.Database.Entities;
using Imgeneus.Login.Packets;
using Imgeneus.Monitoring;
using Imgeneus.Network.Server;
using Imgeneus.Network.Server.Crypto;
using InterServer.Server;
using InterServer.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker;
using System;

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

            // Add admin website
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDefaultIdentity<DbUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredLength = 1;
            })
                .AddRoles<DbRole>()
                .AddEntityFrameworkStores<DatabaseContext>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoginServer loginServer, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            loggerFactory.AddProvider(
                new SignalRLoggerProvider(
                    new SignalRLoggerConfiguration
                    {
                        HubContext = serviceProvider.GetService<IHubContext<MonitoringHub>>(),
                        LogLevel = configuration.GetValue<LogLevel>("Logging:LogLevel:Monitoring")
                    }));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<ISHub>("/inter_server");
                endpoints.MapHub<MonitoringHub>(MonitoringHub.HubUrl);
            });

            loginServer.Start();
        }
    }
}
