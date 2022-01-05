using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class SelectCharacterHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public SelectCharacterHandler(IGamePacketFactory packetFactory, IGameWorld gameWorld) : base(packetFactory)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.SELECT_CHARACTER)]
        public async Task Handle(WorldClient client, SelectCharacterPacket packet)
        {
            var character = await _gameWorld.LoadPlayer(packet.CharacterId, client);

            if (character is null)
            {
                _packetFactory.SendCharacterSelected(client, false, 0);
                return;
            }


            client.CharID = character.Id;

            _packetFactory.SendCharacterSelected(client, true, character.Id);

            //SendWorldDay(); // TODO: why do we need it?
            //SendGuildList();
            //SendGuildMembersOnline();
            _packetFactory.SendDetails(client, character);
            //SendAdditionalStats();
            //SendCurrentHitpoints();
            //SendInventoryItems();
            //SendLearnedSkills();
            //SendOpenQuests();
            //SendFinishedQuests();
            //SendActiveBuffs();
            //SendMoveAndAttackSpeed();
            //SendFriends();
            //SendBlessAmount();
            //SendBankItems();
            //SendGuildNpcLvlList();
            //SendAutoStats();

#if EP8_V1 || SHAIYA_US
            //SendAccountPoints(); // WARNING: This is necessary if you have an in-game item mall.
#endif

            _packetFactory.SendSkillBar(client, character.QuickItems); // Should be always the last! Changes packet encryption to xor!
            client.CryptoManager.UseExpandedKey = true; // Fix for encryption after selection screen.
        }
    }
}
