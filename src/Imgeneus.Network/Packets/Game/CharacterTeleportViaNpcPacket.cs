using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record CharacterTeleportViaNpcPacket : IPacketDeserializer
    {
        public int NpcId { get; private set; }

        public byte GateId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<int>();
            GateId = packetStream.Read<byte>();
        }
    }
}
