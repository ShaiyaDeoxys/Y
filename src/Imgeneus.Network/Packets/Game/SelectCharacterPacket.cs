using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record SelectCharacterPacket : IPacketDeserializer
    {
        /// <summary>
        /// Id of character, that should be loaded.
        /// </summary>
        public uint CharacterId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterId = packetStream.Read<uint>();
        }
    }
}
