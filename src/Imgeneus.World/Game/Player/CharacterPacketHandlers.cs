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
        private void HandlePlayerInTarget(PlayerInTargetPacket packet)
        {
            Target = Map.GetPlayer(packet.TargetId);
        }

        private void HandleMobInTarget(MobInTargetPacket packet)
        {
            Target = Map.GetMob(CellId, packet.TargetId);
        }

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

        private void HandleAutoAttackOnMob(int targetId)
        {
            var target = Map.GetMob(CellId, targetId);
            Attack(255, target);
        }

        private void HandleAutoAttackOnPlayer(int targetId)
        {
            var target = Map.GetPlayer(targetId);
            Attack(255, target);
        }

        private void HandleUseSkillOnMob(byte number, int targetId)
        {
            var target = Map.GetMob(CellId, targetId);
            Attack(number, target);
        }

        private void HandleUseSkillOnPlayer(byte number, int targetId)
        {
            IKillable target = Map.GetPlayer(targetId);
            Attack(number, target);
        }

        private void HandleGetCharacterBuffs(int targetId)
        {
            var target = Map.GetPlayer(targetId);
            if (target != null)
                _packetsHelper.SendCurrentBuffs(Client, target);
        }

        private void HandleGetMobBuffs(int targetId)
        {
            var target = Map.GetMob(CellId, targetId);
            if (target != null)
                _packetsHelper.SendCurrentBuffs(Client, target);
        }

        private void HandleGetMobState(int targetId)
        {
            var target = Map.GetMob(CellId, targetId);
            if (target != null)
            {
                _packetsHelper.SendMobPosition(Client, target.Id, target.MovementManager.PosX, target.MovementManager.PosZ, target.MovementManager.MoveMotion);
                _packetsHelper.SendMobState(Client, target);
            }
            else
                _logger.LogWarning($"Coudn't find mob {targetId} state.");
        }

        private void HandleCharacterShape(int characterId)
        {
            var character = _gameWorld.Players[characterId];
            if (character is null)
            {
                _logger.LogWarning($"Trying to get player {characterId}, that is not presented in game world.");
                return;
            }

            _packetsHelper.SendCharacterShape(Client, character);
        }

        private void HandleFriendRequest(string characterName)
        {
            var character = _gameWorld.Players.FirstOrDefault(p => p.Value.Name == characterName).Value;
            if (character is null || character.CountryProvider.Country != CountryProvider.Country)
                return;

            character.RequestFriendship(this);
        }

        private void HandleSearchParty()
        {
            if (PartyManager.Party != null)
                return;

            Map.RegisterSearchForParty(this);
            _packetsHelper.SendRegisteredInPartySearch(Client, true);

            var searchers = Map.PartySearchers.Where(s => s.CountryProvider.Country == CountryProvider.Country && s != this);
            if (searchers.Any())
                _packetsHelper.SendPartySearchList(Client, searchers.Take(30));
        }

        private void HandleAbsoluteCompose(byte runeBag, byte runeSlot, byte itemBag, byte itemSlot)
        {
            InventoryManager.InventoryItems.TryGetValue((runeBag, runeSlot), out var rune);
            InventoryManager.InventoryItems.TryGetValue((itemBag, itemSlot), out var item);

            if (rune is null || item is null || rune.Special != SpecialEffect.AbsoluteRecreationRune || !item.IsComposable)
            {
                _packetsHelper.SendComposition(Client, true, item);
                return;
            }

            var itemClone = item.Clone();
            LinkingManager.Item = itemClone;
            LinkingManager.Compose(rune);

            _packetsHelper.SendAbsoluteComposition(Client, false, itemClone.GetCraftName());

            // TODO: I'm not sure how absolute composite works and what to do next.

            LinkingManager.Item = null;
        }

        private void HandleItemComposePacket(byte runeBag, byte runeSlot, byte itemBag, byte itemSlot)
        {
            InventoryManager.InventoryItems.TryGetValue((runeBag, runeSlot), out var rune);
            InventoryManager.InventoryItems.TryGetValue((itemBag, itemSlot), out var item);

            if (rune is null || item is null ||
                   (rune.Special != SpecialEffect.RecreationRune &&
                    rune.Special != SpecialEffect.RecreationRune_STR &&
                    rune.Special != SpecialEffect.RecreationRune_DEX &&
                    rune.Special != SpecialEffect.RecreationRune_REC &&
                    rune.Special != SpecialEffect.RecreationRune_INT &&
                    rune.Special != SpecialEffect.RecreationRune_WIS &&
                    rune.Special != SpecialEffect.RecreationRune_LUC) ||
                !item.IsComposable)
            {
                _packetsHelper.SendComposition(Client, true, item);
                return;
            }

            if (item.Bag == 0)
            {
                StatsManager.ExtraStr -= item.ComposedStr;
                StatsManager.ExtraDex -= item.ComposedDex;
                StatsManager.ExtraRec -= item.ComposedRec;
                StatsManager.ExtraInt -= item.ComposedInt;
                StatsManager.ExtraWis -= item.ComposedWis;
                StatsManager.ExtraLuc -= item.ComposedLuc;
                HealthManager.ExtraHP -= item.ComposedHP;
                HealthManager.ExtraMP -= item.ComposedMP;
                HealthManager.ExtraSP -= item.ComposedSP;
            }

            LinkingManager.Item = item;
            LinkingManager.Compose(rune);

            _packetsHelper.SendComposition(Client, false, item);

            if (item.Bag == 0)
            {
                StatsManager.ExtraStr += item.ComposedStr;
                StatsManager.ExtraDex += item.ComposedDex;
                StatsManager.ExtraRec += item.ComposedRec;
                StatsManager.ExtraInt += item.ComposedInt;
                StatsManager.ExtraWis += item.ComposedWis;
                StatsManager.ExtraLuc += item.ComposedLuc;
                HealthManager.ExtraHP += item.ComposedHP;
                HealthManager.ExtraMP += item.ComposedMP;
                HealthManager.ExtraSP += item.ComposedSP;

                //SendAdditionalStats();
            }

            _taskQueue.Enqueue(ActionType.UPDATE_CRAFT_NAME, Id, item.Bag, item.Slot, item.GetCraftName());
            InventoryManager.TryUseItem(rune.Bag, rune.Slot);

            LinkingManager.Item = null;
        }

        /*private void HandleUpdateStats(ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc)
        {
            var fullStat = str + dex + rec + intl + wis + luc;
            if (fullStat > StatsManager.StatPoint || fullStat > ushort.MaxValue)
                return;

            StatsManager.TrySetStats((ushort)(StatsManager.Strength + str),
                                     (ushort)(StatsManager.Dexterity + dex),
                                     (ushort)(StatsManager.Reaction + rec),
                                     (ushort)(StatsManager.Intelligence + intl),
                                     (ushort)(StatsManager.Wisdom + wis),
                                     (ushort)(StatsManager.Luck + luc),
                                     (ushort)(StatsManager.StatPoint - fullStat));


            _taskQueue.Enqueue(ActionType.UPDATE_STATS, Id, StatsManager.Strength, StatsManager.Dexterity, StatsManager.Reaction, StatsManager.Intelligence, StatsManager.Wisdom, StatsManager.Luck, StatsManager.StatPoint);

            _packetsHelper.SendStatsUpdate(Client, str, dex, rec, intl, wis, luc);
            SendAdditionalStats();
        }*/

        private async void HandleGMSetAttributePacket(GMSetAttributePacket gmSetAttributePacket)
        {
            var (attribute, attributeValue, player) = gmSetAttributePacket;

            void SendCommandError()
            {
                _packetsHelper.SendGmCommandError(Client, PacketType.CHARACTER_ATTRIBUTE_SET);
            }

            void SetAttributeAndSendCommandSuccess()
            {
                SendAttribute(attribute);
                _packetsHelper.SendGmCommandSuccess(Client);
            }

            // TODO: This should get player from player dictionary when implemented
            var targetPlayer = _gameWorld.Players.Values.FirstOrDefault(p => p.Name == player);

            if (targetPlayer == null)
            {
                SendCommandError();
                return;
            }

            switch (attribute)
            {
                case CharacterAttributeEnum.Grow:
                    if (await targetPlayer.LevelingManager.TrySetGrow((Mode)attributeValue))
                        SetAttributeAndSendCommandSuccess();
                    else
                        SendCommandError();
                    break;

                case CharacterAttributeEnum.Level:
                    if (targetPlayer.TryChangeLevel((ushort)attributeValue, true))
                        SetAttributeAndSendCommandSuccess();
                    else
                        SendCommandError();
                    break;

                case CharacterAttributeEnum.Money:
                    targetPlayer.InventoryManager.Gold = attributeValue;
                    SetAttributeAndSendCommandSuccess();
                    break;

                case CharacterAttributeEnum.StatPoint:
                    //targetPlayer.StatsManager.TrySetStatPoint((ushort)attributeValue);
                    SetAttributeAndSendCommandSuccess();
                    break;

                case CharacterAttributeEnum.SkillPoint:
                    //targetPlayer.SetSkillPoint((ushort)attributeValue);
                    SetAttributeAndSendCommandSuccess();
                    break;

                case CharacterAttributeEnum.Strength:
                case CharacterAttributeEnum.Dexterity:
                case CharacterAttributeEnum.Reaction:
                case CharacterAttributeEnum.Intelligence:
                case CharacterAttributeEnum.Luck:
                case CharacterAttributeEnum.Wisdom:
                    //targetPlayer.SetStat(attribute, (ushort)attributeValue);
                    SetAttributeAndSendCommandSuccess();
                    break;

                case CharacterAttributeEnum.Hg:
                case CharacterAttributeEnum.Vg:
                case CharacterAttributeEnum.Cg:
                case CharacterAttributeEnum.Og:
                case CharacterAttributeEnum.Ig:
                    SendCommandError();
                    return;

                case CharacterAttributeEnum.Exp:
                    if (targetPlayer.TryChangeExperience((ushort)attributeValue, true))
                        SetAttributeAndSendCommandSuccess();
                    else
                        SendCommandError();
                    break;

                case CharacterAttributeEnum.Kills:
                    targetPlayer.KillsManager.Kills = (ushort)attributeValue;
                    SetAttributeAndSendCommandSuccess();
                    break;

                case CharacterAttributeEnum.Deaths:
                    targetPlayer.KillsManager.Deaths = (ushort)attributeValue;
                    SetAttributeAndSendCommandSuccess();
                    break;

                default:
                    _packetsHelper.SendGmCommandError(Client, PacketType.CHARACTER_ATTRIBUTE_SET);
                    return;
            }
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

        private async void HandleCreateGuild(string name, string message)
        {
            var result = await _guildManager.CanCreateGuild(this, name);
            if (result != GuildCreateFailedReason.Success)
            {
                SendGuildCreateFailed(result);
                return;
            }

            _guildManager.SendGuildRequest(this, name, message);
        }


        private void HandleGuildAgree(bool ok)
        {
            _guildManager.SetAgreeRequest(this, ok);
        }

        private async void HandleGuildJoinRequest(int guildId)
        {
            if (HasGuild)
            {
                _packetsHelper.SendGuildJoinRequest(Client, false);
                return;
            }

            var success = await _guildManager.RequestJoin(guildId, Id);
            _packetsHelper.SendGuildJoinRequest(Client, success);
        }

        private async void HandleJoinResult(bool ok, int characterId)
        {
            if (!HasGuild || GuildRank > 3)
                return;

            var guild = await _guildManager.GetGuild((int)GuildId);
            if (guild is null)
                return;

            await _guildManager.RemoveRequestJoin(characterId);

            var onlinePlayer = _gameWorld.Players[characterId];
            if (!ok)
            {
                if (onlinePlayer != null)
                    onlinePlayer.SendGuildJoinResult(false, guild);

                return;
            }

            var dbCharacter = await _guildManager.TryAddMember((int)GuildId, characterId);
            if (dbCharacter is null)
            {
                if (onlinePlayer != null)
                    onlinePlayer.SendGuildJoinResult(false, guild);

                return;
            }

            // Update guild members.
            foreach (var member in GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];
                guildPlayer.GuildMembers.Add(dbCharacter);
                guildPlayer.SendGuildUserListAdd(dbCharacter, onlinePlayer != null);
            }

            // Send additional info to new member, if he is online.
            if (onlinePlayer != null)
            {
                onlinePlayer.GuildId = guild.Id;
                onlinePlayer.GuildName = guild.Name;
                onlinePlayer.GuildRank = 9;
                onlinePlayer.GuildMembers.AddRange(GuildMembers);

                onlinePlayer.SendGuildJoinResult(true, guild);
                onlinePlayer.SendGuildMembersOnline();
                onlinePlayer.SendGuildNpcLvlList();
            }
        }

        private async void HandleGuildKick(int removeId)
        {
            if (!HasGuild || GuildRank > 3)
            {
                SendGuildKickMember(false, removeId);
                return;
            }

            var dbCharacter = await _guildManager.TryRemoveMember((int)GuildId, removeId);
            if (dbCharacter is null)
            {
                SendGuildKickMember(false, removeId);
                return;
            }

            // Update guild members.
            foreach (var member in GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];

                if (guildPlayer.Id == removeId)
                    guildPlayer.ClearGuild();
                else
                {
                    var temp = guildPlayer.GuildMembers.FirstOrDefault(x => x.Id == removeId);

                    if (temp != null)
                        guildPlayer.GuildMembers.Remove(temp);

                    guildPlayer.SendGuildMemberRemove(removeId);
                }

                guildPlayer.SendGuildKickMember(true, removeId);
            }
        }

        private async void HandleChangeRank(bool demote, int characterId)
        {
            if (!HasGuild || GuildRank > 3)
                return;

            var dbCharacter = await _guildManager.TryChangeRank((int)GuildId, characterId, demote);
            if (dbCharacter is null)
                return;

            foreach (var member in GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];
                var changed = guildPlayer.GuildMembers.FirstOrDefault(x => x.Id == characterId);
                if (changed is null)
                    continue;

                changed.GuildRank = dbCharacter.GuildRank;
                guildPlayer.SendGuildUserChangeRank(changed.Id, changed.GuildRank);
            }
        }

        private void HandleLeaveGuild()
        {
            if (!HasGuild)
                return;

            var dbCharacter = _guildManager.TryRemoveMember((int)GuildId, Id);
            if (dbCharacter == null)
            {
                SendGuildMemberLeaveResult(false);
                return;
            }

            foreach (var member in GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];

                if (guildPlayer.Id == Id)
                {
                    guildPlayer.ClearGuild();
                }
                else
                {
                    var temp = guildPlayer.GuildMembers.FirstOrDefault(x => x.Id == Id);

                    if (temp != null)
                        guildPlayer.GuildMembers.Remove(temp);

                    guildPlayer.SendGuildMemberLeave(Id);
                }
            }

            SendGuildMemberLeaveResult(true);
        }

        private async void HandleGuildDismantle()
        {
            if (!HasGuild || GuildRank != 1)
                return;

            var result = await _guildManager.TryDeleteGuild((int)GuildId);
            if (!result)
                return;

            foreach (var member in GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];
                guildPlayer.ClearGuild();
                guildPlayer.SendGuildDismantle();
            }
        }

        private async void HandleGuildHouseBuy()
        {
            if (!HasGuild)
                return;

            var reason = await _guildManager.TryBuyHouse(this);
            _packetsHelper.SendGuildHouseBuy(Client, reason, InventoryManager.Gold);
        }
        private async void HandleGetEtin()
        {
            var etin = 0;
            if (HasGuild)
            {
                etin = await _guildManager.GetEtin((int)GuildId);
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
            if (!HasGuild || (GuildRank != 1 && GuildRank != 2))
            {
                _packetsHelper.SendGuildUpgradeNpc(Client, GuildNpcUpgradeReason.Failed, npcType, npcGroup, npcLevel);
                return;
            }

            var reason = await _guildManager.TryUpgradeNPC((int)GuildId, npcType, npcGroup, npcLevel);
            if (reason == GuildNpcUpgradeReason.Ok)
            {
                var etin = await _guildManager.GetEtin((int)GuildId);
                _packetsHelper.SendGetEtin(Client, etin);
            }

            _packetsHelper.SendGuildUpgradeNpc(Client, reason, npcType, npcGroup, npcLevel);
        }


        private async void HandleEtinReturn()
        {
            if (!HasGuild)
                return;

            var etins = await _guildManager.ReturnEtin(this);

            _packetsHelper.SendEtinReturnResult(Client, etins);
        }

        private void HandleVehicleRequestPacket(int characterId)
        {
            if (!VehicleManager.IsOnVehicle || VehicleManager.Vehicle2CharacterID != 0)
            {
                SendVehicleResponse(VehicleResponse.Error);
                return;
            }

            var player = Map.GetPlayer(characterId);
            if (player is null || player.VehicleManager.IsOnVehicle || player.CountryProvider.Country != CountryProvider.Country || MathExtensions.Distance(PosX, player.PosX, PosZ, player.PosZ) > 20)
            {
                SendVehicleResponse(VehicleResponse.Error);
                return;
            }

            player.VehicleManager.VehicleRequesterID = Id;
            player.SendVehicleRequest(Id);
        }

        private void HandleVehicleResponsePacket(bool rejected)
        {
            if (VehicleManager.IsOnVehicle)
            {
                return;
            }

            var player = Map.GetPlayer(VehicleManager.VehicleRequesterID);
            if (player is null || !player.VehicleManager.IsOnVehicle || player.VehicleManager.Vehicle2CharacterID != 0 || player.CountryProvider.Country != CountryProvider.Country || MathExtensions.Distance(PosX, player.PosX, PosZ, player.PosZ) > 20)
            {
                return;
            }

            if (rejected)
            {
                player.SendVehicleResponse(VehicleResponse.Rejected);
            }
            else
            {
                VehicleManager.Vehicle2CharacterID = VehicleManager.VehicleRequesterID;
                VehicleManager.VehicleRequesterID = 0;
                //VehicleManager.IsOnVehicle = true;
                player.SendVehicleResponse(VehicleResponse.Accepted);
            }
        }

        private void HandleUseVehicle2Packet()
        {
            if (VehicleManager.Vehicle2CharacterID == 0)
            {
                return;
            }

            VehicleManager.Vehicle2CharacterID = 0;
            //IsOnVehicle = false;
        }
    }
}
