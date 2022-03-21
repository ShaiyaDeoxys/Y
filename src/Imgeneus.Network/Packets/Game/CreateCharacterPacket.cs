using Imgeneus.Database.Entities;
using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record CreateCharacterPacket : IPacketDeserializer
    {
        public byte Slot { get; private set; }

        public Race Race { get; private set; }

        public Mode Mode { get; private set; }

        public byte Hair { get; private set; }

        public byte Face { get; private set; }

        public byte Height { get; private set; }

        public CharacterProfession Class { get; private set; }

        public Gender Gender { get; private set; }

        public string CharacterName { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Slot = packetStream.Read<byte>();
            Race = (Race)packetStream.Read<byte>();
            Mode = (Mode)packetStream.Read<byte>();
            Hair = packetStream.Read<byte>();
            Face = packetStream.Read<byte>();
            Height = packetStream.Read<byte>();
            Class = (CharacterProfession)packetStream.Read<byte>();
            Gender = (Gender)packetStream.Read<byte>();
            CharacterName = packetStream.ReadString((int)packetStream.Length - 1);
        }
    }
}
