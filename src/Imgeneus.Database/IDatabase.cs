using Imgeneus.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Imgeneus.Database
{
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// Selected country for this user.
        /// </summary>
        public DbSet<DbUserModeAndCountry> UsersModeAndCountry { get; set; }

        /// <summary>
        /// Gets or sets the characters.
        /// </summary>
        public DbSet<DbCharacter> Characters { get; set; }

        /// <summary>
        /// Gets or sets the character items.
        /// </summary>
        public DbSet<DbCharacterItems> CharacterItems { get; set; }

        /// <summary>
        /// Gets or sets character skills.
        /// </summary>
        public DbSet<DbCharacterSkill> CharacterSkills { get; set; }

        /// <summary>
        /// Gets or sets character quests.
        /// </summary>
        public DbSet<DbCharacterQuest> CharacterQuests { get; set; }

        /// <summary>
        /// Gets or sets character saved positions.
        /// </summary>
        public DbSet<DbCharacterSavePositions> CharacterSavePositions { get; set; }

        /// <summary>
        /// Collection of friend pairs.
        /// </summary>
        public DbSet<DbCharacterFriend> Friends { get; set; }

        /// <summary>
        /// Collection of characters' active buffs.
        /// </summary>
        public DbSet<DbCharacterActiveBuff> ActiveBuffs { get; set; }

        /// <summary>
        /// Quick items. E.g. skills on skill bar or motion on skill bar or inventory item on skill bar.
        /// </summary>
        public DbSet<DbQuickSkillBarItem> QuickItems { get; set; }

        /// <summary>
        /// Collection of levels and required experience for them. Taken from original db.
        /// </summary>
        public DbSet<DbLevel> Levels { get; set; }

        /// <summary>
        /// Collection of user's bank items.
        /// </summary>
        public DbSet<DbBankItem> BankItems { get; set; }


        /// <summary>
        /// Collection of user's stored items.
        /// </summary>
        public DbSet<DbWarehouseItem> WarehouseItems { get; set; }

        /// <summary>
        /// Collection of guild's stored items.
        /// </summary>
        public DbSet<DbGuildWarehouseItem> GuildWarehouseItems { get; set; }

        /// <summary>
        /// Collection of guilds.
        /// </summary>
        public DbSet<DbGuild> Guilds { get; set; }

        /// <summary>
        /// Connection between guild and its' npcs.
        /// </summary>
        public DbSet<DbGuildNpcLvl> GuildNpcLvls { get; set; }

        /// <summary>
        /// Join requests to guild.
        /// </summary>
        public DbSet<DbGuildJoinRequest> GuildJoinRequests { get; set; }

        /// <summary>
        /// Auction board.
        /// </summary>
        public DbSet<DbMarket> Market { get; set; }

        /// <summary>
        /// Auction board connected item.
        /// </summary>
        public DbSet<DbMarketItem> MarketItems { get; set; }

        /// <summary>
        /// Auction board deal results.
        /// </summary>
        public DbSet<DbMarketCharacterResultItems> MarketResults { get; set; }

        /// <summary>
        /// Auction money results.
        /// </summary>
        public DbSet<DbMarketCharacterResultMoney> MarketMoneys { get; set; }

        /// <summary>
        /// Market concert items.
        /// </summary>
        public DbSet<DbMarketCharacterFavorite> MarketFavorites { get; set; }

        /// <summary>
        /// Saves changes to database.
        /// </summary>
        public int SaveChanges();

        /// <summary>
        /// Saves changes to database.
        /// </summary>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Migrate database.
        /// </summary>
        public void Migrate();
    }
}
