using Imgeneus.Database;
using Imgeneus.Database.Preload;
using Imgeneus.DatabaseBackgroundService;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Imgeneus.World.Game.Notice;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Elements;
using System.Linq;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Movement;

namespace Imgeneus.World.Game.Player
{
    public class CharacterFactory : ICharacterFactory
    {
        private readonly ILogger<ICharacterFactory> _logger;
        private readonly IDatabase _database;
        private readonly ILogger<Character> _characterLogger;
        private readonly IGameWorld _gameWorld;
        private readonly ICharacterConfiguration _characterConfiguration;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IMapsLoader _mapsLoader;
        private readonly ICountryProvider _countryProvider;
        private readonly ISpeedManager _speedManager;
        private readonly IStatsManager _statsManager;
        private readonly IHealthManager _healthManager;
        private readonly ILevelProvider _levelProvider;
        private readonly ILevelingManager _levelingManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IChatManager _chatManager;
        private readonly ILinkingManager _linkingManager;
        private readonly IDyeingManager _dyeingManager;
        private readonly IMobFactory _mobFactory;
        private readonly INpcFactory _npcFactory;
        private readonly INoticeManager _noticeManager;
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

        public CharacterFactory(ILogger<ICharacterFactory> logger,
                                IDatabase database,
                                ILogger<Character> characterLogger,
                                IGameWorld gameWorld,
                                ICharacterConfiguration characterConfiguration,
                                IBackgroundTaskQueue backgroundTaskQueue,
                                IDatabasePreloader databasePreloader,
                                IMapsLoader mapsLoader,
                                ICountryProvider countryProvider,
                                ISpeedManager speedManager,
                                IStatsManager statsManager,
                                IHealthManager healthManager,
                                ILevelProvider levelProvider,
                                ILevelingManager levelingManager,
                                IInventoryManager inventoryManager,
                                IChatManager chatManager,
                                ILinkingManager linkingManager,
                                IDyeingManager dyeingManager,
                                IMobFactory mobFactory,
                                INpcFactory npcFactory,
                                INoticeManager noticeManager,
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
                                IMovementManager movementManager)
        {
            _logger = logger;
            _database = database;
            _characterLogger = characterLogger;
            _gameWorld = gameWorld;
            _characterConfiguration = characterConfiguration;
            _backgroundTaskQueue = backgroundTaskQueue;
            _databasePreloader = databasePreloader;
            _mapsLoader = mapsLoader;
            _countryProvider = countryProvider;
            _speedManager = speedManager;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _levelProvider = levelProvider;
            _levelingManager = levelingManager;
            _inventoryManager = inventoryManager;
            _chatManager = chatManager;
            _linkingManager = linkingManager;
            _dyeingManager = dyeingManager;
            _mobFactory = mobFactory;
            _npcFactory = npcFactory;
            _noticeManager = noticeManager;
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
        }

        public async Task<Character> CreateCharacter(int userId, int characterId)
        {
            Character.ClearOutdatedValues(_database, characterId);

            var dbCharacter = await _database.Characters
                                             .AsNoTracking()
                                             .Include(c => c.Skills)
                                             .Include(c => c.Items).ThenInclude(ci => ci.Item)
                                             .Include(c => c.ActiveBuffs)
                                             //.Include(c => c.Friends).ThenInclude(cf => cf.Friend)
                                             //.Include(c => c.Guild).ThenInclude(g => g.Members)
                                             .Include(c => c.Quests)
                                             .Include(c => c.QuickItems)
                                             .Include(c => c.User)
                                             .ThenInclude(c => c.BankItems)
                                             .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == characterId);

            if (dbCharacter is null)
            {
                _logger.LogWarning("Character with id {characterId} for user {userId} is not found.", characterId, userId);
                return null;
            }

            _gameSession.CharId = dbCharacter.Id;
            _gameSession.IsAdmin = dbCharacter.User.Authority == 0;

            _countryProvider.Init(dbCharacter.Id, dbCharacter.User.Faction);

            _speedManager.Init(dbCharacter.Id);

            _statsManager.Init(dbCharacter.Id, dbCharacter.Strength, dbCharacter.Dexterity, dbCharacter.Rec, dbCharacter.Intelligence, dbCharacter.Wisdom, dbCharacter.Luck, dbCharacter.StatPoint, dbCharacter.Class);

            _levelProvider.Level = dbCharacter.Level;

            _levelingManager.Init(dbCharacter.Id, dbCharacter.Mode);

            _healthManager.Init(dbCharacter.Id, dbCharacter.HealthPoints, dbCharacter.StaminaPoints, dbCharacter.ManaPoints, profession: dbCharacter.Class);

            _skillsManager.Init(dbCharacter.Id, dbCharacter.Skills.Select(s => new Skill(_databasePreloader.SkillsById[s.SkillId], s.Number, 0)), dbCharacter.SkillPoint);

            _buffsManager.Init(dbCharacter.Id, dbCharacter.ActiveBuffs);

            _inventoryManager.Init(dbCharacter.Id, dbCharacter.Items, dbCharacter.Gold);

            _attackManager.Init(dbCharacter.Id);

            _killsManager.Init(dbCharacter.Id, dbCharacter.Kills, dbCharacter.Deaths, dbCharacter.Victories, dbCharacter.Defeats);

            _vehicleManager.Init(dbCharacter.Id);

            _shapeManager.Init(dbCharacter.Id);

            _movementManager.Init(dbCharacter.Id, dbCharacter.PosX, dbCharacter.PosY, dbCharacter.PosZ, dbCharacter.Angle, MoveMotion.Run);

            _stealthManager.Init(dbCharacter.Id);
            _stealthManager.IsAdminStealth = dbCharacter.User.Authority == 0;

            var player = Character.FromDbCharacter(dbCharacter,
                                        _characterLogger,
                                        _gameWorld,
                                        _characterConfiguration,
                                        _backgroundTaskQueue,
                                        _databasePreloader,
                                        _mapsLoader,
                                        _countryProvider,
                                        _speedManager,
                                        _statsManager,
                                        _healthManager,
                                        _levelProvider,
                                        _levelingManager,
                                        _inventoryManager,
                                        _chatManager,
                                        _linkingManager,
                                        _dyeingManager,
                                        _mobFactory,
                                        _npcFactory,
                                        _noticeManager,
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
                                        _gameSession);

            player.Client = _gameSession.Client; // TODO: remove it.

            return player;
        }
    }
}
