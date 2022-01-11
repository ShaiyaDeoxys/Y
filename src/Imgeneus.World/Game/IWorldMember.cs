using Imgeneus.World.Game.Levelling;

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
        public int Id { get; }

        public ILevelingManager LevelingManager { get; }
    }
}
