using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TargetGetMobStatePacket : IPacketDeserializer
    {
        public uint MobId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MobId = packetStream.Read<uint>();
        }
    }
}
