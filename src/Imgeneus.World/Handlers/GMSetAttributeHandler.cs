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
        public async Task HandleOriginal(WorldClient client, GMSetAttributePacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            await Handle(client, packet);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_ATTRIBUTE_SET)]
        public async Task HandleUS(WorldClient client, GMSetAttributePacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            await Handle(client, packet);
        }

        private async Task Handle(WorldClient client, GMSetAttributePacket packet)
        {
            var (attribute, attributeValue, player) = packet;

            // TODO: This should get player from player dictionary when implemented
            var targetPlayer = _gameWorld.Players.Values.FirstOrDefault(p => p.Name == player);

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
                    break;

                case CharacterAttributeEnum.Level:
                    ok = targetPlayer.LevelingManager.TryChangeLevel((ushort)attributeValue, true);
                    break;

                case CharacterAttributeEnum.Exp:
                    ok = targetPlayer.LevelingManager.TryChangeExperience((ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Money:
                    targetPlayer.InventoryManager.Gold = attributeValue;
                    ok = true;
                    break;

                case CharacterAttributeEnum.StatPoint:
                    ok = targetPlayer.StatsManager.TrySetStats(statPoints: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.SkillPoint:
                    ok = targetPlayer.SkillsManager.TrySetSkillPoints((ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Strength:
                    ok = targetPlayer.StatsManager.TrySetStats(str: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Dexterity:
                    ok = targetPlayer.StatsManager.TrySetStats(dex: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Reaction:
                    ok = targetPlayer.StatsManager.TrySetStats(rec: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Intelligence:
                    ok = targetPlayer.StatsManager.TrySetStats(intl: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Luck:
                    ok = targetPlayer.StatsManager.TrySetStats(luc: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Wisdom:
                    ok = targetPlayer.StatsManager.TrySetStats(wis: (ushort)attributeValue);
                    break;

                case CharacterAttributeEnum.Kills:
                    targetPlayer.KillsManager.Kills = (ushort)attributeValue;
                    ok = true;
                    break;

                case CharacterAttributeEnum.Deaths:
                    targetPlayer.KillsManager.Deaths = (ushort)attributeValue;
                    ok = true;
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
                _packetFactory.SendAttribute(targetPlayer.GameSession.Client, attribute, attributeValue);
                _packetFactory.SendGmCommandSuccess(client);
            }
            else
            {
                _packetFactory.SendGmCommandError(client, PacketType.CHARACTER_ATTRIBUTE_SET);
            }
        }
    }
}
