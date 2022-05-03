using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record class TeleportPreloadedAreaPacket : IPacketDeserializer
    {
        public byte Unknwn1 { get; private set; }
        public byte Unknwn2 { get; private set; }
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Unknwn1 = packetStream.ReadByte();
            Unknwn2 = packetStream.ReadByte();
            Bag = packetStream.ReadByte();
            Slot = packetStream.ReadByte();
        }
    }
}
