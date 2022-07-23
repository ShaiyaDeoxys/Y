using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record ChaoticSquareRecipePacket : IPacketDeserializer
    {
        public byte Type { get; private set; }
        public byte TypeId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Type = packetStream.ReadByte();
            TypeId = packetStream.ReadByte();
        }
    }
}
