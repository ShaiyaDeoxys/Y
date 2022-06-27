using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Friends
{
    public class FriendsManager : IFriendsManager
    {
        private readonly ILogger<FriendsManager> _logger;
        private readonly IDatabase _database;
        private readonly IGameWorld _gameWorld;

        private uint _ownerId;

        public FriendsManager(ILogger<FriendsManager> logger, IDatabase database, IGameWorld gameWorld)
        {
            _logger = logger;
            _database = database;
            _gameWorld = gameWorld;
#if DEBUG
            _logger.LogDebug("FriendsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~FriendsManager()
        {
            _logger.LogDebug("FriendsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, IEnumerable<DbCharacter> friends)
        {
            _ownerId = ownerId;

            foreach (var friend in friends)
                Friends.TryAdd(friend.Id, new Friend(friend.Id, friend.Name, friend.Class, _gameWorld.Players.ContainsKey(friend.Id)));

            // Send notification to friends.
            foreach (var friend in Friends.Values)
            {
                _gameWorld.Players.TryGetValue(friend.Id, out var friendPlayer);

                if (friendPlayer != null && friendPlayer.FriendsManager.Friends.ContainsKey(_ownerId))
                {
                    friendPlayer.FriendsManager.Friends[_ownerId].IsOnline = true;
                    friendPlayer.SendFriendOnline(_ownerId, true);
                }
            }
        }

        public Task Clear()
        {
            // Notify friends, that player is offline.
            foreach (var friend in Friends.Values)
            {
                _gameWorld.Players.TryGetValue(friend.Id, out var friendPlayer);

                if (friendPlayer != null && friendPlayer.FriendsManager.Friends.ContainsKey(_ownerId))
                {
                    friendPlayer.FriendsManager.Friends[_ownerId].IsOnline = false;
                    friendPlayer.SendFriendOnline(_ownerId, false);
                }
            }

            return Task.CompletedTask;
        }

        #endregion

        public ConcurrentDictionary<uint, Friend> Friends { get; init; } = new ConcurrentDictionary<uint, Friend>();

        public Character LastRequester { get; set; }

        public async Task<Friend> AddFriend(Character character)
        {
            _database.Friends.Add(new DbCharacterFriend(_ownerId, character.Id));
            await _database.SaveChangesAsync();

            var friend = new Friend(character.Id, character.AdditionalInfoManager.Name, character.AdditionalInfoManager.Class, true);
            Friends.TryAdd(friend.Id, friend);

            return friend;
        }

        public async Task<Friend> DeleteFriend(uint id)
        {
            var dbFriend = _database.Friends.FirstOrDefault(x => x.CharacterId == _ownerId && x.FriendId == id);
            if (dbFriend is not null)
                _database.Friends.Remove(dbFriend);

            await _database.SaveChangesAsync();

            Friends.TryRemove(id, out var friend);
            return friend;
        }
    }
}
