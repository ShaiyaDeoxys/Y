using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class CharacterAdditionalStats : BaseSerializable
    {
        [FieldOrder(0)]
        public int Strength { get; }

        [FieldOrder(1)]
        public int Rec { get; }

        [FieldOrder(2)]
        public int Intelligence { get; }

        [FieldOrder(3)]
        public int Wisdom { get; }

        [FieldOrder(4)]
        public int Dexterity { get; }

        [FieldOrder(5)]
        public int Luck { get; }

        [FieldOrder(6)]
        public int MinAttack { get; }

        [FieldOrder(7)]
        public int MaxAttack { get; }

        [FieldOrder(8)]
        public int MinMagicAttack { get; }

        [FieldOrder(9)]
        public int MaxMagicAttack { get; }

        [FieldOrder(10)]
        public int Defense { get; }

        [FieldOrder(11)]
        public int Resistance { get; }

        public CharacterAdditionalStats(Character character)
        {
            Strength = character.StatsManager.ExtraStr;
            Rec = character.StatsManager.ExtraRec;
            Intelligence = character.StatsManager.ExtraInt;
            Wisdom = character.StatsManager.ExtraWis;
            Dexterity = character.StatsManager.ExtraDex;
            Luck = character.StatsManager.ExtraLuc;
            Defense = character.StatsManager.TotalDefense;
            Resistance = character.StatsManager.TotalResistance;
            MinAttack = character.StatsManager.MinAttack;
            MaxAttack = character.StatsManager.MaxAttack;
            MinMagicAttack = character.StatsManager.MinMagicAttack;
            MaxMagicAttack = character.StatsManager.MaxMagicAttack;
        }
    }
}
