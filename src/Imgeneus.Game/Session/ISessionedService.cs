using System.Threading.Tasks;

namespace Imgeneus.World.Game.Session
{
    /// <summary>
    /// Every service or manager, that is specific for game session must implement this interface
    /// and clear session values on character leaves game world.
    /// </summary>
    public interface ISessionedService
    {
        /// <summary>
        /// Clears session values after character leaves the game world.
        /// </summary>
        Task Clear();
    }
}
