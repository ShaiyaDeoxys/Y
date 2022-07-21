using System;

namespace Imgeneus.Game.Recover
{
    public interface IRecoverManager : IDisposable
    {
        void Init(uint ownerId);
    }
}
