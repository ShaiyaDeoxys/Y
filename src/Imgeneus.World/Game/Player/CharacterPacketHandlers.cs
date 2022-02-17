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

        private void HandleTeleportViaNpc(CharacterTeleportViaNpcPacket teleportViaNpcPacket)
        {
            var npc = Map.GetNPC(CellId, teleportViaNpcPacket.NpcId);
            if (npc is null)
            {
                _logger.LogWarning($"Character {Id} is trying to get non-existing npc via teleport packet.");
                return;
            }

            if (!npc.ContainsGate(teleportViaNpcPacket.GateId))
            {
                _logger.LogWarning($"NPC type {npc.Type} type id {npc.TypeId} doesn't contain teleport gate {teleportViaNpcPacket.GateId}. Check it out!");
                return;
            }

            if (Map is GuildHouseMap)
            {
                if (!GuildManager.HasGuild)
                {
                    _packetsHelper.SendGuildHouseActionError(Client, GuildHouseActionError.LowRank, 30);
                    return;
                }

                var allowed = GuildManager.CanUseNpc(GuildManager.GuildId, npc.Type, npc.TypeId, out var requiredRank);
                if (!allowed)
                {
                    _packetsHelper.SendGuildHouseActionError(Client, GuildHouseActionError.LowRank, requiredRank);
                    return;
                }

                allowed = GuildManager.HasNpcLevel(GuildManager.GuildId, npc.Type, npc.TypeId);
                if (!allowed)
                {
                    _packetsHelper.SendGuildHouseActionError(Client, GuildHouseActionError.LowLevel, 0);
                    return;
                }
            }

            var gate = npc.Gates[teleportViaNpcPacket.GateId];

            if (InventoryManager.Gold < gate.Cost)
            {
                SendTeleportViaNpc(NpcTeleportNotAllowedReason.NotEnoughMoney);
                return;
            }

            var mapConfig = _mapLoader.LoadMapConfiguration(gate.MapId);
            if (mapConfig is null)
            {
                SendTeleportViaNpc(NpcTeleportNotAllowedReason.MapCapacityIsFull);
                return;
            }

            // TODO: there should be somewhere player's level check. But I can not find it in gate config.

            InventoryManager.Gold = (uint)(InventoryManager.Gold - gate.Cost);
            SendTeleportViaNpc(NpcTeleportNotAllowedReason.Success);
            TeleportationManager.Teleport(gate.MapId, gate.X, gate.Y, gate.Z);
        }

        private async void HandleGuildHouseBuy()
        {
            if (!GuildManager.HasGuild)
                return;

            var reason = await GuildManager.TryBuyHouse(this);
            _packetsHelper.SendGuildHouseBuy(Client, reason, InventoryManager.Gold);
        }
        private async void HandleGetEtin()
        {
            var etin = 0;
            if (GuildManager.HasGuild)
            {
                etin = await GuildManager.GetEtin(GuildManager.GuildId);
            }

            _packetsHelper.SendGetEtin(Client, etin);
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

        private async void HandleGuildUpgradeNpc(byte npcType, byte npcGroup, byte npcLevel)
        {
            if (!GuildManager.HasGuild || (GuildManager.GuildRank != 1 && GuildManager.GuildRank != 2))
            {
                _packetsHelper.SendGuildUpgradeNpc(Client, GuildNpcUpgradeReason.Failed, npcType, npcGroup, npcLevel);
                return;
            }

            var reason = await GuildManager.TryUpgradeNPC(GuildManager.GuildId, npcType, npcGroup, npcLevel);
            if (reason == GuildNpcUpgradeReason.Ok)
            {
                var etin = await GuildManager.GetEtin(GuildManager.GuildId);
                _packetsHelper.SendGetEtin(Client, etin);
            }

            _packetsHelper.SendGuildUpgradeNpc(Client, reason, npcType, npcGroup, npcLevel);
        }


        private async void HandleEtinReturn()
        {
            if (!GuildManager.HasGuild)
                return;

            var etins = await GuildManager.ReturnEtin(this);

            _packetsHelper.SendEtinReturnResult(Client, etins);
        }
    }
}
