using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class CreateCharacterHandler : BaseHandler
    {
        private readonly ISelectionScreenManager _selectionScreenManager;

        public CreateCharacterHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ISelectionScreenManager selectionScreenManager) : base(packetFactory, gameSession)
        {
            _selectionScreenManager = selectionScreenManager;
        }

        [HandlerAction(PacketType.CREATE_CHARACTER)]
        public async Task Handle(WorldClient client, CreateCharacterPacket packet)
        {
            var ok = await _selectionScreenManager.TryCreateCharacter(client.UserId, packet);
            _packetFactory.SendCreatedCharacter(client, ok);

            if (ok)
                _packetFactory.SendCharacterList(client, await _selectionScreenManager.GetCharacters(client.UserId));
        }
    }
}
