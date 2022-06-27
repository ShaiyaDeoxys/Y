using Imgeneus.World.Game.Country;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// The interface describes the game world member properties.
    /// </summary>
    public interface IWorldMember
    {
        /// <summary>
        /// Unique id inside of a game world.
        /// </summary>
        public uint Id { get; }

        public ICountryProvider CountryProvider { get; }
    }
}
