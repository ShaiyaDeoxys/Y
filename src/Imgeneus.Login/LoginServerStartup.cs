using Imgeneus.Authentication.Connection;
using Imgeneus.Authentication.Context;
using Imgeneus.Authentication.Entities;
using Imgeneus.Core.Structures.Configuration;
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
using Microsoft.AspNetCore.Identity;
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
            services.AddOptions<UsersDatabaseConfiguration>()
               .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("UsersDatabase").Bind(settings));
            services.AddOptions<LoginConfiguration>()
                .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("LoginServer").Bind(settings));

            services.RegisterDatabaseServices();
            services.RegisterUsersDatabaseServices();

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
                options.Password.RequireDigit = false;
            })
                .AddRoles<DbRole>()
                .AddEntityFrameworkStores<UsersContext>();

            services.AddCors(p => p.AddPolicy("corsapp", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoginServer loginServer, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IConfiguration configuration, IUsersDatabase mainDb, RoleManager<DbRole> roleManager)
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
            app.UseCors("corsapp");
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

            mainDb.Migrate();

            roleManager.CreateAsync(new DbRole() { Name = DbRole.SUPER_ADMIN }).Wait();
            roleManager.CreateAsync(new DbRole() { Name = DbRole.ADMIN }).Wait();
            roleManager.CreateAsync(new DbRole() { Name = DbRole.USER }).Wait();

            loginServer.Start();
        }
    }
}
