using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMClearMobInTargetPacket : IPacketDeserializer
    {
        public uint Id { get; private set; }
        public void Deserialize(ImgeneusPacket packetStream)
        {
            Id = packetStream.Read<uint>();
        }
    }
}
