using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("GuildJoinRequests")]
    public class DbGuildJoinRequest
    {
        /// <summary>
        /// Character id, that wants to join.
        /// </summary>
        public uint CharacterId { get; set; }

        /// <summary>
        /// Request owner.
        /// </summary>
        [ForeignKey(nameof(CharacterId))]
        public DbCharacter Character { get; set; }

        /// <summary>
        /// To which guild player wants to enter.
        /// </summary>
        public uint GuildId { get; set; }

        [ForeignKey(nameof(GuildId))]
        public DbGuild Guild { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
