using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RenameCharacterPacket : IPacketDeserializer
    {
        public uint CharacterId { get; private set; }
        public string NewName { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterId = packetStream.Read<uint>();
            NewName = packetStream.ReadString(21);
        }
    }
}