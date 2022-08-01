using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("MarketCharacterFavorite")]
    public class DbMarketCharacterFavorite : DbEntity
    {
        public uint MarketId { get; set; }
        public DbMarket Market { get; set; }

        public uint CharacterId { get; set; }
        public DbCharacter Character { get; set; }
    }
}
