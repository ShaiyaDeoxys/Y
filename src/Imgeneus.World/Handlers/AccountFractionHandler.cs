using Imgeneus.Database;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AccountFractionHandler : BaseHandler
    {
        private readonly IDatabase _database;
        public AccountFractionHandler(IGamePacketFactory packetFactory, IDatabase database) : base(packetFactory)
        {
            _database = database;
        }

        [HandlerAction(PacketType.ACCOUNT_FACTION)]
        public async Task Handle(WorldClient client, AccountFractionPacket packet)
        {
            var user = await _database.Users.FindAsync(client.UserId);
            user.Faction = packet.Fraction;

            await _database.SaveChangesAsync();
        }
    }
}
