using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildJoinRequestPacket : IPacketDeserializer
    {
        public int GuildId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            GuildId = packetStream.Read<int>();
        }
    }
}
