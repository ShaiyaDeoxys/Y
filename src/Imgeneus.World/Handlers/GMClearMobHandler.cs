using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMClearMobHandler : BaseHandler
    {
        private readonly IMapProvider _mapProvider;

        public GMClearMobHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_CLEARE_MOB_IN_TARGET)]
        public void HandleClearMobInTarget(WorldClient client, GMClearMobInTargetPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (_mapProvider.Map is null)
                return;

            var mob = _mapProvider.Map.GetMob(_mapProvider.CellId, packet.Id);
            if (mob is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_CLEARE_MOB_IN_TARGET);
                return;
            }

            _mapProvider.Map.RemoveMob(mob);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
