using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MobInTargetPacket : IPacketDeserializer
    {
        public uint TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<uint>();
        }
    }

    public record PlayerInTargetPacket : IPacketDeserializer
    {
        public uint TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<uint>();
        }
    }

    public record TargetClearPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}
