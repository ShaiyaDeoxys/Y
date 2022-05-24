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

        IEnumerable<WorldClient> ConnectedUsers { get; }

        /// <summary>
        /// Disconnect client from server.
        /// </summary>
        void DisconnectUser(Guid clientId);
    }
}
