using Imgeneus.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Imgeneus.Database.Context
{
    public class DatabaseContext : DbContext, IDatabase
    {
        public DbSet<DbUserModeAndCountry> UsersModeAndCountry { get; set; }

        public DbSet<DbCharacter> Characters { get; set; }

        public DbSet<DbCharacterItems> CharacterItems { get; set; }

        public DbSet<DbCharacterSkill> CharacterSkills { get; set; }

        public DbSet<DbCharacterQuest> CharacterQuests { get; set; }

        public DbSet<DbCharacterSavePositions> CharacterSavePositions { get; set; }

        public DbSet<DbCharacterFriend> Friends { get; set; }

        public DbSet<DbCharacterActiveBuff> ActiveBuffs { get; set; }

        public DbSet<DbQuickSkillBarItem> QuickItems { get; set; }

        public DbSet<DbLevel> Levels { get; set; }

        public DbSet<DbBankItem> BankItems { get; set; }

        public DbSet<DbWarehouseItem> WarehouseItems { get; set; }

        public DbSet<DbGuildWarehouseItem> GuildWarehouseItems { get; set; }

        public DbSet<DbGuild> Guilds { get; set; }

        public DbSet<DbGuildNpcLvl> GuildNpcLvls { get; set; }

        public DbSet<DbGuildJoinRequest> GuildJoinRequests { get; set; }

        public DbSet<DbMarket> Market { get; set; }

        public DbSet<DbMarketItem> MarketItems { get; set; }

        public DbSet<DbMarketCharacterResultItems> MarketResults { get; set; }

        public DbSet<DbMarketCharacterResultMoney> MarketMoneys { get; set; }

        public DbSet<DbMarketCharacterFavorite> MarketFavorites { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbCharacter>().HasMany(x => x.QuickItems).WithOne(x => x.Character).IsRequired();

            modelBuilder.Entity<DbCharacter>().HasMany(x => x.Friends).WithOne(x => x.Character);

            modelBuilder.Entity<DbCharacterFriend>().HasKey(x => new { x.CharacterId, x.FriendId });

            modelBuilder.Entity<DbCharacter>().HasOne(x => x.Guild);
            modelBuilder.Entity<DbGuild>().HasOne(x => x.Master);
            modelBuilder.Entity<DbGuild>().HasMany(x => x.Members)
                                          .WithOne(x => x.Guild)
                                          .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<DbGuildNpcLvl>().HasKey(x => new { x.GuildId, x.NpcType, x.Group, x.NpcLevel });
            modelBuilder.Entity<DbGuildJoinRequest>().HasKey(x => new { x.CharacterId, x.GuildId });

            modelBuilder.Entity<DbCharacterSavePositions>().HasKey(x => new { x.CharacterId, x.Slot });

            #region Many to many relations
            // Skills.
            modelBuilder.Entity<DbCharacterSkill>().HasKey(x => new { x.CharacterId, x.SkillId });
            modelBuilder.Entity<DbCharacterSkill>().HasOne(pt => pt.Character).WithMany(p => p.Skills).HasForeignKey(pt => pt.CharacterId);

            // Active buffs.
            modelBuilder.Entity<DbCharacterActiveBuff>().HasOne(b => b.Character).WithMany(c => c.ActiveBuffs).HasForeignKey(b => b.CharacterId);

            // Items
            modelBuilder.Entity<DbCharacterItems>().HasOne(pt => pt.Character).WithMany(p => p.Items).HasForeignKey(pt => pt.CharacterId);

            // Quests
            modelBuilder.Entity<DbCharacterQuest>().HasOne(pt => pt.Character).WithMany(p => p.Quests).HasForeignKey(pt => pt.CharacterId);

            // Stored Items
            modelBuilder.Entity<DbGuildWarehouseItem>().HasOne(x => x.Guild).WithMany(x => x.WarehouseItems);

            #endregion

            base.OnModelCreating(modelBuilder);

            // Get rid of "aspnet" prefix in table names
            var tableNamePrefix = "AspNet";
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith(tableNamePrefix))
                    entityType.SetTableName(tableName.Substring(tableNamePrefix.Length));
            }
        }

        /// <summary>
        /// Migrates the database schema.
        /// </summary>
        public void Migrate() => this.Database.Migrate();

        /// <summary>
        /// Check if the database exists.
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists() => (this.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists();
    }
}
