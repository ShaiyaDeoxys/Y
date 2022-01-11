using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Stats
{
    public class StatsManager : IStatsManager
    {
        private readonly ILogger<StatsManager> _logger;

        public StatsManager(ILogger<StatsManager> logger)
        {
            _logger = logger;

#if DEBUG
            _logger.LogDebug($"StatsManager {GetHashCode()} created");
#endif
        }

#if DEBUG
        ~StatsManager()
        {
            _logger.LogDebug($"StatsManager {GetHashCode()} collected by GC");
        }
#endif

        public void Init(ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc)
        {
            Strength = str;
            Dexterity = dex;
            Reaction = rec;
            Intelligence = intl;
            Wisdom = wis;
            Luck = luc;

            ExtraStr = 0;
            ExtraDex = 0;
            ExtraRec = 0;
            ExtraInt = 0;
            ExtraLuc = 0;
            ExtraWis = 0;
            ExtraDefense = 0;
            ExtraResistance = 0;
            ExtraHP = 0;
            ExtraSP = 0;
            ExtraMP = 0;
        }

        #region Constants
        public ushort Strength { get; set; }
        public ushort Dexterity { get; set; }
        public ushort Reaction { get; set; }
        public ushort Intelligence { get; set; }
        public ushort Luck { get; set; }
        public ushort Wisdom { get; set; }
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

        public int ExtraHP { get; set; }

        public int ExtraSP { get; set; }

        public int ExtraMP { get; set; }
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
    }
}
