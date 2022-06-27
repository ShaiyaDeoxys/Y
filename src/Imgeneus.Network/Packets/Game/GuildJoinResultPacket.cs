using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildJoinResultPacket : IPacketDeserializer
    {
        public bool Ok { get; private set; }

        public uint CharacterId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Ok = packetStream.Read<bool>();
            CharacterId = packetStream.Read<uint>();
        }
    }
}
