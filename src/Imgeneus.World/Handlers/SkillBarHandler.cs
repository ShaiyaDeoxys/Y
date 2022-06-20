using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class SkillBarHandler : BaseHandler
    {
        private readonly IDatabase _database;

        public SkillBarHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDatabase database) : base(packetFactory, gameSession)
        {
            _database = database;
        }

        [HandlerAction(PacketType.CHARACTER_SKILL_BAR)]
        public async Task Handle(WorldClient client, SkillBarPacket packet)
        {
            if (_gameSession.Character is null)
                return;

            var characterId = _gameSession.Character.Id;

            // Remove old items.
            var items = _database.QuickItems.Where(item => item.Character.Id == characterId);
            _database.QuickItems.RemoveRange(items);

            var newItems = new DbQuickSkillBarItem[packet.QuickItems.Length];

            // Add new items.
            for (var i = 0; i < packet.QuickItems.Length; i++)
            {
                var quickItem = packet.QuickItems[i];
                newItems[i] = new DbQuickSkillBarItem()
                {
                    Bar = quickItem.Bar,
                    Slot = quickItem.Slot,
                    Bag = quickItem.Bag,
                    Number = quickItem.Number
                };
                newItems[i].CharacterId = characterId;
            }

            await _database.QuickItems.AddRangeAsync(newItems);
            await _database.SaveChangesAsync();
        }
    }
}
