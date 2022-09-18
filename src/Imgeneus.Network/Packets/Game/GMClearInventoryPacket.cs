using Imgeneus.Network.PacketProcessor;
using System;

namespace Imgeneus.Network.Packets.Game
{
    public record GMClearInventoryPacket : IPacketDeserializer
    {
        public string CharacterName { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterName = packetStream.ReadString(21);
        }
    }
}
