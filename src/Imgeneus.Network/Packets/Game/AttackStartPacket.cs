using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record AttackStartPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
            // Looks like this packet client sends in order to get permission to attack some target(mob?).
        }
    }
}
