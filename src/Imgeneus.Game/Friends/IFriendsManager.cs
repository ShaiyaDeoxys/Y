using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Friends
{
    public interface IFriendsManager : ISessionedService
    {
        void Init(uint ownerId, IEnumerable<DbCharacter> friends);

        /// <summary>
        /// Dictionary of friends.
        /// </summary>
        ConcurrentDictionary<uint, Friend> Friends { get; init; }

        /// <summary>
        /// Character from whom friend request was sent last time.
        /// </summary>
        Character LastRequester { get; set; }

        /// <summary>
        /// Saves friend to database.
        /// </summary>
        Task<Friend> AddFriend(Character character);

        /// <summary>
        /// Removed friend from database.
        /// </summary>
        Task<Friend> DeleteFriend(uint id);
    }
}
