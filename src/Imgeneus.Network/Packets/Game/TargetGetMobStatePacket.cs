using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public class TargetGetMobStatePacket : IPacketDeserializer
    {
        public int MobId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MobId = packetStream.Read<int>();
        }
    }
}
