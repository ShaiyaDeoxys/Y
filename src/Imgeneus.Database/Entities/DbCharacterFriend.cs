using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("CharacterFriends")]
    public class DbCharacterFriend
    {
        /// <summary>
        /// Character key.
        /// </summary>
        [Required]
        public uint CharacterId { get; set; }

        [ForeignKey(nameof(CharacterId))]
        public DbCharacter Character { get; set; }

        /// <summary>
        /// Friend key.
        /// </summary>
        [Required]
        public uint FriendId { get; set; }

        [ForeignKey(nameof(FriendId))]
        public DbCharacter Friend { get; set; }

        public DbCharacterFriend(uint characterId, uint friendId)
        {
            CharacterId = characterId;
            FriendId = friendId;
        }

    }
}
