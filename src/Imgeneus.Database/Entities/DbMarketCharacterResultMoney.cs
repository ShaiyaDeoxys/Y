using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("MarketCharacterResultMoney")]
    public class DbMarketCharacterResultMoney : DbEntity
    {
        public uint MarketId { get; set; }
        public DbMarket Market { get; set; }

        public uint CharacterId { get; set; }
        public DbCharacter Character { get; set; }

        public DateTime EndDate { get; set; }

        public bool Success { get; set; }

        public uint Money { get; set; }

        public uint ReturnMoney { get; set; }
    }
}
