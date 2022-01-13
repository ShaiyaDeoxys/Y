using Imgeneus.Database.Entities;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Levelling
{
    public interface ILevelingManager
    {
        void Init(Mode grow);

        /// <summary>
        /// Beginner, Normal, Hard or Ultimate.
        /// </summary>
        Mode Grow { get; }

        /// <summary>
        /// Tries to change user's grow.
        /// </summary>
        /// <returns></returns>
        Task<bool> TrySetGrow(Mode grow);
    }
}
