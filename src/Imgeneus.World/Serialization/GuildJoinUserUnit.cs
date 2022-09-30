using Imgeneus.Network.Serialization;
using Imgeneus.Database.Entities;
using BinarySerialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class GuildJoinUserUnit : BaseSerializable
    {
        [FieldOrder(0)]
        public uint Id { get; }

        [FieldOrder(1)]
        public ushort Level { get; }

        [FieldOrder(2)]
        public CharacterProfession Job { get; }

        [FieldOrder(3), FieldLength(21)]
        public string Name { get; }

        public GuildJoinUserUnit(uint id, ushort level, CharacterProfession job, string name)
        {
            Id = id;
            Level = level;
            Job = job;
            Name = name;
        }
    }
}
