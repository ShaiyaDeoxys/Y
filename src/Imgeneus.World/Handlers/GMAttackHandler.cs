using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMAttackHandler : BaseHandler
    {
        private readonly IHealthManager _healthManager;

        public GMAttackHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IHealthManager healthManager) : base(packetFactory, gameSession)
        {
            _healthManager = healthManager;
        }

        [HandlerAction(PacketType.GM_ATTACK_ON)]
        public void HandleAttackOn(WorldClient client, GMAttackOnPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _healthManager.IsAttackable = false;
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_ATTACK_OFF)]
        public void HandleAttackOff(WorldClient client, GMAttackOffPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _healthManager.IsAttackable = true;
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
