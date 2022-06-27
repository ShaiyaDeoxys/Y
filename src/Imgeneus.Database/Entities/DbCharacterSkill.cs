using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("CharacterSkill")]
    public class DbCharacterSkill
    {
        public uint CharacterId { get; set; }
        public ushort SkillId { get; set; }
        public byte SkillLevel { get; set; }

        /// <summary>
        /// This is unique learned skill number. It's used by client to send which skill was used.
        /// Think of it as skill index.
        /// </summary>
        public byte Number { get; set; }

        public DbCharacter Character { get; set; }
    }
}
