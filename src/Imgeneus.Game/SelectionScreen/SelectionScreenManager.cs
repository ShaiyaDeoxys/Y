using Imgeneus.Core.Extensions;
using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Player.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.SelectionScreen
{
    /// <inheritdoc/>
    public class SelectionScreenManager : ISelectionScreenManager
    {
#if EP8_V2
        public const byte MaxCharacterNumber = 6;
#else
        public const byte MaxCharacterNumber = 5;
#endif


        private readonly ILogger<SelectionScreenManager> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly ICharacterConfiguration _characterConfiguration;
        private readonly IDatabase _database;
        private readonly IDatabasePreloader _databasePreloader;

        public SelectionScreenManager(ILogger<SelectionScreenManager> logger, IGameWorld gameWorld, ICharacterConfiguration characterConfiguration, IDatabase database, IDatabasePreloader databasePreloader)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _characterConfiguration = characterConfiguration;
            _database = database;
            _databasePreloader = databasePreloader;

#if DEBUG
            _logger.LogDebug($"SelectionScreenManager {GetHashCode()} created");
#endif
        }

#if DEBUG
        ~SelectionScreenManager()
        {
            _logger.LogDebug($"SelectionScreenManager {GetHashCode()} collected by GC");
        }
#endif

        public async Task<IEnumerable<DbCharacter>> GetCharacters(int userId)
        {
            var characters = await _database.Characters
                                        .AsNoTracking()
                                        .Include(c => c.Items)
                                        .Include(x => x.User)
                                        .Where(u => u.UserId == userId && !u.IsDelete)
                                        .ToListAsync();

            foreach (var character in characters)
                _gameWorld.EnsureMap(character);

            return characters;
        }

        public async Task<bool> TryCreateCharacter(int userId, CreateCharacterPacket createCharacterPacket)
        {
            // Get number of user characters.
            var characters = await _database.Characters.Where(x => x.UserId == userId && !x.IsDelete).ToListAsync();
            if (characters.Count == MaxCharacterNumber)
            {
                // Max number of characters reached.
                return false;
            }

            byte freeSlot = createCharacterPacket.Slot;
            if (characters.Any(c => c.Slot == freeSlot && !c.IsDelete))
            {
                // Wrong slot.
                return false;
            }

            var defaultStats = _characterConfiguration.DefaultStats.FirstOrDefault(s => s.Job == createCharacterPacket.Class);
            if (defaultStats is null)
            {
                // Something went very wrong. No default stats for this job.
                return false;
            }

            var user = await _database.Users.FindAsync(userId);
            var createConfig = _characterConfiguration.CreateConfigs.FirstOrDefault(p => p.Country == user.Faction && p.Job == createCharacterPacket.Class);
            if (createConfig is null)
            {
                // Something went very wrong. No default position for this job.
                return false;
            }

            // Validate CharacterName
            if (!createCharacterPacket.CharacterName.IsValidCharacterName())
            {
                return false;
            }

            DbCharacter character = new DbCharacter()
            {
                Name = createCharacterPacket.CharacterName,
                Race = createCharacterPacket.Race,
                Mode = createCharacterPacket.Mode,
                Hair = createCharacterPacket.Hair,
                Face = createCharacterPacket.Face,
                Height = createCharacterPacket.Height,
                Class = createCharacterPacket.Class,
                Gender = createCharacterPacket.Gender,
                Strength = defaultStats.Str,
                Dexterity = defaultStats.Dex,
                Rec = defaultStats.Rec,
                Intelligence = defaultStats.Int,
                Wisdom = defaultStats.Wis,
                Luck = defaultStats.Luc,
                Level = 1,
                Slot = freeSlot,
                UserId = userId,
                Map = createConfig.MapId,
                PosX = createConfig.X,
                PosY = createConfig.Y,
                PosZ = createConfig.Z,
                HealthPoints = 1000,
                ManaPoints = 1000,
                StaminaPoints = 1000,
            };

            var result = await _database.Characters.AddAsync(character);
            if (await _database.SaveChangesAsync() > 0)
            {
                for (byte i = 0; i < createConfig.StartItems.Length; i++)
                {
                    var itm = createConfig.StartItems[i];
                    await _database.CharacterItems.AddAsync(new DbCharacterItems()
                    {
                        CharacterId = result.Entity.Id,
                        Type = itm.Type,
                        TypeId = itm.TypeId,
                        Count = itm.Count,
                        Quality = _databasePreloader.Items[(itm.Type, itm.TypeId)].Quality,
                        Bag = 1,
                        Slot = i
                    });
                }

                await _database.SaveChangesAsync();

                characters.Add(character);
                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task<Fraction> GetFaction(int userId)
        {
            var user = await _database.Users.FindAsync(userId);
            return user.Faction;
        }

        public async Task SetFaction(int userId, Fraction fraction)
        {
            var user = await _database.Users.FindAsync(userId);
            user.Faction = fraction;

            await _database.SaveChangesAsync();
        }

        public async Task<Mode> GetMaxMode(int userId)
        {
            var user = await _database.Users.FindAsync(userId);

            Mode maxMode = Mode.Normal;
#if EP8_V2
            maxMode = Mode.Ultimate;
#else
            maxMode = user.MaxMode;
#endif
            return maxMode;
        }

        public async Task<bool> TryDeleteCharacter(int userId, uint id)
        {
            var character = await _database.Characters.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id);
            if (character is null)
                return false;

            character.IsDelete = true;
            character.DeleteTime = DateTime.UtcNow;

            var count = await _database.SaveChangesAsync();
            return count == 1;
        }

        public async Task<bool> TryRestoreCharacter(int userId, uint id)
        {
            var character = await _database.Characters.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id);
            if (character is null)
                return false;

            character.IsDelete = false;
            character.DeleteTime = null;

            var count = await _database.SaveChangesAsync();
            return count == 1;
        }

        public async Task<bool> TryRenameCharacter(int userId, uint id, string newName)
        {
            var character = await _database.Characters.FirstOrDefaultAsync(c => c.UserId == userId && c.Id == id);
            if (character is null)
                return false;

            // Validate the new name
            var nameIsValid = newName.IsValidCharacterName();

            // Check that name isn't in use
            var characterWithNewName = await _database.Characters.FirstOrDefaultAsync(c => c.Name == newName);

            if (!nameIsValid || characterWithNewName != null)
                return false;

            character.Name = newName;
            character.IsRename = false;

            var count = await _database.SaveChangesAsync();
            return count == 1;
        }
    }
}
