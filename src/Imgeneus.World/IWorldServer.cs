using System;
using System.Collections.Generic;

namespace Imgeneus.World
{
    public interface IWorldServer
    {
        /// <summary>
        /// Start server.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Is currently running?
        /// </summary>
        bool IsRunning { get; }

        ICollection<WorldClient> ConnectedUsers { get; }

        /// <summary>
        /// Disconnect client from server.
        /// </summary>
        void DisconnectUser(Guid clientId, bool silent);
    }
}
