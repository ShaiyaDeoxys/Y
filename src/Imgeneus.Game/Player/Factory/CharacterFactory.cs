using Imgeneus.Authentication.Context;
using Imgeneus.Authentication.Entities;
using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Blessing;
using Imgeneus.Game.Market;
using Imgeneus.Game.Recover;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Shop;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Player
{
    public class CharacterFactory : ICharacterFactory
    {
        private readonly ILogger<ICharacterFactory> _logger;
        private readonly IUsersDatabase _usersDatabase;
        private readonly IDatabase _database;
        private readonly ILogger<Character> _characterLogger;
        private readonly IGameWorld _gameWorld;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IGameDefinitionsPreloder _definitionsPreloder;
        private readonly ICountryProvider _countryProvider;
        private readonly ISpeedManager _speedManager;
        private readonly IStatsManager _statsManager;
        private readonly IHealthManager _healthManager;
        private readonly ILevelProvider _levelProvider;
        private readonly ILevelingManager _levelingManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly ILinkingManager _linkingManager;
        private readonly IGuildManager _guildManager;
        private readonly IGameSession _gameSession;
        private readonly IStealthManager _stealthManager;
        private readonly IAttackManager _attackManager;
        private readonly ISkillsManager _skillsManager;
        private readonly IBuffsManager _buffsManager;
        private readonly IElementProvider _elementProvider;
        private readonly IKillsManager _killsManager;
        private readonly IVehicleManager _vehicleManager;
        private readonly IShapeManager _shapeManager;
        private readonly IMovementManager _movementManager;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly IMapProvider _mapProvider;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IPartyManager _partyManager;
        private readonly ITradeManager _tradeManager;
        private readonly IFriendsManager _friendsManager;
        private readonly IDuelManager _duelManager;
        private readonly IBankManager _bankManager;
        private readonly IQuestsManager _questsManager;
        private readonly IUntouchableManager _untouchableManager;
        private readonly IWarehouseManager _warehouseManager;
        private readonly IShopManager _shopManager;
        private readonly ISkillCastingManager _skillCastingManager;
        private readonly ICastProtectionManager _castProtectionManager;
        private readonly IBlessManager _blessManager;
        private readonly IRecoverManager _recoverManager;
        private readonly IMarketManager _marketManager;
        private readonly IChatManager _chatManager;
        private readonly IGamePacketFactory _packetFactory;
        private readonly UserManager<DbUser> _userManager;

        public CharacterFactory(ILogger<ICharacterFactory> logger,
                                IUsersDatabase usersDatabase,
                                IDatabase database,
                                ILogger<Character> characterLogger,
                                IGameWorld gameWorld,
                                IDatabasePreloader databasePreloader,
                                IGameDefinitionsPreloder definitionsPreloder,
                                ICountryProvider countryProvider,
                                ISpeedManager speedManager,
                                IStatsManager statsManager,
                                IHealthManager healthManager,
                                ILevelProvider levelProvider,
                                ILevelingManager levelingManager,
                                IInventoryManager inventoryManager,
                                ILinkingManager linkingManager,
                                IGuildManager guildManager,
                                IGameSession gameSession,
                                IStealthManager stealthManager,
                                IAttackManager attackManager,
                                ISkillsManager skillsManager,
                                IBuffsManager buffsManager,
                                IElementProvider elementProvider,
                                IKillsManager killsManager,
                                IVehicleManager vehicleManager,
                                IShapeManager shapeManager,
                                IMovementManager movementManager,
                                IAdditionalInfoManager additionalInfoManager,
                                IMapProvider mapProvider,
                                ITeleportationManager teleportationManager,
                                IPartyManager partyManager,
                                ITradeManager tradeManager,
                                IFriendsManager friendsManager,
                                IDuelManager duelManager,
                                IBankManager bankManager,
                                IQuestsManager questsManager,
                                IUntouchableManager untouchableManager,
                                IWarehouseManager warehouseManager,
                                IShopManager shopManager,
                                ISkillCastingManager skillCastingManager,
                                ICastProtectionManager castProtectionManager,
                                IBlessManager blessManager,
                                IRecoverManager recoverManager,
                                IMarketManager marketManager,
                                IChatManager chatManager,
                                IGamePacketFactory packetFactory,
                                UserManager<DbUser> userManager)
        {
            _logger = logger;
            _usersDatabase = usersDatabase;
            _database = database;
            _characterLogger = characterLogger;
            _gameWorld = gameWorld;
            _databasePreloader = databasePreloader;
            _definitionsPreloder = definitionsPreloder;
            _countryProvider = countryProvider;
            _speedManager = speedManager;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _levelProvider = levelProvider;
            _levelingManager = levelingManager;
            _inventoryManager = inventoryManager;
            _linkingManager = linkingManager;
            _guildManager = guildManager;
            _gameSession = gameSession;
            _stealthManager = stealthManager;
            _attackManager = attackManager;
            _skillsManager = skillsManager;
            _buffsManager = buffsManager;
            _elementProvider = elementProvider;
            _killsManager = killsManager;
            _vehicleManager = vehicleManager;
            _shapeManager = shapeManager;
            _movementManager = movementManager;
            _additionalInfoManager = additionalInfoManager;
            _mapProvider = mapProvider;
            _teleportationManager = teleportationManager;
            _partyManager = partyManager;
            _tradeManager = tradeManager;
            _friendsManager = friendsManager;
            _duelManager = duelManager;
            _bankManager = bankManager;
            _questsManager = questsManager;
            _untouchableManager = untouchableManager;
            _warehouseManager = warehouseManager;
            _shopManager = shopManager;
            _skillCastingManager = skillCastingManager;
            _castProtectionManager = castProtectionManager;
            _blessManager = blessManager;
            _recoverManager = recoverManager;
            _marketManager = marketManager;
            _chatManager = chatManager;
            _packetFactory = packetFactory;
            _userManager = userManager;
        }

        public async Task<Character> CreateCharacter(int userId, uint characterId)
        {
            Character.ClearOutdatedValues(_database, characterId);

            var user = await _usersDatabase.Users.FindAsync(userId);
            if (user is null)
                return null;

            var userCountry = await _database.UsersModeAndCountry.FindAsync(userId);
            if (userCountry is null)
                return null;

            // Before loading character, check, that right map is loaded.
            _gameWorld.EnsureMap(await _database.Characters.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == characterId), userCountry.Country);
            await _database.SaveChangesAsync();

            var dbCharacter = await _database.Characters.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId && c.Id == characterId).ConfigureAwait(false);
            if (dbCharacter is null)
            {
                _logger.LogWarning("Character with id {characterId} for user {userId} is not found.", characterId, userId);
                return null;
            }

            // Load other infos for this character.
            dbCharacter.Skills = await _database.CharacterSkills.AsNoTracking().Where(x => x.CharacterId == characterId).ToListAsync().ConfigureAwait(false);
            dbCharacter.Items = await _database.CharacterItems.AsNoTracking().Where(x => x.CharacterId == characterId).ToListAsync().ConfigureAwait(false);
            dbCharacter.ActiveBuffs = await _database.ActiveBuffs.AsNoTracking().Where(x => x.CharacterId == characterId).ToListAsync().ConfigureAwait(false);
            dbCharacter.Guild = await _database.Guilds.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dbCharacter.GuildId).ConfigureAwait(false);
            dbCharacter.Quests = await _database.CharacterQuests.AsNoTracking().Where(x => x.CharacterId == characterId).ToListAsync().ConfigureAwait(false);
            dbCharacter.QuickItems = await _database.QuickItems.AsNoTracking().Where(x => x.CharacterId == characterId).ToListAsync().ConfigureAwait(false);
            dbCharacter.SavedPositions = await _database.CharacterSavePositions.AsNoTracking().Where(x => x.CharacterId == characterId).ToListAsync().ConfigureAwait(false);
            var bankItems = await _database.BankItems.AsNoTracking().Where(x => x.UserId == dbCharacter.UserId).ToListAsync().ConfigureAwait(false);
            var warehouseItems = await _database.WarehouseItems.AsNoTracking().Where(x => x.UserId == dbCharacter.UserId).ToListAsync().ConfigureAwait(false);

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains(DbRole.ADMIN) || roles.Contains(DbRole.SUPER_ADMIN);

            _levelProvider.Init(dbCharacter.Id, dbCharacter.Level);

            _gameSession.IsAdmin = isAdmin;

            _countryProvider.Init(dbCharacter.Id, userCountry.Country);

            _speedManager.Init(dbCharacter.Id);

            _statsManager.Init(dbCharacter.Id,
                               dbCharacter.Strength,
                               dbCharacter.Dexterity,
                               dbCharacter.Rec,
                               dbCharacter.Intelligence,
                               dbCharacter.Wisdom,
                               dbCharacter.Luck,
                               dbCharacter.StatPoint,
                               dbCharacter.Class,
                               autoStr: dbCharacter.AutoStr,
                               autoDex: dbCharacter.AutoDex,
                               autoRec: dbCharacter.AutoRec,
                               autoInt: dbCharacter.AutoInt,
                               autoWis: dbCharacter.AutoWis,
                               autoLuc: dbCharacter.AutoLuc);

            _additionalInfoManager.Init(dbCharacter.Id, dbCharacter.Race, dbCharacter.Class, dbCharacter.Hair, dbCharacter.Face, dbCharacter.Height, dbCharacter.Gender, dbCharacter.Mode, user.Points, dbCharacter.Name);

            _buffsManager.Init(dbCharacter.Id, dbCharacter.ActiveBuffs);

            _inventoryManager.Init(dbCharacter.Id, dbCharacter.Items, dbCharacter.Gold);

            _levelingManager.Init(dbCharacter.Id, dbCharacter.Exp);

            _skillsManager.Init(dbCharacter.Id, dbCharacter.Skills.Select(s => new Skill(_definitionsPreloder.Skills[(s.SkillId, s.SkillLevel)], s.Number, 0)), dbCharacter.SkillPoint);

            // Pay attention! Init health manager after inventory and stats, so that HP won't go lower, when loading from db.
            _healthManager.Init(dbCharacter.Id, dbCharacter.HealthPoints, dbCharacter.StaminaPoints, dbCharacter.ManaPoints);

            _attackManager.Init(dbCharacter.Id);

            _killsManager.Init(dbCharacter.Id, dbCharacter.Kills, dbCharacter.Deaths, dbCharacter.Victories, dbCharacter.Defeats, dbCharacter.KillLevel, dbCharacter.DeathLevel);

            _vehicleManager.Init(dbCharacter.Id);

            _shapeManager.Init(dbCharacter.Id);

            _movementManager.Init(dbCharacter.Id, dbCharacter.PosX, dbCharacter.PosY, dbCharacter.PosZ, dbCharacter.Angle, MoveMotion.Run);

            _mapProvider.NextMapId = dbCharacter.Map;

            _teleportationManager.Init(dbCharacter.Id, dbCharacter.SavedPositions);

            _partyManager.Init(dbCharacter.Id);

            _tradeManager.Init(dbCharacter.Id);

            _friendsManager.Init(dbCharacter.Id, _database.Friends.AsNoTracking().Include(x => x.Friend).Where(x => x.CharacterId == characterId).Select(x => x.Friend));

            _duelManager.Init(dbCharacter.Id);

            _questsManager.Init(dbCharacter.Id, dbCharacter.Quests);

            _untouchableManager.Init(dbCharacter.Id);

            _skillCastingManager.Init(dbCharacter.Id);

            _castProtectionManager.Init(dbCharacter.Id);

            if (dbCharacter.GuildId != null)
            {
                var guild = await _database.Guilds.AsNoTracking().Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == dbCharacter.GuildId);
                _guildManager.Init(dbCharacter.Id, dbCharacter.GuildId.Value, guild?.Name, dbCharacter.GuildRank, guild is null ? (byte)0 : guild.Rank, guild?.Members);
            }
            else
            {
                _guildManager.Init(dbCharacter.Id);
            }

            _bankManager.Init(dbCharacter.UserId, bankItems.Where(bi => !bi.IsClaimed));

            _warehouseManager.Init(dbCharacter.UserId, dbCharacter.Id, dbCharacter.GuildId, warehouseItems);

            _shopManager.Init(dbCharacter.Id);

            _marketManager.Init(dbCharacter.Id);

            _recoverManager.Init(dbCharacter.Id);
            _recoverManager.Start();

            _stealthManager.Init(dbCharacter.Id);

