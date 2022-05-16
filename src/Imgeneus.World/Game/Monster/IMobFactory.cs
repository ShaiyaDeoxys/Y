using Imgeneus.World.Game.AI;

namespace Imgeneus.World.Game.Monster
{
    public interface IMobFactory
    {
        /// <summary>
        /// Creates mob instance.
        /// </summary>
        /// <param name="mobId">mob id</param>
        /// <param name="shouldRebirth">should rebirth in some time?</param>
        /// <param name="moveArea">where mob can walk</param>
        /// <returns>mob instance</returns>
        public Mob CreateMob(ushort mobId, bool shouldRebirth, MoveArea moveArea);
    }
}
