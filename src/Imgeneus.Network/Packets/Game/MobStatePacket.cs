using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MobStatePacket : IPacketDeserializer
    {
        public int MobId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MobId = packetStream.Read<int>();
        }
    }
}
