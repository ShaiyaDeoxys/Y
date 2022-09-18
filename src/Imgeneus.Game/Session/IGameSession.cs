using Imgeneus.World.Game.Player;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Session
{
    /// <summary>
    /// Manages game session.
    /// </summary>
    public interface IGameSession
    {
        /// <summary>
        /// Selected character.
        /// </summary>
        Character Character { get; set; }

        /// <summary>
        /// Indicator if it's gm game session.
        /// </summary>
        bool IsAdmin { get; set; }

        /// <summary>
        /// Current TCP connection.
        /// </summary>
        public IWorldClient Client { get; set; }

        /// <summary>
        /// Is character about to log off?
        /// </summary>
        bool IsLoggingOff { get; }

        /// <summary>
        /// Log off from game session. Starts 10 second timer, that will remove player from game world.
        /// </summary>
        void StartLogOff(bool quitGame = false);

        /// <summary>
        /// Stops log off timer.
        /// </summary>
        void StopLogOff();

        /// <summary>
        /// Leaves game world and clears game session.
        /// </summary>
        /// <param name="quitGame"></param>
        /// <returns></returns>
        Task Logout(bool quitGame);
    }
}
