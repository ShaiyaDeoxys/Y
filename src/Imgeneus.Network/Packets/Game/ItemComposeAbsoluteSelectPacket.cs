using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record ItemComposeAbsoluteSelectPacket : IPacketDeserializer
    {
        public byte Option { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Option = packetStream.Read<byte>();
        }
    }
}
