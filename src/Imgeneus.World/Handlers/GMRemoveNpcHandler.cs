using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMRemoveNpcHandler : BaseHandler
    {
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;

        public GMRemoveNpcHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_REMOVE_NPC)]
        public void HandleOriginal(WorldClient client, GMRemoveNpcPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            Handle(client, packet);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_REMOVE_NPC)]
        public void HandleUs(WorldClient client, GMRemoveNpcPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            Handle(client, packet);
        }

        private void Handle(WorldClient client, GMRemoveNpcPacket packet)
        {
            var cellId = _gameWorld.Players[_gameSession.Character.Id].CellId;
            _mapProvider.Map.RemoveNPC(cellId, packet.Type, packet.TypeId, packet.Count);

            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
