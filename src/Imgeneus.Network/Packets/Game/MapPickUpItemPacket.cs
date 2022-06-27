using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MapPickUpItemPacket : IPacketDeserializer
    {
        public uint ItemId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            ItemId = packetStream.Read<uint>();
        }
    }
}
