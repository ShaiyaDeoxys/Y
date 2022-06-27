using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("CharacterSavePoint")]
    public class DbCharacterSavePositions
    {
        public uint CharacterId { get; set; }

        public byte Slot { get; set; }

        public ushort MapId { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public DbCharacter Character { get; set; }
    }
}
