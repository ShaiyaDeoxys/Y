using Imgeneus.Network.PacketProcessor;
using System.Threading.Tasks;

namespace Imgeneus.World
{
    public interface IWorldClient
    {
        /// <summary>
        /// Gets the client's logged user id.
        /// </summary>
        int UserId { get; }

        /// <summary>
        /// Send packet to TCP connection.
        /// </summary>
        void Send(ImgeneusPacket packet, bool shouldEncrypt = true);

        /// <summary>
        /// Clears all session specific services.
        /// </summary>
        Task ClearSession(bool quitGame = false);
    }
}
