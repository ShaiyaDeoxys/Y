using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record PartyKickPacket : IPacketDeserializer
    {
        public int CharacterId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterId = packetStream.Read<int>();
        }
    }
}
