using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TargetCharacterGetBuffs : IPacketDeserializer
    {
        public int TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<int>();
        }
    }
}
