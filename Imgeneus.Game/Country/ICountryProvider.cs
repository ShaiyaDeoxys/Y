using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;

namespace Imgeneus.World.Game.Country
{
    public interface ICountryProvider
    {
        /// <summary>
        /// None, light or dark.
        /// </summary>
        CountryType Country { get; }

        /// <summary>
        /// Inits country of player.
        /// </summary>
        void Init(int ownerId, Fraction country);

        /// <summary>
        /// Inits country of mob.
        /// </summary>
        void Init(int ownerId, MobFraction country);

        /// <summary>
        /// Returns fraction of those players, who are enemies to this mob.
        /// </summary>
        CountryType EnemyPlayersFraction { get; }
    }
}
