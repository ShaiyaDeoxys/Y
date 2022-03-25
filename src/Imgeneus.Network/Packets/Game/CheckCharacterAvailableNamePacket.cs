using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record CheckCharacterAvailableNamePacket : IPacketDeserializer
    {
        /// <summary>
        /// Character name which the client sends.
        /// </summary>
        public string CharacterName { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterName = packetStream.ReadString((int)packetStream.Length - 1);
        }
    }
}
