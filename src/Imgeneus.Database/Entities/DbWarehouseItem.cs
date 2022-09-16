using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("WarehouseItems")]
    public class DbWarehouseItem : DbEntity
    {
        /// <summary>
        /// Gets the warehouse item associated user id.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        [Required]
        public byte Type { get; set; }

        [Required]
        public byte TypeId { get; set; }

        [Required]
        [Range(0, 239)]
        public byte Slot { get; set; }

        [Required]
        public byte Count { get; set; }

        public ushort Quality { get; set; }

        public int GemTypeId1 { get; set; }
        public int GemTypeId2 { get; set; }
        public int GemTypeId3 { get; set; }
        public int GemTypeId4 { get; set; }
        public int GemTypeId5 { get; set; }
        public int GemTypeId6 { get; set; }

        [Required]
        [MaxLength(20)]
        public string Craftname { get; set; } = string.Empty;

        public DateTime CreationTime { get; set; }

        public DateTime? ExpirationTime { get; set; }

        public bool HasDyeColor { get; set; }

        public byte DyeColorAlpha { get; set; }

        public byte DyeColorSaturation { get; set; }

        public byte DyeColorR { get; set; }

        public byte DyeColorG { get; set; }

        public byte DyeColorB { get; set; }

        public DbWarehouseItem()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
}
