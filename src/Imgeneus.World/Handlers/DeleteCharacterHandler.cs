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
    public class DeleteCharacterHandler : BaseHandler
    {
        private readonly ISelectionScreenManager _selectionScreenManager;

        public DeleteCharacterHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ISelectionScreenManager selectionScreenManager) : base(packetFactory, gameSession)
        {
            _selectionScreenManager = selectionScreenManager;
        }

        [HandlerAction(PacketType.DELETE_CHARACTER)]
        public async Task Handle(WorldClient client, DeleteCharacterPacket packet)
        {
            var ok = await _selectionScreenManager.TryDeleteCharacter(client.UserId, packet.CharacterId);
            _packetFactory.SendDeletedCharacter(client, ok, packet.CharacterId);
        }
    }
}
