using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMSetAttributeHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public GMSetAttributeHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.CHARACTER_ATTRIBUTE_SET)]
        public void HandleOriginal(WorldClient client, GMSetAttributePacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            Handle(client, packet, PacketType.CHARACTER_ATTRIBUTE_SET);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_ATTRIBUTE_SET)]
        public void HandleUS(WorldClient client, GMSetAttributePacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            Handle(client, packet, PacketType.GM_SHAIYA_US_ATTRIBUTE_SET);
        }

        private void Handle(WorldClient client, GMSetAttributePacket packet, PacketType packetType)
        {
            var (attribute, attributeValue, player) = packet;

            // TODO: This should get player from player dictionary when implemented
            var targetPlayer = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == player);

            if (targetPlayer is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.CHARACTER_ATTRIBUTE_SET);
                return;
            }

            var ok = false;
            switch (attribute)
            {
                case CharacterAttributeEnum.Grow:
                    targetPlayer.AdditionalInfoManager.Grow = (Mode)attributeValue;
                    ok = true;
                    _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Level:
                    ok = targetPlayer.LevelingManager.TryChangeLevel((ushort)attributeValue);
                    if (ok)
                    {
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, CharacterAttributeEnum.Level, targetPlayer.LevelProvider.Level, packetType);
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, CharacterAttributeEnum.Exp, targetPlayer.LevelingManager.Exp, packetType);
                    }
                    break;

                case CharacterAttributeEnum.Exp:
                    ok = targetPlayer.LevelingManager.TryChangeExperience((ushort)attributeValue);
                    if (ok)
                    {
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, CharacterAttributeEnum.Level, targetPlayer.LevelProvider.Level, packetType);
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, CharacterAttributeEnum.Exp, targetPlayer.LevelingManager.Exp, packetType);
                    }
                    break;

                case CharacterAttributeEnum.Money:
                    targetPlayer.InventoryManager.Gold = attributeValue;
                    ok = true;
                    _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.StatPoint:
                    ok = targetPlayer.StatsManager.TrySetStats(statPoints: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.SkillPoint:
                    ok = targetPlayer.SkillsManager.TrySetSkillPoints((ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Strength:
                    ok = targetPlayer.StatsManager.TrySetStats(str: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Dexterity:
                    ok = targetPlayer.StatsManager.TrySetStats(dex: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Reaction:
                    ok = targetPlayer.StatsManager.TrySetStats(rec: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Intelligence:
                    ok = targetPlayer.StatsManager.TrySetStats(intl: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Luck:
                    ok = targetPlayer.StatsManager.TrySetStats(luc: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Wisdom:
                    ok = targetPlayer.StatsManager.TrySetStats(wis: (ushort)attributeValue);
                    if (ok)
                        _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Kills:
                    targetPlayer.KillsManager.Kills = attributeValue;
                    ok = true;
                    _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Deaths:
                    targetPlayer.KillsManager.Deaths = attributeValue;
                    ok = true;
                    _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue, packetType);
                    break;

                case CharacterAttributeEnum.Hg:
                case CharacterAttributeEnum.Vg:
                case CharacterAttributeEnum.Cg:
                case CharacterAttributeEnum.Og:
                case CharacterAttributeEnum.Ig:
                    _packetFactory.SendGmCommandError(client, PacketType.CHARACTER_ATTRIBUTE_SET);
                    return;

                default:
                    throw new NotImplementedException($"{attribute}");
            }

            if (ok)
            {
                _packetFactory.SendGmCommandSuccess(client);
            }
            else
            {
                _packetFactory.SendGmCommandError(client, PacketType.CHARACTER_ATTRIBUTE_SET);
            }
        }
    }
}
