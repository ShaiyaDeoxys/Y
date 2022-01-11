using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMCharacterOffPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}
