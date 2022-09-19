using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TeleportToBattlegroundPacket : IPacketDeserializer
    {
        public ushort MapId { get; private set; }
        public uint Unknown { get; private set; }
        public uint Unknown2 { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MapId = packetStream.Read<ushort>();
            Unknown = packetStream.Read<uint>();
            Unknown2 = packetStream.Read<uint>();
        }
    }
}
