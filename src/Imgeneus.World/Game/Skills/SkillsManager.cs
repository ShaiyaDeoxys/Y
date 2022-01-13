using Imgeneus.Database;
using Imgeneus.Database.Preload;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Skills
{
    public class SkillsManager : ISkillsManager
    {
        private readonly ILogger<SkillsManager> _logger;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IDatabase _database;

        private int _ownerId;

        public SkillsManager(ILogger<SkillsManager> logger, IDatabasePreloader databasePreloader, IDatabase database)
        {
            _logger = logger;
            _databasePreloader = databasePreloader;
            _database = database;

#if DEBUG
            _logger.LogDebug("SkillsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~SkillsManager()
        {
            _logger.LogDebug("SkillsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init

        public void Init(int ownerId, ushort skillPoint)
        {
            _ownerId = ownerId;
            SkillPoints = skillPoint;
        }

        #endregion

        #region Skill points

        public ushort SkillPoints { get; private set; }

        public async Task<bool> TrySetSkillPoints(ushort value)
        {
            if(SkillPoints == value)
                return true;

            var character = await _database.Characters.FindAsync(_ownerId);
            if(character is null)
                return false;

            character.SkillPoint = value;

            var ok = (await _database.SaveChangesAsync()) > 0;
            if (ok)
                SkillPoints = value;

            return ok;
        }

        #endregion
    }
}
