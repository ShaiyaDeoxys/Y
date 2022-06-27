using Imgeneus.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Etin
{
    public class EtinManager : IEtinManager
    {
        private readonly ILogger<EtinManager> _logger;
        private readonly IDatabase _database;

        public EtinManager(ILogger<EtinManager> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;
#if DEBUG
            _logger.LogDebug("EtinManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~EtinManager()
        {
            _logger.LogDebug("EtinManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public async Task<int> GetEtin(uint guildId)
        {
            var guild = await _database.Guilds.AsNoTracking().FirstOrDefaultAsync(x => x.Id == guildId);
            return guild is null ? 0 : guild.Etin;
        }
    }
}
