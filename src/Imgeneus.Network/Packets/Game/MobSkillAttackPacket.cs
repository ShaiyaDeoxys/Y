using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MobSkillAttackPacket : IPacketDeserializer
    {
        public byte Number { get; private set; }

        public uint TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Number = packetStream.Read<byte>();
            TargetId = packetStream.Read<uint>();
        }
    }
}
