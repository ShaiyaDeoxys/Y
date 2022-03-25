using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record QuitGamePacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
            // This is empty packet. Needed for server.
        }
    }
}
