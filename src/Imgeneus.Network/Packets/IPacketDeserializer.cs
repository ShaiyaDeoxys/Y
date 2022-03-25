using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets
{
    public interface IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream);
    }
}