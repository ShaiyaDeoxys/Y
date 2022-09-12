using Imgeneus.Database.Entities;
using Imgeneus.GameDefinitions.Constants;

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
        void Init(uint ownerId, Fraction country);

        /// <summary>
        /// Inits country of mob.
        /// </summary>
        void Init(uint ownerId, MobFraction country);

        /// <summary>
        /// Returns fraction of those players, who are enemies to this mob.
        /// </summary>
        CountryType EnemyPlayersFraction { get; }
    }
}
