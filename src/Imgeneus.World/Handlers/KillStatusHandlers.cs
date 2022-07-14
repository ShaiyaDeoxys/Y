using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class KillStatusHandlers : BaseHandler
    {
        private readonly IKillsManager _killsManager;

        public KillStatusHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IKillsManager killsManager) : base(packetFactory, gameSession)
        {
            _killsManager = killsManager;
        }

        [HandlerAction(PacketType.KILLSTATUS_RESULT_INFO)]
        public void HandleKillStatusInfo(WorldClient client, KillStatusPacket packet)
        {
            _packetFactory.SendKillStatusInfo(client, _killsManager.KillLevel, _killsManager.DeathLevel);
        }

        [HandlerAction(PacketType.KILLS_GET_REWARD)]
        public void HandleKillsReward(WorldClient client, KillRewardPacket packet)
        {
            var result = _killsManager.TryGetKillsReward();
            _packetFactory.SendKillsReward(client, result.Ok, result.Stats);
        }

        [HandlerAction(PacketType.DEATHS_GET_REWARD)]
        public void HandleDeathsReward(WorldClient client, KillRewardPacket packet)
        {
            var result = _killsManager.TryGetDeathsReward();
            _packetFactory.SendDeathsReward(client, result.Ok, result.Money);
        }
    }
}
