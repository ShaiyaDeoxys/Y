using Imgeneus.Database.Entities;
using Imgeneus.Game.Blessing;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Collections.Generic;
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
        private readonly IGuildManager _guildManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IBlessManager _blessManager;

        public SelectCharacterHandler(IGamePacketFactory packetFactory,
                                      IGameSession gameSession,
                                      IGameWorld gameWorld,
                                      ICharacterFactory characterFactory,
                                      IStatsManager statsManager,
                                      IGuildManager guildManager,
                                      ICountryProvider countryProvider,
                                      IBlessManager blessManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _characterFactory = characterFactory;
            _statsManager = statsManager;
            _guildManager = guildManager;
            _countryProvider = countryProvider;
            _blessManager = blessManager;
        }

        [HandlerAction(PacketType.SELECT_CHARACTER)]
        public async Task Handle(WorldClient client, SelectCharacterPacket packet)
        {
            if (_gameWorld.Players.TryGetValue(packet.CharacterId, out var existingPlayer))
            {
                _packetFactory.SendCharacterSelected(client, false, 0);
                return;
            }

            var character = await _characterFactory.CreateCharacter(client.UserId, packet.CharacterId);
            if (character is null)
            {
                _packetFactory.SendCharacterSelected(client, false, 0);
                return;
            }

            _gameSession.Character = character;

            _packetFactory.SendCharacterSelected(client, true, character.Id);

            _packetFactory.SendWorldDay(client); // TODO: why do we need it?
            _packetFactory.SendGuildList(client, await _guildManager.GetAllGuilds(_countryProvider.Country == CountryType.Light ? Fraction.Light : Fraction.Dark));

            var online = new List<DbCharacter>();
            var notOnline = new List<DbCharacter>();
            foreach (var m in _guildManager.GuildMembers)
            {
                if (_gameWorld.Players.ContainsKey(m.Id) || character.Id == m.Id)
                    online.Add(m);
                else
                    notOnline.Add(m);
            }
            _packetFactory.SendGuildMembersOnline(client, online, true);
            _packetFactory.SendGuildMembersOnline(client, notOnline, false);


            _packetFactory.SendDetails(client, character);

            _packetFactory.SendAdditionalStats(client, character);

            _packetFactory.SendCurrentHitpoints(client, character.HealthManager.CurrentHP, character.HealthManager.CurrentMP, character.HealthManager.CurrentSP);

            _packetFactory.SendInventoryItems(client, character.InventoryManager.InventoryItems.Values); // WARNING: some servers expanded invetory to 6 bags(os is 5 bags), if you send item in 6 bag, client will crash!
            foreach (var item in character.InventoryManager.InventoryItems.Values.Where(i => i.ExpirationTime != null))
                _packetFactory.SendItemExpiration(client, item);

            _packetFactory.SendLearnedSkills(client, character);

            _packetFactory.SendOpenQuests(client, character.QuestsManager.Quests.Where(q => !q.IsFinished));
            _packetFactory.SendFinishedQuests(client, character.QuestsManager.Quests.Where(q => q.IsFinished));

            _packetFactory.SendActiveBuffs(client, character.BuffsManager.ActiveBuffs);

            _packetFactory.SendFriends(client, character.FriendsManager.Friends.Values);

            _packetFactory.SendBlessAmount(client, character.CountryProvider.Country, character.CountryProvider.Country == CountryType.Light ? _blessManager.LightAmount : _blessManager.DarkAmount, _blessManager.RemainingTime);

            _packetFactory.SendBankItems(client, character.BankManager.BankItems.Values);

            _packetFactory.SendWarehouseItems(client, character.WarehouseManager.Items.Values);

            _packetFactory.SendTeleportSavedPositions(client, character.TeleportationManager.SavedPositions);

            if (character.GuildManager.HasGuild)
            {
                _packetFactory.SendGuildNpcs(client, await character.GuildManager.GetGuildNpcs());
                
                if (character.GuildManager.GuildMemberRank <= 3)
                {
                    var requests = await _guildManager.GetJoinRequests();
                    foreach (var request in requests)
                        _packetFactory.SendGuildJoinRequestAdd(client, request.CharacterId, request.Character.Level, request.Character.Class, request.Character.Name);
                }
            }

#if !EP8_V2
            _packetFactory.SendAccountPoints(client, character.AdditionalInfoManager.Points); // WARNING: This is necessary if you have an in-game item mall.
#endif

            _packetFactory.SendSkillBar(client, character.QuickItems); // Should be always the last! Changes packet encryption to xor!
            client.CryptoManager.UseExpandedKey = true;
        }
    }
}
