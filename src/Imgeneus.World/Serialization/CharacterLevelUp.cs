using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class CharacterLevelUp : BaseSerializable
    {
        [FieldOrder(0)]
        public uint CharacterId { get; }

        [FieldOrder(1)]
        public ushort Level { get; }

        [FieldOrder(2)]
        public ushort StatPoint { get; }

        [FieldOrder(3)]
        public ushort SkillPoint { get; }

        [FieldOrder(4)]
        public uint MinLevelExp { get; }

        [FieldOrder(5)]
        public uint NextLevelExp { get; }

        public CharacterLevelUp(uint characterId, ushort level, ushort statPoint, ushort skillPoint, uint minExp, uint nextExp)
        {
            CharacterId = characterId;
            Level = level;
            StatPoint = statPoint;
            SkillPoint = skillPoint;
            MinLevelExp = minExp / 10; // Normalize experience for ep8 game
            NextLevelExp = nextExp / 10; // Normalize experience for ep8 game
        }
    }
}