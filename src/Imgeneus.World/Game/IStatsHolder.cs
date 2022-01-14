using Imgeneus.World.Game.Stats;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// Must-have stats.
    /// </summary>
    public interface IStatsHolder
    {
        public IStatsManager StatsManager { get; }

        /// <summary>
        /// Physical defense.
        /// </summary>
        int Defense { get; }

        /// <summary>
        /// Magic resistance.
        /// </summary>
        int Resistance { get; }
    }
}
