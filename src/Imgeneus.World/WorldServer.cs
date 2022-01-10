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
        }

        private void SendWorldInfo()
        {
            _interClient.OnConnected -= SendWorldInfo;
            _interClient.Send(new ISMessage(ISMessageType.WORLD_INFO, _worldConfiguration));

            //_interClient.OnSessionResponse += LoadSelectionScreen;
        }

        /// <inheritdoc />
        /*protected async override void OnClientDisconnected(WorldClient client)
        {
            base.OnClientDisconnected(client);

            SelectionScreenManagers.Remove(client.Id, out var manager);
            manager.Dispose();
            client.OnPacketArrived -= Client_OnPacketArrived;

            if (_gameWorld.Players.ContainsKey(client.CharID))
            {
                await Task.Delay(1000 * 10);
                _gameWorld.RemovePlayer(client.CharID);
            }
        }*/

        /// <inheritdoc />
        /*protected override void OnClientConnected(WorldClient client)
        {
            base.OnClientConnected(client);

            client.OnPacketArrived += Client_OnPacketArrived;
        }

        private async void Client_OnPacketArrived(ServerClient sender, IDeserializedPacket packet)
        {
            if (packet is HandshakePacket)
            {
                var handshake = (HandshakePacket)packet;
                (sender as WorldClient).SetClientUserID(handshake.UserId);

                clients.TryRemove(sender.Id, out var client);

                // Now give client new id.
                client.Id = handshake.SessionId;

                // Return client back to dictionary.
                clients.TryAdd(client.Id, client);
                SelectionScreenManagers.Add(client.Id, _selectionScreenFactory.CreateSelectionManager(client));

                // Send request to login server and get client key.
                await _interClient.Send(new ISMessage(ISMessageType.AES_KEY_REQUEST, sender.Id));
            }

            if (packet is PingPacket)
            {
                // TODO: implement disconnect, if client is not sending ping packet.
            }

            if (packet is CashPointPacket)
            {
                // TODO: implement cash point packet.
                using var dummyPacket = new Packet(PacketType.CASH_POINT);
                dummyPacket.Write(0);
                sender.SendPacket(dummyPacket);
            }

            if (packet is MotionPacket)
            {
                (sender as WorldClient).IsLoggingOff = false;
            }

            if (packet is LogOutPacket)
            {
                var worldClient = sender as WorldClient;
                worldClient.IsLoggingOff = true;

                // TODO: For sure, here should be timer!
                await Task.Delay(1000 * 10); // 10 seconds * 1000 milliseconds

                if (sender.IsDispose || !worldClient.IsLoggingOff)
                    return;

                _gameWorld.RemovePlayer(worldClient.CharID);

                using var logoutPacket = new Packet(PacketType.LOGOUT);
                sender.SendPacket(logoutPacket);

                sender.CryptoManager.UseExpandedKey = false;

                if (SelectionScreenManagers.ContainsKey(sender.Id))
                    SelectionScreenManagers[sender.Id].SendSelectionScrenInformation(((WorldClient)sender).UserID);
            }

            if (packet is QuitGamePacket)
            {
                var worldClient = sender as WorldClient;
                worldClient.IsLoggingOff = true;

                // TODO: For sure, here should be timer!
                await Task.Delay(1000 * 10); // 10 seconds * 1000 milliseconds

                if (sender.IsDispose || !worldClient.IsLoggingOff)
                    return;

                _gameWorld.RemovePlayer(worldClient.CharID);

                using var logoutPacket = new Packet(PacketType.QUIT_GAME);
                sender.SendPacket(logoutPacket);
            }
        }
        */
    }
}
