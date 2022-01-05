using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RestoreCharacterHandler : BaseHandler
    {
        private readonly ISelectionScreenManager _selectionScreenManager;

        public RestoreCharacterHandler(IGamePacketFactory packetFactory, ISelectionScreenManager selectionScreenManager) : base(packetFactory)
        {
            _selectionScreenManager = selectionScreenManager;
        }

        [HandlerAction(PacketType.RESTORE_CHARACTER)]
        public async Task Handle(WorldClient client, RestoreCharacterPacket packet)
        {
            var ok = await _selectionScreenManager.TryRestoreCharacter(client.UserId, packet.CharacterId);
            _packetFactory.SendRestoredCharacter(client, ok, packet.CharacterId);
        }
    }
}
