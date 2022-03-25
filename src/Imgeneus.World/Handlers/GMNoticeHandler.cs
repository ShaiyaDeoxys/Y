using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Notice;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMNoticeHandler : BaseHandler
    {
        private readonly INoticeManager _noticeManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IMapProvider _mapProvider;

        public GMNoticeHandler(IGamePacketFactory packetFactory, IGameSession gameSession, INoticeManager noticeManager, ICountryProvider countryProvider, IMapProvider mapProvider) : base(packetFactory, gameSession)
        {
            _noticeManager = noticeManager;
            _countryProvider = countryProvider;
            _mapProvider = mapProvider;
        }

        [HandlerAction(PacketType.NOTICE_WORLD)]
        public void HandleNoticeWorld(WorldClient client, GMNoticeWorldPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _noticeManager.SendWorldNotice(packet.Message, packet.TimeInterval);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.NOTICE_PLAYER)]
        public void HandleNoticePlayer(WorldClient client, GMNoticePlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (_noticeManager.TrySendPlayerNotice(packet.Message, packet.TargetName, packet.TimeInterval))
                _packetFactory.SendGmCommandSuccess(client);
            else
                _packetFactory.SendGmCommandError(client, PacketType.NOTICE_PLAYER);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_NOTICE_WORLD)]
        public void HandleUsNoticePlayer(WorldClient client, GMNoticeWorldPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _noticeManager.SendWorldNotice(packet.Message, packet.TimeInterval);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.NOTICE_FACTION)]
        public void HandleNoticeFaction(WorldClient client, GMNoticeFactionPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _noticeManager.SendFactionNotice(packet.Message, _countryProvider.Country, packet.TimeInterval);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.NOTICE_ADMINS)]
        public void HandleNoticeAdmins(WorldClient client, GMNoticeAdminsPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _noticeManager.SendAdminNotice(packet.Message);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.NOTICE_MAP)]
        public void HandleNoticeMap(WorldClient client, GMNoticeMapPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            _noticeManager.SendMapNotice(packet.Message, _mapProvider.Map.Id, packet.TimeInterval);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
