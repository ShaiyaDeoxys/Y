using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record ChaoticSquareListPacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.ReadByte();
            Slot = packetStream.ReadByte();
        }
    }
}
