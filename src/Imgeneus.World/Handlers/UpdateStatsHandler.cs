using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class UpdateStatsHandler : BaseHandler
    {
        private readonly IStatsManager _statsManager;

        public UpdateStatsHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IStatsManager statsManager) : base(packetFactory, gameSession)
        {
            _statsManager = statsManager;
        }

        [HandlerAction(PacketType.UPDATE_STATS)]
        public async Task Handle(WorldClient client, UpdateStatsPacket packet)
        {
            var (str, dex, rec, intl, wis, luc) = packet;

            var fullStat = str + dex + rec + intl + wis + luc;
            if (fullStat > _statsManager.StatPoint || fullStat > ushort.MaxValue)
                return;

            var ok = await _statsManager.TrySetStats((ushort)(_statsManager.Strength + str),
                                                     (ushort)(_statsManager.Dexterity + dex),
                                                     (ushort)(_statsManager.Reaction + rec),
                                                     (ushort)(_statsManager.Intelligence + intl),
                                                     (ushort)(_statsManager.Wisdom + wis),
                                                     (ushort)(_statsManager.Luck + luc),
                                                     (ushort)(_statsManager.StatPoint - fullStat));

            if (ok)
                _packetFactory.SendStatsUpdate(client, str, dex, rec, intl, wis, luc);
        }
    }
}
