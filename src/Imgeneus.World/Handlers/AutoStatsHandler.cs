using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AutoStatsHandler : BaseHandler
    {
        private readonly IStatsManager _statsManager;
        private readonly ICharacterConfiguration _characterConfig;
        private readonly IAdditionalInfoManager _additionalInfoManager;

        public AutoStatsHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IStatsManager statsManager, ICharacterConfiguration characterConfig, IAdditionalInfoManager additionalInfoManager) : base(packetFactory, gameSession)
        {
            _statsManager = statsManager;
            _characterConfig = characterConfig;
            _additionalInfoManager = additionalInfoManager;
        }

        [HandlerAction(PacketType.AUTO_STATS_SET)]
        public async Task HandleSet(WorldClient client, AutoStatsSettingsPacket packet)
        {
            var (str, dex, rec, intl, wis, luc) = packet;

            if (str + dex + rec + intl + wis + luc > _characterConfig.GetLevelStatSkillPoints(_additionalInfoManager.Grow).StatPoint)
                return;

            var ok = await _statsManager.TrySetAutoStats(str, dex, rec, intl, wis, luc);
            _packetFactory.SendAutoStats(client, str, dex, rec, intl, wis, luc);
        }

        [HandlerAction(PacketType.AUTO_STATS_LIST)]
        public void HandleList(WorldClient client, EmptyPacket packet)
        {
            _packetFactory.SendAutoStats(client, _statsManager.AutoStr, _statsManager.AutoDex, _statsManager.AutoRec, _statsManager.AutoInt, _statsManager.AutoWis, _statsManager.AutoLuc);
        }
    }
}
