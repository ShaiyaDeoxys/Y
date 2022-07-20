using Imgeneus.Core.Structures.Configuration;
using Imgeneus.InterServer.Common;
using InterServer.Client;
using InterServer.Common;
using InterServer.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
            { ISMessageType.NUMBER_OF_CONNECTED_PLAYERS, nameof(ISHub.NumberOfConnectedPlayersChanged) },
            { ISMessageType.WORLD_STATE, nameof(WorldStateChanged) }
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

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var serverInfo = _interServer.WorldServers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (serverInfo is null)
                return Task.CompletedTask;

            _interServer.RemoveWorldServer(serverInfo);

            return Task.CompletedTask;
        }

        public override Task OnConnectedAsync()
        {
            // get relative client info from headers
            var context = Context.Features.Get<IHttpContextFeature>();
            var host = context.HttpContext.Request.Headers["Host"];
            var userAgent = context.HttpContext.Request.Headers["User-Agent"];
            var realIP = context.HttpContext.Request.Headers["X-Real-IP"];
            var forwardeds = context.HttpContext.Request.Headers["X-Forwarded-For"];
            var connectedInfo = new Dictionary<string, string>() { { "Host", host }, { "UserAgent", userAgent }, { "Real-IP", realIP }, { "Forward-For", forwardeds },};
            _logger.LogInformation($"{JsonConvert.SerializeObject(connectedInfo)}");

            // for example just passing through, 
            // you could handle your controlling logic here
            return base.OnConnectedAsync();
        }

        public void WorldServerConnected(WorldConfiguration config)
        {
            _logger.LogWarning($"Got connection from: {Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress}");
            var worldInfo = new WorldServerInfo(
                    (byte)_interServer.WorldServers.Count,
                    Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.GetAddressBytes(),
                    config.Name,
                    config.BuildVersion,
                    config.MaximumNumberOfConnections,
                    Context.ConnectionId);
            _interServer.AddWorldServer(worldInfo);
        }

        public void GetAesKey(Guid sessionId)
        {
            _interServer.Sessions.TryRemove(sessionId, out var keyPair);
            Clients.Caller.SendAsync(MessageTypeToMethodName[ISMessageType.AES_KEY_RESPONSE], new SessionResponse(sessionId, keyPair));
        }

        public void NumberOfConnectedPlayersChanged(NumberOfConnectedUsers payload)
        {
            var serverInfo = _interServer.WorldServers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (serverInfo is null)
                return;

            serverInfo.ConnectedUsers = payload.NumberOfPlayers;
        }

        public void WorldStateChanged(WorldStateChanged payload)
        {
            var serverInfo = _interServer.WorldServers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (serverInfo is null)
                return;

            serverInfo.WorldStatus = payload.IsRunning ? WorldState.Normal : WorldState.Lock;
        }
    }
}
