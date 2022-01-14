using BinarySerialization;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.Network.Serialization
{
    public class CharacterSkills : BaseSerializable
    {
        [FieldOrder(0)]
        public ushort CharSkillPoints { get; }

        [FieldOrder(1)]
        public byte SkillsCount { get; }

        [FieldOrder(2)]
        public byte[] Skills { get; }

        public CharacterSkills(Character character)
        {
            CharSkillPoints = character.SkillsManager.SkillPoints;
            SkillsCount = (byte)character.SkillsManager.Skills.Count;

            var serializedSkills = new List<byte>();
            foreach (var skill in character.SkillsManager.Skills.Values)
            {
                var serialized = new SerializedSkill(skill).Serialize();
                serializedSkills.AddRange(serialized);
            }
            Skills = serializedSkills.ToArray();
        }
    }
}
