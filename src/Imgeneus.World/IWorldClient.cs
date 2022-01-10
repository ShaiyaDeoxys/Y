using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.World
{
    public interface IWorldClient
    {
        /// <summary>
        /// Gets the client's logged user id.
        /// </summary>
        int UserId { get; }

        void Send(ImgeneusPacket packet, bool shouldEncrypt = true);
    }
}
