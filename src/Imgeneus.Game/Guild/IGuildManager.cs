using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Guild
{
    public interface IGuildManager : ISessionedService
    {
        void Init(uint ownerId, uint guildId = 0, string name = "", byte rank = 0, byte guildRank = 0, IEnumerable<DbCharacter> members = null);

        /// <summary>
        /// Owner guild id.
        /// </summary>
        uint GuildId { get; }

        /// <summary>
        /// Bool indicator, that shows if character is guild member.
        /// </summary>
        bool HasGuild { get; }

        /// <summary>
        /// Current guild name.
        /// </summary>
        string GuildName { get; }

        /// <summary>
        /// Character rank in the current guild.
        /// </summary>
        byte GuildMemberRank { get; set; }

        /// <summary>
        /// Is it guild creator?
        /// </summary>
        bool IsGuildMaster { get; }

        /// <summary>
        /// Sets guild info and raises changed event.
        /// </summary>
        void SetGuildInfo(uint guildId, string name, byte memberRank);

        /// <summary>
        /// Event is fired, when player enters/leaves the guild.
        /// </summary>
        event Action<uint> OnGuildInfoChanged;

        /// <summary>
        /// Global guild rank.
        /// </summary>
        byte GuildRank { get; }

        /// <summary>
        /// Guild has guild house?
        /// </summary>
        bool HasGuildHouse { get; }

        /// <summary>
        /// Guild house was paid after GRB?
        /// </summary>
        bool HasPaidGuildHouse { get; }

        /// <summary>
        /// Guild has top 30 rank?
        /// </summary>
        bool HasTopRank { get; }

        /// <summary>
        /// Guild member ids for easy notification.
        /// </summary>
        List<DbCharacter> GuildMembers { get; }

        /// <summary>
        /// Checks if character can create a guild.
        /// </summary>
        Task<GuildCreateFailedReason> CanCreateGuild(string guildName);

        /// <summary>
        /// Saves all party members and checks if they leave party.
        /// </summary>
        void InitCreateRequest(string guildName, string guildMessage);

        /// <summary>
        /// Guild creation request.
        /// </summary>
        GuildCreateRequest CreationRequest { get; set; }

        /// <summary>
        /// Creates guild in database.
        /// </summary>
        /// <returns>Db guild, if it was created, otherwise null.</returns>
        Task<DbGuild> TryCreateGuild(string name, string message, uint masterId);

        /// <summary>
        /// Tries to add character to guild.
        /// </summary>
        /// <param name="guildId">guild id</param>
        /// <param name="characterId">new character</param>
        /// <param name="rank">character rank in guild</param>
        /// <returns>db character, if character was added to guild, otherwise null</returns>
        Task<DbCharacter> TryAddMember(uint characterId, byte rank = 9);

        /// <summary>
        /// Tries to remove character from guild.
        /// </summary>
        /// <param name="characterId">character id</param>
        /// <returns>db character, if character was removed, otherwise null</returns>
        Task<bool> TryRemoveMember(uint characterId);

        /// <summary>
        /// Get all guilds in this server.
        /// </summary>
        /// <param name="country">optional param, fraction light or dark</param>
        /// <returns>collection of guilds</returns>
        Task<DbGuild[]> GetAllGuilds(Fraction country = Fraction.NotSelected);

        /// <summary>
        /// Gets guild members.
        /// </summary>
        /// <returns>collection of memebers</returns>
        Task<ICollection<DbCharacter>> GetMemebers(uint guildId);

        /// <summary>
        /// Player requests to join a guild.
        /// </summary>
        /// <returns>true is success</returns>
        Task<bool> RequestJoin(uint guildId, Character player);

        /// <summary>
        /// Removes player from join requests.
        /// </summary>
        Task RemoveRequestJoin(uint playerId);

        /// <summary>
        /// Current join requests.
        /// </summary>
        Task<IEnumerable<DbGuildJoinRequest>> GetJoinRequests();

        /// <summary>
        /// Change guild rank of character.
        /// </summary>
        /// <param name="characterId">character id</param>
        /// <param name="demote">decrease or increase rank?</param>
        /// <returns>new rank</returns>
        Task<byte> TryChangeRank(uint characterId, bool demote);

        /// <summary>
        /// Tries to remove guild.
        /// </summary>
        /// <returns>true if was removed</returns>
        Task<bool> TryDeleteGuild();

        /// <summary>
        /// Tries to buy guild house.
        /// </summary>
        Task<GuildHouseBuyReason> TryBuyHouse();

        /// <summary>
        /// Gets guild rank.
        /// </summary>
        byte GetRank(int guildId);

        /// <summary>
        /// Reloads guild ranks after GRB.
        /// </summary>
        void ReloadGuildRanks(IEnumerable<(uint GuildId, int Points, byte Rank)> results);

        /// <summary>
        /// Gets guild's etin.
        /// </summary>
        Task<int> GetEtin();

        /// <summary>
        /// Gives etin from character inventory to guild. Saves changes to database.
        /// </summary>
        /// <returns>List of removed etin items.</returns>
        Task<IList<Item>> ReturnEtin();

        /// <summary>
        /// Finds npcs & their levels assigned to this guild.
        /// </summary>
        Task<IEnumerable<DbGuildNpcLvl>> GetGuildNpcs();

        /// <summary>
        /// Checks if guild has enough rank in order to use NPC.
        /// </summary>
        /// <param name="guildId">guild id</param>
        /// <param name="type">npc type</param>
        /// <param name="typeId">npc type is</param>
        /// <param name="requiredRank">guild's rank, that needed for this NPC</param>
        /// <returns>true, if player can use it</returns>
        bool CanUseNpc(NpcType type, short typeId, out byte requiredRank);

        /// <summary>
        /// Checks if guild has bought NPC of the right level.
        /// </summary>
        /// <param name="type">npc type</param>
        /// <param name="typeId">npc type is</param>
        /// <returns>true, if guild has right NPC level</returns>
        bool HasNpcLevel(NpcType type, short typeId);

        /// <summary>
        /// Checks if guild has npc lvl of npc group.
        /// </summary>
        /// <param name="type">npc type</param>
        /// <param name="level">npc level</param>
        /// <returns>true, if guild has NPC level, that is >= provided level</returns>
        bool HasNpcLevel(NpcType type, byte level);

        /// <summary>
        /// Gets discount in %.
        /// </summary>
        /// <returns>for ex. 20% discount => 0.2f</returns>
        float GetDiscount(NpcType type, short typeId);

        /// <summary>
        /// Tries to upgrade NPC. Updates guild etin as well.
        /// </summary>
        /// <param name="npcType">npc type</param>
        /// <param name="npcGroup">npc group</param>
        /// <param name="npcLevel">npc next level</param>
        Task<GuildNpcUpgradeReason> TryUpgradeNPC(NpcType npcType, byte npcGroup, byte npcLevel);

        /// <summary>
        /// Gets blacksmith npc extra rates based on NPC level.
        /// </summary>
        (byte LinkRate, byte RepaireRate) GetBlacksmithRates();
    }
}
