using Imgeneus.World.Game.Session;

namespace Imgeneus.World.Game.Kills
{
    public interface IKillsManager : ISessionedService
    {
        void Init(uint ownerId, ushort kills = 0, ushort deaths = 0, ushort victories = 0, ushort defeats = 0);

        ushort Kills { get; set; }
        ushort Deaths { get; set; }
        ushort Victories { get; set; }
        ushort Defeats { get; set; }
    }
}
