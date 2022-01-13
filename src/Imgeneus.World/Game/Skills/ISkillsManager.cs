using System.Threading.Tasks;

namespace Imgeneus.World.Game.Skills
{
    public interface ISkillsManager
    {
        /// <summary>
        /// Inits skill manager.
        /// </summary>
        /// <param name="id">owner id</param>
        /// <param name="skillPoint">free skill points</param>
        void Init(int ownerId, ushort skillPoint);

        /// <summary>
        /// Free skill points.
        /// </summary>
        ushort SkillPoints { get; }

        /// <summary>
        /// Tries to set new value for skill points nad save it to db
        /// </summary>
        /// <param name="skillPoint">value of skill points</param>
        /// <returns>true if success</returns>
        Task<bool> TrySetSkillPoints(ushort skillPoint);
    }
}
