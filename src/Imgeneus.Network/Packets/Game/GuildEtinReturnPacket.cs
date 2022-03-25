using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildEtinReturnPacket : IPacketDeserializer
    {
        public int NpcId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<int>();
        }
    }
}
