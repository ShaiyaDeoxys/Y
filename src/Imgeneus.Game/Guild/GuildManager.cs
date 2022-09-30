using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Etin;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Guild
{
    public class GuildManager : IGuildManager
    {
        private readonly ILogger<IGuildManager> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly ITimeService _timeService;
        private readonly IInventoryManager _inventoryManager;
        private readonly IPartyManager _partyManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IEtinManager _etinManager;
        private readonly IServiceProvider _serviceProvider;
        private uint _ownerId;

        private readonly IGuildConfiguration _config;
        private readonly IGuildHouseConfiguration _houseConfig;

        public GuildManager(ILogger<IGuildManager> logger, IGuildConfiguration config, IGuildHouseConfiguration houseConfig, IGameWorld gameWorld, ITimeService timeService, IInventoryManager inventoryManager, IPartyManager partyManager, ICountryProvider countryProvider, IEtinManager etinManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _timeService = timeService;
            _inventoryManager = inventoryManager;
            _partyManager = partyManager;
            _countryProvider = countryProvider;
            _etinManager = etinManager;
            _serviceProvider = serviceProvider;
            _config = config;
            _houseConfig = houseConfig;
#if DEBUG
            _logger.LogDebug("GuildManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~GuildManager()
        {
            _logger.LogDebug("GuildManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, uint guildId = 0, string name = "", byte rank = 0, byte guildRank = 0, IEnumerable<DbCharacter> members = null)
        {
            _ownerId = ownerId;
            GuildId = guildId;

            if (GuildId != 0)
            {
                GuildName = name;
                GuildMemberRank = rank;
                GuildRank = guildRank;
                GuildMembers.AddRange(members);
                NotifyGuildMembersOnline();
            }
        }

        public Task Clear()
        {
            NotifyGuildMembersOffline();
            SetGuildInfo(0, string.Empty, 0);
            GuildMembers.Clear();
            return Task.CompletedTask;
        }

        public uint GuildId { get; private set; }

        public bool HasGuild { get => GuildId != 0; }

        public string GuildName { get; private set; } = string.Empty;

        public byte GuildMemberRank { get; set; }

        public bool IsGuildMaster { get => GuildMemberRank == 1; }

        public byte GuildRank { get; private set; }

        public bool HasTopRank
        {
            get
            {
                if (!HasGuild)
                    return false;

                return GuildRank <= 30;
            }
        }

        public void SetGuildInfo(uint guildId, string name, byte memberRank)
        {
            GuildId = guildId;
            GuildName = name;
            GuildMemberRank = memberRank;
            OnGuildInfoChanged?.Invoke(_ownerId);
        }

        public event Action<uint> OnGuildInfoChanged;

        public List<DbCharacter> GuildMembers { get; init; } = new List<DbCharacter>();

        /// <summary>
        /// Notifies guild members, that player is online.
        /// </summary>
        private void NotifyGuildMembersOnline()
        {
            if (!HasGuild)
                return;

            foreach (var m in GuildMembers)
            {
                var id = m.Id;
                if (id == _ownerId)
                    continue;

                if (!_gameWorld.Players.ContainsKey(id))
                    continue;

                _gameWorld.Players.TryGetValue(id, out var player);

                if (player is null)
                    continue;

                player.SendGuildMemberIsOnline(_ownerId);
            }
        }

        /// <summary>
        /// Notifies guild members, that player is offline.
        /// </summary>
        private void NotifyGuildMembersOffline()
        {
            if (!HasGuild)
                return;

            foreach (var m in GuildMembers)
            {
                var id = m.Id;
                if (id == _ownerId)
                    continue;

                if (!_gameWorld.Players.ContainsKey(id))
                    continue;

                _gameWorld.Players.TryGetValue(id, out var player);

                if (player is null)
                    continue;

                player.SendGuildMemberIsOffline(_ownerId);
            }
        }

        #endregion

        #region Guild creation

        public async Task<GuildCreateFailedReason> CanCreateGuild(string guildName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guildName))
                    return GuildCreateFailedReason.WrongName;

                if (_inventoryManager.Gold < _config.MinGold)
                    return GuildCreateFailedReason.NotEnoughGold;

                if (!_partyManager.HasParty || !(_partyManager.Party is Party) || _partyManager.Party.Members.Count != _config.MinMembers)
                    return GuildCreateFailedReason.NotEnoughMembers;

                if (!_partyManager.Party.Members.All(x => x.LevelProvider.Level >= _config.MinLevel))
                    return GuildCreateFailedReason.LevelLimit;

                // TODO: banned words?
                // if(guildName.Contains(bannedWords))
                // return GuildCreateFailedReason.WrongName;

                if (_partyManager.Party.Members.Any(x => x.GuildManager.HasGuild))
                    return GuildCreateFailedReason.PartyMemberInAnotherGuild;

                var penalty = false;
                foreach (var m in _partyManager.Party.Members)
                {
                    if (await CheckPenalty(m.Id))
                    {
                        penalty = true;
                        break;
                    }
                }
                if (penalty)
                    return GuildCreateFailedReason.PartyMemberGuildPenalty;

                return GuildCreateFailedReason.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return GuildCreateFailedReason.Unknown;
            }
        }

        /// <summary>
        /// Ensures, that character doesn't have a penalty.
        /// </summary>
        /// <returns>true is penalty</returns>
        private async Task<bool> CheckPenalty(uint characterId)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            var character = await database.Characters.FindAsync(characterId);
            if (character is null)
                return true;

            if (character.GuildLeaveTime is null)
                return false;

            var leaveTime = (DateTime)character.GuildLeaveTime;
            return _timeService.UtcNow.Subtract(leaveTime).TotalHours < _config.MinPenalty;
        }

        public GuildCreateRequest CreationRequest { get; set; }

        public void InitCreateRequest(string guildName, string guildMessage)
        {
            var request = new GuildCreateRequest(_ownerId, _partyManager.Party.Members, guildName, guildMessage);
            foreach (var m in request.Members)
                m.GuildManager.CreationRequest = request;

            _partyManager.Party.OnMemberEnter += Party_OnMemberChange;
            _partyManager.Party.OnMemberLeft += Party_OnMemberChange;
        }

        private void Party_OnMemberChange(IParty party)
        {
            CreationRequest?.Dispose();
            CreationRequest = null;

            party.OnMemberEnter -= Party_OnMemberChange;
            party.OnMemberLeft -= Party_OnMemberChange;
        }

        public async Task<DbGuild> TryCreateGuild(string name, string message, uint masterId)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            var guild = new DbGuild(name, message, masterId, _countryProvider.Country == CountryType.Light ? Fraction.Light : Fraction.Dark);
            database.Guilds.Add(guild);

            var result = await database.SaveChangesAsync();

            if (result > 0)
            {
                var guildCreator = _gameWorld.Players[masterId];
                guildCreator.InventoryManager.Gold -= _config.MinGold;
                guildCreator.SendGoldUpdate();

                guild.Master = await database.Characters.FindAsync(masterId);

                return guild;
            }
            else
                return null;
        }

        #endregion

        #region Guild remove

        public async Task<bool> TryDeleteGuild()
        {
            if (GuildId == 0)
                throw new Exception("Guild can not be deleted, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.AsNoTracking().Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == GuildId);
            if (guild is null)
                return false;

            foreach (var m in guild.Members)
            {
                m.GuildId = null;
                m.GuildRank = 0;
            }

            database.Guilds.Remove(guild);

            var result = await database.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region Add/remove members

        public async Task<DbCharacter> TryAddMember(uint characterId, byte rank = 9)
        {
            if (GuildId == 0)
                throw new Exception("Member can not be added to guild, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.FindAsync(GuildId);
            if (guild is null)
                return null;

            var character = await database.Characters.FindAsync(characterId);
            if (character is null)
                return null;

            guild.Members.Add(character);
            character.GuildRank = rank;
            character.GuildJoinTime = _timeService.UtcNow;

            var result = await database.SaveChangesAsync();
            if (result > 0)
                return character;
            else
                return null;
        }

        public async Task<bool> TryRemoveMember(uint characterId)
        {
            if (GuildId == 0)
                throw new Exception("Member can not be removed from guild, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.FindAsync(GuildId);
            if (guild is null)
                return false;

            var character = await database.Characters.FindAsync(characterId);
            if (character is null)
                return false;

            character.GuildId = null;
            character.GuildRank = 0;
            character.GuildLeaveTime = _timeService.UtcNow;

            var result = await database.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region List guilds

        public Task<DbGuild[]> GetAllGuilds(Fraction country = Fraction.NotSelected)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            if (country == Fraction.NotSelected)
                return database.Guilds.Include(g => g.Master).ToArrayAsync();

            return database.Guilds.Include(g => g.Master).Where(g => g.Country == country).ToArrayAsync();
        }

        /// <inheritdoc/>
        public async Task<ICollection<DbCharacter>> GetMemebers(uint guildId)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == guildId);
            if (guild is null)
                return new List<DbCharacter>();

            return guild.Members;
        }

        /// <inheritdoc/>
        public void ReloadGuildRanks(IEnumerable<(uint GuildId, int Points, byte Rank)> results)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            foreach (var res in results)
            {
                if (res.GuildId == GuildId)
                    GuildRank = res.Rank;

                var guild = database.Guilds.Find(res.GuildId);
                if (guild is null)
                    return;

                guild.Points = res.Points;
                guild.Rank = res.Rank;
            }

            // Likely no need to save to db since GuildRankingManager will enqueue save?
        }

        #endregion

        #region Request join

        public async Task<bool> RequestJoin(uint guildId, Character player)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == guildId);
            if (guild is null)
                return false;

            await RemoveRequestJoin(player.Id);

            await database.GuildJoinRequests.AddAsync(new DbGuildJoinRequest()
            {
                GuildId = guildId,
                CharacterId = player.Id,
                CreateTime = DateTime.UtcNow
            });
            var ok = await database.SaveChangesAsync() > 0;
            if (!ok)
                return false;

            foreach (var m in guild.Members.Where(x => x.GuildRank < 3))
            {
                if (!_gameWorld.Players.ContainsKey(m.Id))
                    continue;

                var guildMember = _gameWorld.Players[m.Id];
                guildMember.SendGuildJoinRequestAdd(player);
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task RemoveRequestJoin(uint playerId)
        {
            var database = _serviceProvider.GetService<IDatabase>();

            var requests = await database.GuildJoinRequests.Where(x => x.CharacterId == playerId).ToListAsync();
            if (requests.Count == 0)
                return;

            foreach (var request in requests)
                database.GuildJoinRequests.Remove(request);

            await database.SaveChangesAsync();

            var guildId = requests.Last().GuildId;
            var guild = await database.Guilds.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == guildId);
            if (guild is null)
                return;

            foreach (var m in guild.Members.Where(x => x.GuildRank < 3))
            {
                if (!_gameWorld.Players.ContainsKey(m.Id))
                    continue;

                var guildMember = _gameWorld.Players[m.Id];
                guildMember.SendGuildJoinRequestRemove(playerId);
            }
        }

        public async Task<IEnumerable<DbGuildJoinRequest>> GetJoinRequests()
        {
            if (GuildId == 0)
                throw new Exception("Can not get join requests, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var requests = await database.GuildJoinRequests.Include(x => x.Character).Where(x => x.GuildId == GuildId).ToListAsync();
            return requests;
        }

        #endregion

        #region Member rank change

        public async Task<byte> TryChangeRank(uint playerId, bool demote)
        {
            if (GuildId == 0)
                throw new Exception("Rank of member can not be changed, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var character = await database.Characters.FirstOrDefaultAsync(x => x.GuildId == GuildId && x.Id == playerId);
            if (character is null)
                return 0;

            if (demote && character.GuildRank == 9)
                return 0;

            if (!demote && character.GuildRank == 2)
                return 0;

            if (demote)
                character.GuildRank++;
            else
                character.GuildRank--;

            var result = await database.SaveChangesAsync();
            return result > 0 ? character.GuildRank : (byte)0;
        }

        #endregion

        #region Guild house

        public bool HasGuildHouse
        {
            get
            {
                if (!HasGuild)
                    return false;

                var database = _serviceProvider.GetService<IDatabase>();

                var guild = database.Guilds.FirstOrDefault(x => x.Id == GuildId);
                if (guild is null)
                    return false;

                return guild.HasHouse;
            }
        }

        public bool HasPaidGuildHouse
        {
            get
            {
                if (!HasGuild)
                    return false;

                var database = _serviceProvider.GetService<IDatabase>();

                var guild = database.Guilds.FirstOrDefault(x => x.Id == GuildId);
                if (guild is null)
                    return false;

                return guild.Etin >= guild.KeepEtin;
            }
        }

        /// <inheritdoc/>
        public async Task<GuildHouseBuyReason> TryBuyHouse()
        {
            if (GuildMemberRank != 1)
            {
                return GuildHouseBuyReason.NotAuthorized;
            }

            if (_inventoryManager.Gold < _houseConfig.HouseBuyMoney)
            {
                return GuildHouseBuyReason.NoGold;
            }

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.FindAsync(GuildId);
            if (guild is null || guild.Rank > 30)
            {
                return GuildHouseBuyReason.LowRank;
            }

            if (guild.HasHouse)
            {
                return GuildHouseBuyReason.AlreadyBought;
            }

            _inventoryManager.Gold = (uint)(_inventoryManager.Gold - _houseConfig.HouseBuyMoney);

            guild.HasHouse = true;
            var count = await database.SaveChangesAsync();

            return count > 0 ? GuildHouseBuyReason.Ok : GuildHouseBuyReason.Unknown;
        }

        ///  <inheritdoc/>
        public byte GetRank(int guildId)
        {
            var database = _serviceProvider.GetService<IDatabase>();
            var guild = database.Guilds.Find(guildId);
            if (guild is null)
                return 0;

            return guild.Rank;
        }

        ///  <inheritdoc/>
        public bool CanUseNpc(NpcType type, short typeId, out byte requiredRank)
        {
            if (GuildId == 0)
                throw new Exception("NPC can not be checked, if guild manager is not initialized.");

            requiredRank = 30;

            if (GuildRank > 30)
                return false;

            var npcInfo = FindNpcInfo(_countryProvider.Country, type, typeId);

            if (npcInfo is null)
                return false;

            requiredRank = npcInfo.MinRank;
            return requiredRank >= GuildMemberRank;
        }

        public bool HasNpcLevel(NpcType type, short typeId)
        {
            if (GuildId == 0)
                throw new Exception("NPC level can not be checked, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = database.Guilds.Include(x => x.NpcLvls).FirstOrDefault(x => x.Id == GuildId);
            if (guild is null)
                return false;

            var npcInfo = FindNpcInfo(_countryProvider.Country, type, typeId);

            if (npcInfo is null)
                return false;

            if (npcInfo.NpcLvl == 0)
                return true;

            var currentLevel = guild.NpcLvls.FirstOrDefault(x => x.NpcType == npcInfo.NpcType && x.Group == npcInfo.Group);

            return currentLevel != null && currentLevel.NpcLevel >= npcInfo.NpcLvl;
        }

        public bool HasNpcLevel(NpcType type, byte level)
        {
            if (GuildId == 0)
                throw new Exception("NPC level can not be checked, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = database.Guilds.Include(x => x.NpcLvls).FirstOrDefault(x => x.Id == GuildId);
            if (guild is null)
                return false;

            var npcInfo = _houseConfig.NpcInfos.FirstOrDefault(x => x.NpcType == type);
            var currentLevel = guild.NpcLvls.FirstOrDefault(x => x.NpcType == npcInfo.NpcType && x.Group == npcInfo.Group);
            return (currentLevel != null && currentLevel.NpcLevel >= level) || (currentLevel is null && level == 0);
        }

        public float GetDiscount(NpcType type, short typeId)
        {
            if (GuildId == 0)
                throw new Exception("NPC level can not be checked, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = database.Guilds.Include(x => x.NpcLvls).FirstOrDefault(x => x.Id == GuildId);
            if (guild is null)
                return 0;

            var npcInfo = FindNpcInfo(_countryProvider.Country, type, typeId);
            if (npcInfo is null)
                return 0;

            var maxNpcLevel = guild.NpcLvls.FirstOrDefault(x => x.NpcType == npcInfo.NpcType && x.Group == npcInfo.Group);
            if (maxNpcLevel is null)
                return 0;

            var maxNpcInfo = _houseConfig.NpcInfos.FirstOrDefault(x => x.NpcType == type && x.Group == npcInfo.Group && x.NpcLvl == maxNpcLevel.NpcLevel);
            if (maxNpcInfo is null)
                return 0;

            return maxNpcInfo.PriceRate / 100f;
        }

        public async Task<IEnumerable<DbGuildNpcLvl>> GetGuildNpcs()
        {
            if (GuildId == 0)
                throw new Exception("NPC list can not be loaded, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            return await database.GuildNpcLvls.AsNoTracking().Where(x => x.GuildId == GuildId).ToListAsync();
        }

        ///  <inheritdoc/>
        public async Task<GuildNpcUpgradeReason> TryUpgradeNPC(NpcType npcType, byte npcGroup, byte nextLevel)
        {
            if (GuildId == 0)
                throw new Exception("NPC can not be upgraded, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.FindAsync(GuildId);
            if (guild is null || guild.Rank > 30)
                return GuildNpcUpgradeReason.LowRank;

            var currentLevel = database.GuildNpcLvls.FirstOrDefault(x => x.GuildId == GuildId && x.NpcType == npcType && x.Group == npcGroup);
            if (currentLevel is null && nextLevel != 1) // current npc level is 0
                return GuildNpcUpgradeReason.OneByOneLvl;

            if (currentLevel != null && currentLevel.NpcLevel + 1 != nextLevel)
                return GuildNpcUpgradeReason.OneByOneLvl;

            var npcInfo = FindNpcInfo(npcType, npcGroup, nextLevel);
            if (npcInfo is null)
                return GuildNpcUpgradeReason.Failed;

            if (guild.Rank > npcInfo.MinRank)
                return GuildNpcUpgradeReason.LowRank;

            if (npcInfo.UpPrice > guild.Etin)
                return GuildNpcUpgradeReason.NoEtin;

            if (currentLevel is null)
            {
                currentLevel = new DbGuildNpcLvl() { NpcType = npcType, Group = npcGroup, GuildId = GuildId, NpcLevel = 0 };
            }
            else // Remove prevous level.
            {
                database.GuildNpcLvls.Remove(currentLevel);
                await database.SaveChangesAsync();
            }
            currentLevel.NpcLevel++;

            guild.Etin -= npcInfo.UpPrice;
            database.GuildNpcLvls.Add(currentLevel);

            var count = await database.SaveChangesAsync();

            return count > 0 ? GuildNpcUpgradeReason.Ok : GuildNpcUpgradeReason.Failed;
        }

        private GuildHouseNpcInfo FindNpcInfo(CountryType country, NpcType npcType, short npcTypeId)
        {
            GuildHouseNpcInfo npcInfo;
            if (country == CountryType.Light)
            {
                npcInfo = _houseConfig.NpcInfos.FirstOrDefault(x => x.NpcType == npcType && x.LightNpcTypeId == npcTypeId);
            }
            else
            {
                npcInfo = _houseConfig.NpcInfos.FirstOrDefault(x => x.NpcType == npcType && x.DarkNpcTypeId == npcTypeId);
            }

            return npcInfo;
        }

        private GuildHouseNpcInfo FindNpcInfo(NpcType npcType, byte npcGroup, byte npcLevel)
        {
            return _houseConfig.NpcInfos.FirstOrDefault(x => x.NpcType == npcType && x.Group == npcGroup && x.NpcLvl == npcLevel);
        }

        ///  <inheritdoc/>
        public (byte LinkRate, byte RepaireRate) GetBlacksmithRates()
        {
            if (GuildId == 0)
                throw new Exception("Linking rate can not be calculated, if guild manager is not initialized.");

            var database = _serviceProvider.GetService<IDatabase>();

            var npc = database.GuildNpcLvls.AsNoTracking().FirstOrDefault(x => x.GuildId == GuildId && x.NpcType == NpcType.Blacksmith && x.Group == 0);
            if (npc is null)
                return (0, 0);

            var npcInfo = FindNpcInfo((NpcType)npc.NpcType, npc.Group, npc.NpcLevel);
            if (npcInfo is null)
                return (0, 0);

            return (npcInfo.RapiceMixPercentRate, npcInfo.RapiceMixDecreRate);
        }

        #endregion

        #region Etin

        public Task<int> GetEtin()
        {
            if (GuildId == 0)
                throw new Exception("Etin can not be checked, if guild manager is not initialized.");

            return _etinManager.GetEtin(GuildId);
        }

        /// <inheritdoc/>
        public async Task<IList<Item>> ReturnEtin()
        {
            var result = new List<Item>();

            var database = _serviceProvider.GetService<IDatabase>();

            var guild = await database.Guilds.FindAsync(GuildId);

            var totalEtin = 0;

            var etins = _inventoryManager.InventoryItems.Select(x => x.Value).Where(itm => itm.Special == SpecialEffect.Etin_1 || itm.Special == SpecialEffect.Etin_10 || itm.Special == SpecialEffect.Etin_100 || itm.Special == SpecialEffect.Etin_1000).ToList();
            foreach (var etin in etins)
            {
                _inventoryManager.RemoveItem(etin, "return_etin");

                var etinNumber = 0;
                switch (etin.Special)
                {
                    case SpecialEffect.Etin_1:
                        etinNumber = 1;
                        break;

                    case SpecialEffect.Etin_10:
                        etinNumber = 10;
                        break;

                    case SpecialEffect.Etin_100:
                        etinNumber = 100;
                        break;

                    case SpecialEffect.Etin_1000:
                        etinNumber = 1000;
                        break;
                }

                totalEtin += etinNumber * etin.Count;
                result.Add(etin);
            }

            guild.Etin = await GetEtin() + totalEtin;

            var count = await database.SaveChangesAsync();

            return count > 0 ? result : throw new Exception("Could not save etins to database");
        }

        #endregion
    }
}
