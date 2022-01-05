using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RenameCharacterHandler : BaseHandler
    {
        private readonly ISelectionScreenManager _selectionScreenManager;
        public RenameCharacterHandler(IGamePacketFactory packetFactory, ISelectionScreenManager selectionScreenManager) : base(packetFactory)
        {
            _selectionScreenManager = selectionScreenManager;
        }

        [HandlerAction(PacketType.RENAME_CHARACTER)]
        public async Task Handle(WorldClient client, RenameCharacterPacket packet)
        {
            var ok = await _selectionScreenManager.TryRenameCharacter(client.UserId, packet.CharacterId, packet.NewName);
            _packetFactory.SendRenamedCharacter(client, ok, packet.CharacterId);
        }
    }
}
