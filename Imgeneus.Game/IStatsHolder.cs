using Imgeneus.World.Game.Stats;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// Must-have stats.
    /// </summary>
    public interface IStatsHolder
    {
        public IStatsManager StatsManager { get; }
    }
}
