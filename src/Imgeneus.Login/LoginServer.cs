using Imgeneus.Network.Server;
using InterServer.Client;
using InterServer.Server;
using LiteNetwork.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.Login
{
    public sealed class LoginServer : LiteServer<LoginClient>, ILoginServer
    {
        private readonly ILogger<LoginServer> _logger;
        private readonly IInterServer _interServer;

        /// <summary>
        /// Gets the list of the connected worlds.
        /// </summary>
        public IEnumerable<WorldServerInfo> ClustersConnected => _interServer.WorldServers;

        public LoginServer(ILogger<LoginServer> logger, IOptions<ImgeneusServerOptions> tcpConfiguration, IServiceProvider serviceProvider, IInterServer interServer)
            : base(tcpConfiguration.Value, serviceProvider)
        {
            _logger = logger;
            _interServer = interServer;
        }

        protected override void OnAfterStart()
        {
            _logger.LogInformation("Login server is listening on {port}...", Options.Port);
        }

        /// <inheritdoc />
        public IEnumerable<WorldServerInfo> GetConnectedWorlds() => _interServer.WorldServers;

        public WorldServerInfo GetWorldByID(byte id)
        {
            return _interServer.WorldServers.FirstOrDefault(x => x.Id == id);
        }

        public LoginClient GetClientByUserID(int userID)
        {
            return ConnectedUsers.FirstOrDefault(x => x.UserId == userID);
        }

        public bool IsClientConnected(int userID)
        {
            return GetClientByUserID(userID) != null;
        }
    }
}
