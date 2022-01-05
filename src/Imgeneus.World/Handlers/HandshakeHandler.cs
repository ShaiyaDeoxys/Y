using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using InterServer.Client;
using InterServer.Common;
using InterServer.SignalR;
using Sylver.HandlerInvoker.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class HandshakeHandler : BaseHandler
    {
        private readonly IWorldServer _server;
        private readonly IInterServerClient _interClient;
        private readonly ISelectionScreenManager _selectionScreenManager;
        private Guid _sessionId;

        public HandshakeHandler(IGamePacketFactory packetFactory, IWorldServer server, IInterServerClient interClient, ISelectionScreenManager selectionScreenManager): base(packetFactory)
        {
            _server = server;
            _interClient = interClient;
            _selectionScreenManager = selectionScreenManager;
            _interClient.OnSessionResponse += InitGameSession;
        }

        [HandlerAction(PacketType.GAME_HANDSHAKE)]
        public async Task Handle(WorldClient client, HandshakePacket packet)
        {
            client.SetClientUserID(packet.UserId);

            _sessionId = client.Id;

            // Send request to login server and get client key.
            await _interClient.Send(new ISMessage(ISMessageType.AES_KEY_REQUEST, packet.SessionId));
        }

        private async void InitGameSession(SessionResponse sessionInfo)
        {
            _interClient.OnSessionResponse -= InitGameSession;

            var worldClient = _server.ConnectedUsers.First(x => x.Id == _sessionId);
            worldClient.CryptoManager.GenerateAES(sessionInfo.KeyPair.Key, sessionInfo.KeyPair.IV);

            _packetFactory.SendGameHandshake(worldClient);

            _packetFactory.SendCharacterList(worldClient, await _selectionScreenManager.GetCharacters(worldClient.UserId));
            _packetFactory.SendFaction(worldClient, await _selectionScreenManager.GetFaction(worldClient.UserId), await _selectionScreenManager.GetMaxMode(worldClient.UserId));
        }
    }
}