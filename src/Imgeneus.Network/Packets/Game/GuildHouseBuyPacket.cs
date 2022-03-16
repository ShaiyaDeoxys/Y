using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildHouseBuyPacket : IPacketDeserializer
    {
        public int NpcId { get; private set; } // what for?

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<int>();
        }
    }
}
