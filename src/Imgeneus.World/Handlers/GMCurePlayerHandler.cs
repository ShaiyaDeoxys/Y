using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMCurePlayerHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IBuffsManager _buffsManager;

        public GMCurePlayerHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IBuffsManager buffsManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _buffsManager = buffsManager;
        }

        [HandlerAction(PacketType.GM_CURE_PLAYER)]
        public void HandleOriginal(WorldClient client, GMCurePlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var target = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;

            if (target == null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CURE_PLAYER);
                return;
            }

            target.HealthManager.FullRecover();
            target.SkillsManager.Cooldowns.Clear();

            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_CURE_PLAYER)]
        public void HandleUs(WorldClient client, GMCurePlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var target = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;

            if (target == null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_CURE_PLAYER);
                return;
            }

            target.HealthManager.FullRecover();
            target.SkillsManager.Cooldowns.Clear();

            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_CLEAR_BUFFS)]
        public void HandleClearBuffs(WorldClient client, GMCurePlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (string.IsNullOrEmpty(packet.Name))
                return;

            var target = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;
            if (target == null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CLEAR_BUFFS);
                return;
            }

            target.HealthManager.FullRecover();
            target.SkillsManager.Cooldowns.Clear();
            foreach(var buff in _buffsManager.ActiveBuffs.ToList())
            {
                buff.CancelBuff();
            }

            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
