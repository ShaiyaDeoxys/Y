using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class ChatHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IChatManager _chatManager;

        public ChatHandlers(IGamePacketFactory packetFactory, IGameWorld gameWorld, IGameSession gameSession, IChatManager chatManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _chatManager = chatManager;
        }

        [HandlerAction(PacketType.CHAT_NORMAL)]
        public void HandleChatNormal(WorldClient client, ChatNormalPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Normal, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_NORMAL_ADMIN)]
        public void HandleGMChatNormal(WorldClient client, ChatNormalPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Normal, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_WHISPER)]
        public void HandleChatWhisper(WorldClient client, ChatWhisperPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Whisper, packet.Message, packet.TargetName);
        }

        [HandlerAction(PacketType.CHAT_WHISPER_ADMIN)]
        public void HandleGMChatWhisper(WorldClient client, ChatWhisperPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Whisper, packet.Message, packet.TargetName);
        }

        [HandlerAction(PacketType.CHAT_PARTY)]
        public void HandleChatParty(WorldClient client, ChatPartyPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Party, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_PARTY_ADMIN)]
        public void HandleGMChatParty(WorldClient client, ChatPartyPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Party, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_MAP)]
        public void HandleChatMap(WorldClient client, ChatMapPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Map, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_WORLD)]
        public void HandleChatWorld(WorldClient client, ChatWorldPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.World, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_GUILD)]
        public void HandleChatGuild(WorldClient client, ChatGuildPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Guild, packet.Message);
        }

        [HandlerAction(PacketType.CHAT_GUILD_ADMIN)]
        public void HandleChatGMGuild(WorldClient client, ChatGuildPacket packet)
        {
            if (!_gameWorld.Players.TryGetValue(_gameSession.Character.Id, out var sender))
                return;

            _chatManager.SendMessage(sender, MessageType.Guild, packet.Message);
        }
    }
}
