using BinarySerialization;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class PartySearchUnit : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Level;

        [FieldOrder(1)]
        public CharacterProfession Job;

        [FieldOrder(2), FieldLength(21)]
        public string Name;

        public PartySearchUnit(Character character)
        {
            Level = (byte)character.LevelProvider.Level;
            Job = character.AdditionalInfoManager.Class;
            Name = character.AdditionalInfoManager.Name;
        }
    }
}
