using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TeleportSavePositionPacket : IPacketDeserializer
    {
        public byte Index { get; private set; }
        public ushort MapId { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Index = packetStream.Read<byte>();
            MapId = packetStream.Read<ushort>();
            X = packetStream.Read<float>();
            Y = packetStream.Read<float>();
            Z = packetStream.Read<float>();
        }
    }
}
