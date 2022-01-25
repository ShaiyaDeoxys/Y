using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class SelectCharacterHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly ICharacterFactory _characterFactory;
        private readonly IStatsManager _statsManager;

        public SelectCharacterHandler(IGamePacketFactory packetFactory,
                                      IGameSession gameSession,
                                      IGameWorld gameWorld,
                                      ICharacterFactory characterFactory,
                                      IStatsManager statsManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _characterFactory = characterFactory;
            _statsManager = statsManager;
        }

        [HandlerAction(PacketType.SELECT_CHARACTER)]
        public async Task Handle(WorldClient client, SelectCharacterPacket packet)
        {
            var character = await _characterFactory.CreateCharacter(client.UserId, packet.CharacterId);
            if (character is null)
            {
                _packetFactory.SendCharacterSelected(client, false, 0);
                return;
            }

            var ok = _gameWorld.TryLoadPlayer(character);
            if (!ok)
            {
                _packetFactory.SendCharacterSelected(client, false, 0);
                return;
            }

            _packetFactory.SendCharacterSelected(client, true, character.Id);

            //SendWorldDay(); // TODO: why do we need it?
            //SendGuildList();
            //SendGuildMembersOnline();
            _packetFactory.SendDetails(client, character);
            _packetFactory.SendAdditionalStats(client, character);
            //SendCurrentHitpoints();
            _packetFactory.SendInventoryItems(client, character.InventoryManager.InventoryItems.Values); // WARNING: some servers expanded invetory to 6 bags(os is 5 bags), if you send item in 6 bag, client will crash!
            foreach (var item in character.InventoryManager.InventoryItems.Values.Where(i => i.ExpirationTime != null))
                _packetFactory.SendItemExpiration(client, item);
            _packetFactory.SendLearnedSkills(client, character);
            //SendOpenQuests();
            //SendFinishedQuests();
            _packetFactory.SendActiveBuffs(client, character.BuffsManager.ActiveBuffs);
            //SendMoveAndAttackSpeed();
            //SendFriends();
            //SendBlessAmount();
            //SendBankItems();
            //SendGuildNpcLvlList();
            _packetFactory.SendAutoStats(client, _statsManager.AutoStr, _statsManager.AutoDex, _statsManager.AutoRec, _statsManager.AutoInt, _statsManager.AutoWis, _statsManager.AutoLuc);

#if !EP8_V2
            //SendAccountPoints(); // WARNING: This is necessary if you have an in-game item mall.
#endif

            _packetFactory.SendSkillBar(client, character.QuickItems); // Should be always the last! Changes packet encryption to xor!
            client.CryptoManager.UseExpandedKey = true;
        }
    }
}
