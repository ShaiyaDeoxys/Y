using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildAgreePacket : IPacketDeserializer
    {
        /// <summary>
        /// Player agrees to create a guild.
        /// </summary>
        public bool Ok { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Ok = packetStream.Read<bool>();
        }
    }
}
