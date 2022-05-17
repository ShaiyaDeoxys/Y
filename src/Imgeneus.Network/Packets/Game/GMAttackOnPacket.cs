using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMAttackOnPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}
