using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMStealthHandler : BaseHandler
    {
        private readonly IStealthManager _stealthManager;

        public GMStealthHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IStealthManager stealthManager) : base(packetFactory, gameSession)
        {
            _stealthManager = stealthManager;
        }

        [HandlerAction(PacketType.GM_CHAR_ON)]
        public void HandleOn(WorldClient client, GMCharacterOnPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (_stealthManager.IsAdminStealth) // Already in admin stealth.
                _packetFactory.SendGmCommandError(client, PacketType.GM_CHAR_ON);

            _stealthManager.IsAdminStealth = true;
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_CHAR_OFF)]
        public void HandleOff(WorldClient client, GMCharacterOffPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (!_stealthManager.IsAdminStealth) // Alredy not in stealth.
                _packetFactory.SendGmCommandError(client, PacketType.GM_CHAR_OFF);

            _stealthManager.IsAdminStealth = false;
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
