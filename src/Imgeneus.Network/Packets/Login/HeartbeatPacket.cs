using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Login
{
    public record HeartbeatPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}