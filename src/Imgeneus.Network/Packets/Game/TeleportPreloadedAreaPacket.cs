using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record class TeleportPreloadedAreaPacket : IPacketDeserializer
    {
        // Always 1. Check GeneralMoveTowns_Client.xml
        public byte MoveTownId { get; private set; }
        public byte MoveTownInfoId { get; private set; }
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MoveTownId = packetStream.ReadByte();
            MoveTownInfoId = packetStream.ReadByte();
            Bag = packetStream.ReadByte();
            Slot = packetStream.ReadByte();
        }
    }
}
