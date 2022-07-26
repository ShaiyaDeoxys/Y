using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("MarketItems")]
    public class DbMarketItem : DbEntity
    {
        [Required, ForeignKey(nameof(Market))]
        public uint MarketId { get; set; }

        public DbMarket Market { get; set; }

        [Required]
        public byte Type { get; set; }

        [Required]
        public byte TypeId { get; set; }

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

        public bool HasDyeColor { get; set; }

        public byte DyeColorAlpha { get; set; }

        public byte DyeColorSaturation { get; set; }

        public byte DyeColorR { get; set; }

        public byte DyeColorG { get; set; }

        public byte DyeColorB { get; set; }
    }
}
