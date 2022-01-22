using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMTeleportMapPacket : IPacketDeserializer
    {
        public ushort MapId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MapId = packetStream.Read<ushort>();
        }
    }
}