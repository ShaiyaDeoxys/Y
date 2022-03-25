using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record LearnNewSkillPacket : IPacketDeserializer
    {
        public ushort SkillId { get; private set; }

        public byte SkillLevel { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            SkillId = packetStream.Read<ushort>();
            SkillLevel = packetStream.Read<byte>();
        }
    }
}
