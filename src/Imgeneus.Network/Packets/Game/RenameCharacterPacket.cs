using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RenameCharacterPacket : IPacketDeserializer
    {
        public int CharacterId { get; private set; }
        public string NewName { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterId = packetStream.Read<int>();
            NewName = packetStream.ReadString(21);
        }
    }
}