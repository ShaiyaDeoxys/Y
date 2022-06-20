using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class FriendHandlers : BaseHandler
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IGameWorld _gameWorld;
        private readonly ICountryProvider _countryProvider;

        public FriendHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IFriendsManager friendsManager, IGameWorld gameWorld, ICountryProvider countryProvider) : base(packetFactory, gameSession)
        {
            _friendsManager = friendsManager;
            _gameWorld = gameWorld;
            _countryProvider = countryProvider;
        }

        [HandlerAction(PacketType.FRIEND_REQUEST)]
        public void HandleFriendRequest(WorldClient client, FriendRequestPacket packet)
        {
            var requester = _gameWorld.Players[_gameSession.Character.Id];
            if (requester is null)
                return;

            var responser = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.CharacterName).Value;
            if (responser is null || responser.CountryProvider.Country != _countryProvider.Country)
                return;

            responser.FriendsManager.LastRequester = requester;
            _packetFactory.SendFriendRequest(responser.GameSession.Client, requester.AdditionalInfoManager.Name);
        }

        [HandlerAction(PacketType.FRIEND_RESPONSE)]
        public async Task HandleFriendResponse(WorldClient client, FriendResponsePacket packet)
        {
            var responser = _gameWorld.Players[_gameSession.Character.Id];
            if (responser is null)
                return;

            var requester = _friendsManager.LastRequester;
            _packetFactory.SendFriendResponse(requester.GameSession.Client, packet.Accepted);

            if (packet.Accepted)
            {
                var friend = await _friendsManager.AddFriend(requester);
                _packetFactory.SendFriendAdded(client, friend);

                friend = await requester.FriendsManager.AddFriend(responser);
                _packetFactory.SendFriendAdded(requester.GameSession.Client, friend);
            }

            _friendsManager.LastRequester = null;
        }

        [HandlerAction(PacketType.FRIEND_DELETE)]
        public async Task HandleFriendDelete(WorldClient client, FriendDeletePacket packet)
        {
            var deleted = await _friendsManager.DeleteFriend(packet.CharacterId);
            if (deleted is not null)
            {
                _packetFactory.SendFriendDeleted(client, deleted.Id);

                _gameWorld.Players.TryGetValue(deleted.Id, out var friendPlayer);
                if (friendPlayer != null)
                {
                    deleted = await friendPlayer.FriendsManager.DeleteFriend(_gameSession.Character.Id);

                    if (deleted is not null)
                        _packetFactory.SendFriendDeleted(friendPlayer.GameSession.Client, deleted.Id);
                }
            }
        }
    }
}