#if !DEBUG
            _stealthManager.IsAdminStealth = isAdmin;
            if (isAdmin)
                _healthManager.IsAttackable = false;
#endif

            var player = Character.FromDbCharacter(dbCharacter,
                                        _characterLogger,
                                        _databasePreloader,
                                        _definitionsPreloder,
                                        _countryProvider,
                                        _speedManager,
                                        _statsManager,
                                        _additionalInfoManager,
                                        _healthManager,
                                        _levelProvider,
                                        _levelingManager,
                                        _inventoryManager,
                                        _linkingManager,
                                        _guildManager,
                                        _stealthManager,
                                        _attackManager,
                                        _skillsManager,
                                        _buffsManager,
                                        _elementProvider,
                                        _killsManager,
                                        _vehicleManager,
                                        _shapeManager,
                                        _movementManager,
                                        _mapProvider,
                                        _teleportationManager,
                                        _partyManager,
                                        _tradeManager,
                                        _friendsManager,
                                        _duelManager,
                                        _bankManager,
                                        _questsManager,
                                        _untouchableManager,
                                        _warehouseManager,
                                        _shopManager,
                                        _skillCastingManager,
                                        _castProtectionManager,
                                        _blessManager,
                                        _recoverManager,
                                        _marketManager,
                                        _chatManager,
                                        _gameSession,
                                        _packetFactory);

            return player;
        }
    }
}
