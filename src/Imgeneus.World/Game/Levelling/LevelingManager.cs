using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Session;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelingManager : ILevelingManager
    {
        private readonly ILogger<LevelingManager> _logger;
        private readonly IDatabase _database;
        private readonly IGameSession _gameSession;
        private readonly ILevelProvider _levelProvider;

        private int _owner;

        public LevelingManager(ILogger<LevelingManager> logger, IDatabase database, IGameSession gameSession, ILevelProvider levelProvider)
        {
            _logger = logger;
            _database = database;
            _gameSession = gameSession;
            _levelProvider = levelProvider;

#if DEBUG
            _logger.LogDebug("LevelingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~LevelingManager()
        {
            _logger.LogDebug("LevelingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(int owner, Mode grow)
        {
            _owner = owner;
            Grow = grow;
        }

        #region Grow

        public Mode Grow { get; private set; }

        public async Task<bool> TrySetGrow(Mode grow)
        {
            if (Grow == grow)
                return true;

            if (grow > Mode.Ultimate)
                return false;

            var character = await _database.Characters.FindAsync(_gameSession.CharId);
            if (character is null)
                return false;

            character.Mode = grow;

            var ok = (await _database.SaveChangesAsync()) > 0;
            if (ok)
                Grow = grow;

            return ok;
        }

        #endregion
    }
}
