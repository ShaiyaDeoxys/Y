using Imgeneus.Database.Entities;
using System.Collections.Generic;

namespace Imgeneus.Database.Preload
{
    /// <summary>
    /// Database preloader loads game definitions from database, that not gonna change during game server lifetime.
    /// E.g. item definitions, mob definitions, buff/debuff definitions etc.
    /// </summary>
    public interface IDatabasePreloader
    {
        /// <summary>
        /// Preloaded mobs.
        /// </summary>
        Dictionary<ushort, DbMob> Mobs { get; }

        /// <summary>
        /// Preloaded mob items.
        /// </summary>
        Dictionary<(ushort MobId, byte ItemOrder), DbMobItems> MobItems { get; }

        /// <summary>
        /// Preloaded levels.
        /// </summary>
        Dictionary<(Mode Mode, ushort Level), DbLevel> Levels { get; }
    }
}
