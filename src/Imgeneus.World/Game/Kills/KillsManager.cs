using Imgeneus.Database;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Kills
{
    public class KillsManager : IKillsManager
    {
        private readonly ILogger<KillsManager> _logger;
        private readonly IDatabase _database;
        private int _ownerId;

        public KillsManager(ILogger<KillsManager> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;

#if DEBUG
            _logger.LogDebug("KillsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~KillsManager()
        {
            _logger.LogDebug("KillsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId, ushort kills = 0, ushort deaths = 0, ushort victories = 0, ushort defeats = 0)
        {
            _ownerId = ownerId;

            Kills = kills;
            Deaths = deaths;
            Victories = victories;
            Defeats = defeats;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
            {
                _logger.LogError("Character {id} is not found in database.", _ownerId);
                return;
            }

            character.Kills = Kills;
            character.Deaths = Deaths;
            character.Victories = Victories;
            character.Defeats = Defeats;

            await _database.SaveChangesAsync();
        }

        #endregion

        public ushort Kills { get; set; }
        public ushort Deaths { get; set; }
        public ushort Victories { get; set; }
        public ushort Defeats { get; set; }
    }
}
