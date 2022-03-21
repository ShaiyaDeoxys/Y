using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record CharacterEnteredMapPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
            // Doesn't contain any inforation, just notification, that map was loaded on client side.
        }
    }
}
