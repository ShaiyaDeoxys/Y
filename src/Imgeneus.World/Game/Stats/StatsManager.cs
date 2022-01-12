using Imgeneus.Database;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Stats
{
    public class StatsManager : IStatsManager
    {
        private readonly ILogger<StatsManager> _logger;
        private readonly IDatabase _database;

        private int _ownerId;

        public StatsManager(ILogger<StatsManager> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;

#if DEBUG
            _logger.LogDebug("StatsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~StatsManager()
        {
            _logger.LogDebug("StatsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(int ownerId, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc, ushort statPoints)
        {
            _ownerId = ownerId;
            Strength = str;
            Dexterity = dex;
            Reaction = rec;
            Intelligence = intl;
            Wisdom = wis;
            Luck = luc;
            StatPoint = statPoints;

            ExtraStr = 0;
            ExtraDex = 0;
            ExtraRec = 0;
            ExtraInt = 0;
            ExtraLuc = 0;
            ExtraWis = 0;
            ExtraDefense = 0;
            ExtraResistance = 0;
        }

        #region Constants
        public ushort Strength { get; private set; }
        public ushort Dexterity { get; private set; }
        public ushort Reaction { get; private set; }
        public ushort Intelligence { get; private set; }
        public ushort Luck { get; private set; }
        public ushort Wisdom { get; private set; }
        #endregion

        #region Extras
        public int ExtraStr { get; set; }

        public int ExtraDex { get; set; }

        public int ExtraRec { get; set; }

        public int ExtraInt { get; set; }

        public int ExtraLuc { get; set; }

        public int ExtraWis { get; set; }

        public int ExtraDefense { get; set; }

        public int ExtraResistance { get; set; }

        #endregion

        #region Total
        public int TotalStr => Strength + ExtraStr;
        public int TotalDex => Dexterity + ExtraDex;
        public int TotalRec => Reaction + ExtraRec;
        public int TotalInt => Intelligence + ExtraInt;
        public int TotalWis => Wisdom + ExtraWis;
        public int TotalLuc => Luck + ExtraLuc;
        #endregion

        public ushort Absorption { get; set; }

        public ushort StatPoint { get; private set; }

        public async Task<bool> TrySetStats(ushort? str = null, ushort? dex = null, ushort? rec = null, ushort? intl = null, ushort? wis = null, ushort? luc = null, ushort? statPoint = null)
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if(character is null)
                return false;

            character.Strength = str.HasValue ? str.Value : character.Strength;
            character.Dexterity = dex.HasValue ? dex.Value : character.Dexterity;
            character.Rec = rec.HasValue ? rec.Value : character.Rec;
            character.Intelligence = intl.HasValue ? intl.Value : character.Intelligence;
            character.Wisdom = wis.HasValue ? wis.Value : character.Wisdom;
            character.Luck = luc.HasValue ? luc.Value : character.Luck;
            character.StatPoint = statPoint.HasValue ? statPoint.Value : character.StatPoint;

            var count = await _database.SaveChangesAsync();

            if (count > 0)
            {
                Strength = character.Strength;
                Dexterity = character.Dexterity;
                Reaction = character.Rec;
                Intelligence = character.Intelligence;
                Wisdom = character.Wisdom;
                Luck = character.Luck;
                StatPoint = character.StatPoint;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
