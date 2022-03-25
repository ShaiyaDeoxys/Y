using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMCreateMobPacket : IPacketDeserializer
    {
        public ushort MobId { get; private set; }

        public byte NumberOfMobs { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MobId = packetStream.Read<ushort>();
            NumberOfMobs = packetStream.Read<byte>();
        }
    }
}
