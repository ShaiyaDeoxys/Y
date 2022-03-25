using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RaidChangeAutoInvitePacket : IPacketDeserializer
    {
        public bool IsAutoInvite;

        public void Deserialize(ImgeneusPacket packetStream)
        {
            IsAutoInvite = packetStream.Read<bool>();
        }
    }
}
