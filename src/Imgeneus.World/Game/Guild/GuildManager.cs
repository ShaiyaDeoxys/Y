using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Guild
{
    public class GuildManager : IGuildManager
    {
        private readonly ILogger<IGuildManager> _logger;
        private readonly IDatabase _database;
        private readonly IGameWorld _gameWorld;
        private readonly ITimeService _timeService;
        private readonly IInventoryManager _inventoryManager;
        private readonly IPartyManager _partyManager;
        private readonly ICountryProvider _countryProvider;
        private SemaphoreSlim _sync = new SemaphoreSlim(1);

        private int _ownerId;

        private readonly IGuildConfiguration _config;
        private readonly IGuildHouseConfiguration _houseConfig;

        public GuildManager(ILogger<IGuildManager> logger, IGuildConfiguration config, IGuildHouseConfiguration houseConfig, IDatabase database, IGameWorld gameWorld, ITimeService timeService, IInventoryManager inventoryManager, IPartyManager partyManager, ICountryProvider countryProvider)
        {
            _logger = logger;
            _database = database;
            _gameWorld = gameWorld;
            _timeService = timeService;
            _inventoryManager = inventoryManager;
            _partyManager = partyManager;
            _countryProvider = countryProvider;
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

        public void Init(int ownerId, int guildId = 0, string name = "", byte rank = 0, IEnumerable<DbCharacter> members = null)
        {
            _ownerId = ownerId;
            GuildId = guildId;

            if (GuildId != 0)
            {
                GuildName = name;
                GuildRank = rank;
                GuildMembers.AddRange(members);
                NotifyGuildMembersOnline();
            }
        }

        public Task Clear()
        {
            NotifyGuildMembersOffline();
            GuildId = 0;
            GuildName = string.Empty;
            GuildRank = 0;
            GuildMembers.Clear();
            return Task.CompletedTask;
        }

        public int GuildId { get; set; }

        public bool HasGuild { get => GuildId != 0; }

        public string GuildName { get; set; } = string.Empty;

        public byte GuildRank { get; set; }

        public bool HasTopRank
        {
            get
            {
                if (!HasGuild)
                    return false;

                return GuildRank <= 30;
            }
        }

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
        private async Task<bool> CheckPenalty(int characterId)
        {
            var character = await _database.Characters.FindAsync(characterId);
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

            party.OnMemberEnter -= Party_OnMemberChange;
            party.OnMemberLeft -= Party_OnMemberChange;
        }

        public async Task<DbGuild> TryCreateGuild(string name, string message, int masterId)
        {
            var guild = new DbGuild(name, message, masterId, _countryProvider.Country == Country.CountryType.Light ? Fraction.Light : Fraction.Dark);

            _database.Guilds.Add(guild);

            var result = await _database.SaveChangesAsync();

            if (result > 0)
            {
                var guildCreator = _gameWorld.Players[masterId];
                guildCreator.InventoryManager.Gold -= _config.MinGold;
                guildCreator.SendGoldUpdate();

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

            var guild = await _database.Guilds.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == GuildId);
            if (guild is null)
                return false;

            foreach (var m in guild.Members)
            {
                m.GuildId = null;
                m.GuildRank = 0;
            }

            _database.Guilds.Remove(guild);

            var result = await _database.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region Add/remove members

        public async Task<DbCharacter> TryAddMember(int characterId, byte rank = 9)
        {
            if (GuildId == 0)
                throw new Exception("Member can not be added to guild, if guild manager is not initialized.");

            var guild = await _database.Guilds.FindAsync(GuildId);
            if (guild is null)
                return null;

            var character = await _database.Characters.FindAsync(characterId);
            if (character is null)
                return null;

            guild.Members.Add(character);
            character.GuildRank = rank;
            character.GuildJoinTime = _timeService.UtcNow;

            var result = await _database.SaveChangesAsync();
            if (result > 0)
                return character;
            else
                return null;
        }

        public async Task<bool> TryRemoveMember(int characterId)
        {
            if (GuildId == 0)
                throw new Exception("Member can not be removed from guild, if guild manager is not initialized.");

            var guild = await _database.Guilds.FindAsync(GuildId);
            if (guild is null)
                return false;

            var character = await _database.Characters.FindAsync(characterId);
            if (character is null)
                return false;

            guild.Members.Remove(character);
            character.GuildId = null;
            character.GuildRank = 0;
            character.GuildLeaveTime = _timeService.UtcNow;

            var result = await _database.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region List guilds

        public Task<DbGuild[]> GetAllGuilds(Fraction country = Fraction.NotSelected)
        {
            if (country == Fraction.NotSelected)
                return _database.Guilds.Include(g => g.Master).ToArrayAsync();

            return _database.Guilds.Include(g => g.Master).Where(g => g.Country == country).ToArrayAsync();
        }

        /// <inheritdoc/>
        public async Task<DbGuild> GetGuild(int guildId)
        {
            return await _database.Guilds.AsNoTracking().FirstAsync(x => x.Id == guildId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DbCharacter>> GetMemebers(int guildId)
        {
            var guild = await _database.Guilds.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == guildId);
            if (guild is null)
                return new List<DbCharacter>();

            return guild.Members;
        }

        /// <inheritdoc/>
        public void ReloadGuildRanks(IEnumerable<(int GuildId, int Points, byte Rank)> results)
        {
            foreach (var res in results)
            {
                var guild = _database.Guilds.Find(res.GuildId);
                if (guild is null)
                    return;

                guild.Points = res.Points;
                guild.Rank = res.Rank;
            }

            // Likely no need to save to db since GuildRankingManager will enqueue save?
        }

        #endregion

        #region Request join

        /// <summary>
        /// Dictionary of join requests.
        /// Key is player id.
        /// Value is guild id.
        /// </summary>
        public static readonly ConcurrentDictionary<int, int> JoinRequests = new ConcurrentDictionary<int, int>();

        public async Task<bool> RequestJoin(int guildId, Character player)
        {
            var guild = await _database.Guilds.FindAsync(guildId);
            if (guild is null)
                return false;

            await RemoveRequestJoin(player.Id);

            JoinRequests.TryAdd(player.Id, guildId);

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
        public async Task RemoveRequestJoin(int playerId)
        {
            if (JoinRequests.TryRemove(playerId, out var removed))
            {
                var guild = await _database.Guilds.FindAsync(removed);
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
        }

        #endregion

        #region Member rank change

        public async Task<byte> TryChangeRank(int playerId, bool demote)
        {
            if (GuildId == 0)
                throw new Exception("Rank of member can not be changed, if guild manager is not initialized.");

            var character = await _database.Characters.FirstOrDefaultAsync(x => x.GuildId == GuildId && x.Id == playerId);
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

            var result = await _database.SaveChangesAsync();
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

                var guild = _database.Guilds.AsNoTracking().FirstOrDefault(x => x.Id == GuildId);
                if (guild is null)
                    return false;

                return guild.HasHouse;
            }
        }

        /// <inheritdoc/>
        public async Task<GuildHouseBuyReason> TryBuyHouse()
        {
            if (GuildRank != 1)
            {
                return GuildHouseBuyReason.NotAuthorized;
            }

            if (_inventoryManager.Gold < _houseConfig.HouseBuyMoney)
            {
                return GuildHouseBuyReason.NoGold;
            }

            var guild = await _database.Guilds.FindAsync(GuildId);
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
            var count = await _database.SaveChangesAsync();

            return count > 0 ? GuildHouseBuyReason.Ok : GuildHouseBuyReason.Unknown;
        }

        ///  <inheritdoc/>
        public byte GetRank(int guildId)
        {
            var guild = _database.Guilds.Find(guildId);
            if (guild is null)
                return 0;

            return guild.Rank;
        }

        ///  <inheritdoc/>
        public bool CanUseNpc(byte type, ushort typeId, out byte requiredRank)
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
            return requiredRank >= GuildRank;
        }

        ///  <inheritdoc/>
        public bool HasNpcLevel(byte type, ushort typeId)
        {
            if (GuildId == 0)
                throw new Exception("NPC level can not be checked, if guild manager is not initialized.");

            var guild = _database.Guilds.Include(x => x.NpcLvls).FirstOrDefault(x => x.Id == GuildId);
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

        ///  <inheritdoc/>
        public IEnumerable<DbGuildNpcLvl> GetGuildNpcs(int guildId)
        {
            return _database.GuildNpcLvls.Where(x => x.GuildId == guildId).ToList();
        }

        ///  <inheritdoc/>
        public async Task<GuildNpcUpgradeReason> TryUpgradeNPC(int guildId, byte npcType, byte npcGroup, byte nextLevel)
        {
            await _sync.WaitAsync();

            var result = await UpgradeNPC(guildId, npcType, npcGroup, nextLevel);

            _sync.Release();

            return result;
        }

        private async Task<GuildNpcUpgradeReason> UpgradeNPC(int guildId, byte npcType, byte npcGroup, byte nextLevel)
        {
            var guild = _database.Guilds.Find(guildId);
            if (guild is null || guild.Rank > 30)
                return GuildNpcUpgradeReason.LowRank;

            var currentLevel = _database.GuildNpcLvls.FirstOrDefault(x => x.GuildId == guildId && x.NpcType == npcType && x.Group == npcGroup);
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
                currentLevel = new DbGuildNpcLvl() { NpcType = npcType, Group = npcGroup, GuildId = guildId, NpcLevel = 0 };
            }
            else // Remove prevous level.
            {
                _database.GuildNpcLvls.Remove(currentLevel);
                await _database.SaveChangesAsync();
            }
            currentLevel.NpcLevel++;

            guild.Etin -= npcInfo.UpPrice;
            _database.GuildNpcLvls.Add(currentLevel);

            await _database.SaveChangesAsync();

            return GuildNpcUpgradeReason.Ok;
        }

        private GuildHouseNpcInfo FindNpcInfo(CountryType country, byte npcType, ushort npcTypeId)
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

        private GuildHouseNpcInfo FindNpcInfo(byte npcType, byte npcGroup, byte npcLevel)
        {
            return _houseConfig.NpcInfos.FirstOrDefault(x => x.NpcType == npcType && x.Group == npcGroup && x.NpcLvl == npcLevel);
        }

        ///  <inheritdoc/>
        public (byte LinkRate, byte RepaireRate) GetBlacksmithRates(int guildId)
        {
            var npc = _database.GuildNpcLvls.FirstOrDefault(x => x.GuildId == guildId && x.NpcType == 3 && x.Group == 0);
            if (npc is null)
                return (0, 0);

            var npcInfo = FindNpcInfo((byte)npc.NpcType, npc.Group, npc.NpcLevel);
            if (npcInfo is null)
                return (0, 0);

            return (npcInfo.RapiceMixPercentRate, npcInfo.RapiceMixDecreRate);
        }

        #endregion

        #region Etin

        public async Task<int> GetEtin()
        {
            if (GuildId == 0)
                throw new Exception("Etin can not be checked, if guild manager is not initialized.");

            var guild = await GetGuild(GuildId);
            return guild.Etin;
        }

        /// <inheritdoc/>
        public async Task<IList<Item>> ReturnEtin(Character character)
        {
            await _sync.WaitAsync();

            var result = new List<Item>();
            var guild = await GetGuild(GuildId);

            var totalEtin = 0;

            var etins = character.InventoryManager.InventoryItems.Select(x => x.Value).Where(itm => itm.Special == SpecialEffect.Etin_1 || itm.Special == SpecialEffect.Etin_10 || itm.Special == SpecialEffect.Etin_100 || itm.Special == SpecialEffect.Etin_1000).ToList();
            foreach (var etin in etins)
            {
                character.InventoryManager.RemoveItem(etin);

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

            guild.Etin += totalEtin;

            await _database.SaveChangesAsync();

            _sync.Release();

            return result;
        }

        #endregion
    }
}
