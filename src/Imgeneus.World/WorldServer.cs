using Imgeneus.Core.Structures.Configuration;
using Imgeneus.InterServer.Common;
using Imgeneus.Network.Server;
using Imgeneus.World.Game;
using InterServer.Client;
using InterServer.SignalR;
using LiteNetwork.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Imgeneus.World
{
    public sealed class WorldServer : LiteServer<WorldClient>, IWorldServer
    {
        private readonly ILogger<WorldServer> _logger;
        private readonly WorldConfiguration _worldConfiguration;
        private readonly IInterServerClient _interClient;
        private readonly IGameWorld _gameWorld;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public WorldServer(ILogger<WorldServer> logger, IOptions<ImgeneusServerOptions> tcpConfiguration, IServiceProvider serviceProvider, IOptions<WorldConfiguration> worldConfiguration, IInterServerClient interClient, IGameWorld gameWorld, IHostApplicationLifetime applicationLifetime)
            : base(tcpConfiguration.Value, serviceProvider)
        {
            _logger = logger;
            _worldConfiguration = worldConfiguration.Value;
            _interClient = interClient;
            _gameWorld = gameWorld;
            _applicationLifetime = applicationLifetime;

            _interClient.OnConnected += SendWorldInfo;

            _applicationLifetime.ApplicationStopping.Register(() => {
                _logger.LogInformation("Application stopped. Server is stopping too.");
                Stop();
            });
        }
        protected override void OnBeforeStart()
        {
            _gameWorld.Init();
            _interClient.Connect();
        }

        protected override void OnAfterStart()
        {
            _logger.LogTrace("Host: {0}, Port: {1}, MaxNumberOfConnections: {2}",
                Options.Host,
                Options.Port,
                Options.Backlog);

            _interClient.Send(new ISMessage(ISMessageType.WORLD_STATE, new WorldStateChanged(_worldConfiguration.Name, IsRunning)));
        }

        protected override void OnClientConnected(WorldClient client)
        {
            _interClient.Send(new ISMessage(ISMessageType.NUMBER_OF_CONNECTED_PLAYERS, new NumberOfConnectedUsers(_worldConfiguration.Name, (ushort)ConnectedUsers.Count)));
        }

        protected override void OnClientDisconnected(WorldClient client)
        {
            _interClient.Send(new ISMessage(ISMessageType.NUMBER_OF_CONNECTED_PLAYERS, new NumberOfConnectedUsers(_worldConfiguration.Name, (ushort)ConnectedUsers.Count)));
        }

        private void SendWorldInfo()
        {
            _interClient.Send(new ISMessage(ISMessageType.WORLD_INFO, _worldConfiguration));
        }

        protected override void OnAfterStop()
        {
            _interClient.Send(new ISMessage(ISMessageType.WORLD_STATE, new WorldStateChanged(_worldConfiguration.Name, IsRunning)));
        }
    }
}
