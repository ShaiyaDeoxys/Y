using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public struct PingPacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
            // This is empty packet. Needed for server.
        }
    }
}
