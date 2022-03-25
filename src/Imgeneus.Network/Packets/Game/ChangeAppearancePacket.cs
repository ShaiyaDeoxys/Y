using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record ChangeAppearancePacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }

        public byte Slot { get; private set; }

        public byte Hair { get; private set; }

        public byte Face { get; private set; }

        public byte Size { get; private set; }

        public byte Sex { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            Hair = packetStream.Read<byte>();
            Face = packetStream.Read<byte>();
            Size = packetStream.Read<byte>();
            Sex = packetStream.Read<byte>();
        }
    }
}
