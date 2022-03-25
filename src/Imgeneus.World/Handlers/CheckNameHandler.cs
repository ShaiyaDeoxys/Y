using Imgeneus.Core.Extensions;
using Imgeneus.Database;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class CheckNameHandler : BaseHandler
    {
        private readonly IDatabase _database;

        public CheckNameHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDatabase database): base(packetFactory, gameSession)
        {
            _database = database;
        }

        [HandlerAction(PacketType.CHECK_CHARACTER_AVAILABLE_NAME)]
        public void Handle(WorldClient client, CheckCharacterAvailableNamePacket packet)
        {
            var character = _database.Characters.FirstOrDefault(c => c.Name == packet.CharacterName);

            var isAvailable = character is null && packet.CharacterName.IsValidCharacterName();

            _packetFactory.SendCheckName(client, isAvailable);
        }
    }
}
