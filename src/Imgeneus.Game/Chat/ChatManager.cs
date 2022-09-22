using Imgeneus.Logs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Imgeneus.World.Game.Chat
{
    public class ChatManager : IChatManager
    {
        private readonly ILogger<IChatManager> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly IGamePacketFactory _packetFactory;
        private readonly ILogsManager _logsManager;

        public ChatManager(ILogger<IChatManager> logger, IGameWorld gameWorld, IGamePacketFactory packetFactory, ILogsManager logsManager)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _packetFactory = packetFactory;
            _logsManager = logsManager;
        }

        public void SendMessage(Character sender, MessageType messageType, string message, string targetName = "")
        {
            if (IsMuted)
                return;

            if (messageType == MessageType.Normal && IsMessageToServer)
            {
                messageType = MessageType.MessageToServer;
                IsMessageToServer = false;
            }

            switch (messageType)
            {
                case MessageType.Normal:
                    var players = sender.Map.Cells[sender.CellId].GetPlayers(sender.PosX, sender.PosZ, 50, CountryType.None, true).Cast<Character>();
                    foreach (var player in players)
                    {
                        _packetFactory.SendNormal(player.GameSession.Client, sender.Id, message, player.GameSession.IsAdmin);
                    }
                    break;

                case MessageType.Whisper:
                    var target = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == targetName);
                    if (target != null && target.Id != sender.Id && target.CountryProvider.Country == sender.CountryProvider.Country)
                    {
                        _packetFactory.SendWhisper(sender.GameSession.Client, sender.AdditionalInfoManager.Name, message, sender.GameSession.IsAdmin);
                        _packetFactory.SendWhisper(target.GameSession.Client, sender.AdditionalInfoManager.Name, message, sender.GameSession.IsAdmin);
                    }
                    break;

                case MessageType.Party:
                    if (sender.PartyManager.Party != null)
                    {
                        foreach (var player in sender.PartyManager.Party.Members.ToList())
                        {
                            _packetFactory.SendParty(player.GameSession.Client, sender.Id, message, sender.GameSession.IsAdmin);
                        }
                    }
                    break;

                case MessageType.Map:
                    var mapPlayers = sender.Map.Players.Where(x => x.Value.CountryProvider.Country == sender.CountryProvider.Country).Select(x => x.Value);
                    foreach (var player in mapPlayers)
                    {
                        _packetFactory.SendMap(player.GameSession.Client, sender.AdditionalInfoManager.Name, message);
                    }
                    break;

                case MessageType.World:
                    if (sender.LevelProvider.Level > 10)
                    {
                        var worldPlayers = _gameWorld.Players.Values.Where(p => p.CountryProvider.Country == sender.CountryProvider.Country);
                        foreach (var player in worldPlayers)
                        {
                            _packetFactory.SendWorld(player.GameSession.Client, sender.AdditionalInfoManager.Name, message);
                        }
                    }
                    break;

                case MessageType.MessageToServer:
                    var allPlayers = _gameWorld.Players.Values;
                    foreach (var player in allPlayers)
                    {
                        _packetFactory.SendMessageToServer(player.GameSession.Client, sender.AdditionalInfoManager.Name, message);
                    }
                    break;

                case MessageType.Guild:
                    if (sender.GuildManager.HasGuild)
                    {
                        foreach (var guildMember in sender.GuildManager.GuildMembers.ToList())
                        {
                            if (!_gameWorld.Players.ContainsKey(guildMember.Id))
                                continue;

                            var player = _gameWorld.Players[guildMember.Id];
                            _packetFactory.SendGuild(player.GameSession.Client, sender.AdditionalInfoManager.Name, message, sender.GameSession.IsAdmin);
                        }
                    }

                    break;

                default:
                    _logger.LogError("Not implemented message type.");
                    break;
            }

            _logsManager.LogChat(sender.Id, messageType.ToString(), message, targetName);
        }

        public bool IsMessageToServer { get; set; }

        public bool IsMuted { get; set; }
    }
}
