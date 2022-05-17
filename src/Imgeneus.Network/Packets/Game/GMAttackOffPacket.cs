using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMAttackOffPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}
