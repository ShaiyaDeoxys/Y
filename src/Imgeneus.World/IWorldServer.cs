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
    }
}
