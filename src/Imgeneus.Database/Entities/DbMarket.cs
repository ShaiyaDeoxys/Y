using Imgeneus.Database.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("Market")]
    public class DbMarket : DbEntity
    {
        public uint CharacterId { get; set; }
        public DbCharacter Character { get; set; }

        public uint MinMoney { get; set; }

        public uint DirectMoney { get; set; }

        public MarketType MarketType { get; set; }

        public uint GuaranteeMoney { get; set; }

        public uint TenderCharacterId { get; set; }

        public uint TenderMoney { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public uint MarketItemId { get; set; }

        public DbMarketItem MarketItem { get; set; }
    }
}
