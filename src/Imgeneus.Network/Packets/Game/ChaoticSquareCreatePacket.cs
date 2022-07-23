using Imgeneus.Network.PacketProcessor;
using System;

namespace Imgeneus.Network.Packets.Game
{
    public record ChaoticSquareCreatePacket : IPacketDeserializer
    {
        public void Deserialize(ImgeneusPacket packetStream)
        {
        }
    }
}
