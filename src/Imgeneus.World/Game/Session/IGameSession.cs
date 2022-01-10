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
        int CharId { get; set; }

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
    }
}
