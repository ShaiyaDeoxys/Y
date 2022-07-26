using Imgeneus.Network.PacketProcessor;
using System;

namespace Imgeneus.Network.Packets.Game
{
    public record MarketUnregisterItemPacket : IPacketDeserializer
    {
        public uint MarketId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MarketId = packetStream.Read<uint>();
        }
    }
}
