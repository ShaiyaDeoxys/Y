using System.Collections.Generic;

namespace Imgeneus.World.Game.Teleport
{
    public interface IMoveTownsConfiguration
    {
        Dictionary<byte, MoveTownInfo> MoveTowns { get; }
    }
}
