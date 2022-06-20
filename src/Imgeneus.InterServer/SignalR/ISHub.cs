using Imgeneus.Core.Structures.Configuration;
using Imgeneus.InterServer.Common;
using InterServer.Client;
using InterServer.Common;
using InterServer.Server;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace InterServer.SignalR
{
    public class ISHub : Hub
    {
        private readonly ILogger<ISHub> _logger;
        private readonly IInterServer _interServer;

        private static readonly Dictionary<ISMessageType, string> _messageTypeToMethodName = new Dictionary<ISMessageType, string>() {
            { ISMessageType.WORLD_INFO, nameof(ISHub.WorldServerConnected) },
            { ISMessageType.AES_KEY_REQUEST, nameof(ISHub.GetAesKey) },
            { ISMessageType.AES_KEY_RESPONSE, nameof(ISClient.OnAesKeyResponse) },
            { ISMessageType.NUMBER_OF_CONNECTED_PLAYERS_CHANGED, nameof(ISHub.NumberOfConnectedPlayersChanged) }
        };

        /// <summary>
        /// SignalR message to method name connection.
        /// </summary>
        public static readonly ReadOnlyDictionary<ISMessageType, string> MessageTypeToMethodName = new ReadOnlyDictionary<ISMessageType, string>(_messageTypeToMethodName);

        public ISHub(ILogger<ISHub> logger, IInterServer interServer) : base()
        {
            _logger = logger;
            _interServer = interServer;
        }

        public void WorldServerConnected(WorldConfiguration config)
        {
            var worldInfo = new WorldServerInfo(
                    (byte)_interServer.WorldServers.Count,
                    IPAddress.Parse(config.Host).GetAddressBytes(),
                    config.Name,
                    config.BuildVersion,
                    config.MaximumNumberOfConnections);
            _interServer.AddWorldServer(worldInfo);
        }

        public void GetAesKey(Guid sessionId)
        {
            _interServer.Sessions.TryRemove(sessionId, out var keyPair);
            Clients.Caller.SendAsync(MessageTypeToMethodName[ISMessageType.AES_KEY_RESPONSE], new SessionResponse(sessionId, keyPair));
        }

        public void NumberOfConnectedPlayersChanged(NumberOfConnectedUsers payload)
        {
            var serverInfo = _interServer.WorldServers.FirstOrDefault(x => x.Name == payload.ServerName);
            if (serverInfo is null)
                return;

            serverInfo.ConnectedUsers = payload.NumberOfPlayers;
        }
    }
}
