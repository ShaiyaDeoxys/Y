using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RaidCreatePacket : IPacketDeserializer
    {
        public bool RaidType { get; private set; }

        public bool AutoJoin { get; private set; }

        public int DropType { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            RaidType = packetStream.Read<bool>();
            AutoJoin = packetStream.Read<bool>();
            DropType = packetStream.Read<int>();
        }
    }
}
