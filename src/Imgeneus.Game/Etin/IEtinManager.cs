using System.Threading.Tasks;

namespace Imgeneus.World.Game.Etin
{
    public interface IEtinManager
    {
        Task<int> GetEtin(uint guildId);
    }
}
