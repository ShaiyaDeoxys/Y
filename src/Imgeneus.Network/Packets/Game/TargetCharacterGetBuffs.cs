using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TargetCharacterGetBuffs : IPacketDeserializer
    {
        public uint TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<uint>();
        }
    }
}
