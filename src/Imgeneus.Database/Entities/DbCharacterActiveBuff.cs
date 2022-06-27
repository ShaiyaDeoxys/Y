using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("CharacterActiveBuff")]
    public class DbCharacterActiveBuff : DbEntity
    {
        /// <summary>
        /// Character id.
        /// </summary>
        public uint CharacterId { get; set; }

        /// <summary>
        /// Skill id.
        /// </summary>
        public ushort SkillId { get; set; }

        /// <summary>
        /// Skill level.
        /// </summary>
        public byte SkillLevel { get; set; }

        /// <summary>
        /// Time at which buff ends.
        /// </summary>
        public DateTime ResetTime { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public DbCharacter Character { get; set; }
    }
}
