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

        /// <summary>
        /// Possibility to hit enemy.
        /// </summary>
        double PhysicalHittingChance { get; }

        /// <summary>
        /// Possibility to escape hit.
        /// </summary>
        double PhysicalEvasionChance { get; }

        /// <summary>
        /// Possibility to magic hit enemy.
        /// </summary>
        double MagicHittingChance { get; }

        /// <summary>
        /// Possibility to escape magic hit.
        /// </summary>
        double MagicEvasionChance { get; }

        /// <summary>
        /// Possibility to make critical hit.
        /// </summary>
        double CriticalHittingChance { get; }
    }
}
