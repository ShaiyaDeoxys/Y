using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RaidChangeLootPacket : IPacketDeserializer
    {
        public int LootType { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            LootType = packetStream.Read<int>();
        }
    }
}
