using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Zone;
using System.Collections.Generic;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Game.Guild;
using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Vehicle;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        private void HandleMotion(MotionPacket packet)
        {
            if (packet.Motion == Motion.None || packet.Motion == Motion.Sit)
            {
                Motion = packet.Motion;
            }

            _logger.LogDebug($"Character {Id} sends motion {packet.Motion}");
            OnMotion?.Invoke(this, packet.Motion);
        }

        private void HandleSkillBarPacket(SkillBarPacket skillBarPacket)
        {
            _taskQueue.Enqueue(ActionType.SAVE_QUICK_BAR, Id, skillBarPacket.QuickItems);
        }

        private void HandleNpcBuyItem(int npcId, byte itemIndex, byte count)
        {
            /*var npc = Map.GetNPC(CellId, npcId);
            if (npc is null || !npc.ContainsProduct(itemIndex))
            {
                _logger.LogWarning($"NPC with id {npcId} doesn't contain item at index: {itemIndex}.");
                return;
            }

            if (Map is GuildHouseMap)
            {
                if (!HasGuild)
                {
                    _packetsHelper.SendGuildHouseActionError(Client, GuildHouseActionError.LowRank, 30);
                    return;
                }

                var allowed = _guildManager.CanUseNpc((int)GuildId, npc.Type, npc.TypeId, out var requiredRank);
                if (!allowed)
                {
                    _packetsHelper.SendGuildHouseActionError(Client, GuildHouseActionError.LowRank, requiredRank);
                    return;
                }

                allowed = _guildManager.HasNpcLevel((int)GuildId, npc.Type, npc.TypeId);
                if (!allowed)
                {
                    _packetsHelper.SendGuildHouseActionError(Client, GuildHouseActionError.LowLevel, 0);
                    return;
                }
            }

            var buyItem = npc.Products[itemIndex];
            var boughtItem = BuyItem(buyItem, count);
            if (boughtItem != null)
                _packetsHelper.SendBoughtItem(Client, boughtItem, InventoryManager.Gold);*/
        }
    }
}
