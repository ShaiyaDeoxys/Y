using Imgeneus.Network.Data;
using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MobInTargetPacket : IPacketDeserializer
    {
        public int TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<int>();
        }
    }

    public record PlayerInTargetPacket : IPacketDeserializer
    {
        public int TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<int>();
        }
    }

    public record TargetClearPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}
