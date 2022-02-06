using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RaidResponsePacket : IPacketDeserializer
    {
        public bool IsDeclined { get; private set; }

        public int CharacterId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            IsDeclined = packetStream.Read<bool>();
            CharacterId = packetStream.Read<int>();
        }
    }
}
