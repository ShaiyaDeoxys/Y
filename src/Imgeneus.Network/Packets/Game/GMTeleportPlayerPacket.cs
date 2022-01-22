using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMTeleportPlayerPacket : IPacketDeserializer
    {
        public string Name { get; private set; }

        public float X { get; private set; }

        public float Z { get; private set; }

        public ushort MapId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Name = packetStream.ReadString(21);
            X = packetStream.Read<float>();
            Z = packetStream.Read<float>();
            MapId = packetStream.Read<ushort>();
        }

        public void Deconstruct(out string name, out float x, out float z, out ushort mapId)
        {
            name = Name;
            x = X;
            z = Z;
            mapId = MapId;
        }
    }
}