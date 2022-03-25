using Imgeneus.Core.Structures.Configuration;
using Imgeneus.Network.Server;
using Imgeneus.World.Game;
using InterServer.Client;
using InterServer.SignalR;
using LiteNetwork.Server;
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

        public WorldServer(ILogger<WorldServer> logger, IOptions<ImgeneusServerOptions> tcpConfiguration, IServiceProvider serviceProvider, IOptions<WorldConfiguration> worldConfiguration, IInterServerClient interClient, IGameWorld gameWorld)
            : base(tcpConfiguration.Value, serviceProvider)
        {
            _logger = logger;
            _worldConfiguration = worldConfiguration.Value;
            _interClient = interClient;
            _gameWorld = gameWorld;
        }

        protected override void OnAfterStart()
        {
            _logger.LogTrace("Host: {0}, Port: {1}, MaxNumberOfConnections: {2}",
                Options.Host,
                Options.Port,
                Options.Backlog);
            _interClient.Connect();
            _interClient.OnConnected += SendWorldInfo;

            _gameWorld.Init();
        }

        private void SendWorldInfo()
        {
            _interClient.OnConnected -= SendWorldInfo;
            _interClient.Send(new ISMessage(ISMessageType.WORLD_INFO, _worldConfiguration));
        }
    }
}
