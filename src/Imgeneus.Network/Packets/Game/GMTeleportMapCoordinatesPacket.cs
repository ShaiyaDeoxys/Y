using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMTeleportMapCoordinatesPacket : IPacketDeserializer
    {
        public float X { get; private set; }

        public float Z { get; private set; }

        public ushort MapId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            X = packetStream.Read<float>();
            Z = packetStream.Read<float>();
            MapId = packetStream.Read<ushort>();
        }

        public void Deconstruct(out float x, out float z, out ushort mapId)
        {
            x = X;
            z = Z;
            mapId = MapId;
        }

    }
}
