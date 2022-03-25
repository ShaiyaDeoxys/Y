using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildNpcUpgradePacket : IPacketDeserializer
    {
        public byte NpcType { get; private set; }

        public byte NpcGroup { get; private set; }

        public byte NpcLevel { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcType = packetStream.Read<byte>();
            NpcGroup = packetStream.Read<byte>();
            NpcLevel = packetStream.Read<byte>();
        }
    }
}
