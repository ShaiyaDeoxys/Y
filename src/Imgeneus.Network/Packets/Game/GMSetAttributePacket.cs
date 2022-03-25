using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMSetAttributePacket : IPacketDeserializer
    {
        public CharacterAttributeEnum Attribute { get; private set; }
        public uint Value { get; private set; }
        public string Name { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Attribute = (CharacterAttributeEnum)packetStream.Read<byte>();
            Value = packetStream.Read<uint>();
            Name = packetStream.ReadString(21);
        }

        public void Deconstruct(out CharacterAttributeEnum attribute, out uint value, out string charname)
        {
            attribute = Attribute;
            value = Value;
            charname = Name;
        }
    }
}