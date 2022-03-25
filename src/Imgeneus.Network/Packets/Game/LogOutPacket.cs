using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record LogOutPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
            // Logout packet is called, when user  leaves game or goes back to selection screen.
        }
    }
}
