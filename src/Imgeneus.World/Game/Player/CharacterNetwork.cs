using Imgeneus.Database.Constants;
using Imgeneus.Network.Data;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.Network.Server;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        private IWorldClient _client;

        /// <summary>
        /// TCP connection with client.
        /// </summary>
        public IWorldClient Client
        {
            get => _client;

            set
            {
                if (_client is null)
                {
                    _client = value;
                    //_client.OnPacketArrived += Client_OnPacketArrived;
                }
                else
                {
                    throw new ArgumentException("TCP connection can not be set twice");
                }
            }
        }

        /// <summary>
        /// Removes TCP connection.
        /// </summary>
        public void ClearConnection()
        {
            //_client.OnPacketArrived -= Client_OnPacketArrived;
            _client = null;
        }

        /// <summary>
        /// Tries to handle all packets, that client sends.
        /// </summary>
        /// <param name="sender">TCP connection with client</param>
        /// <param name="packet">packet, that clients sends</param>
        /*private void Client_OnPacketArrived(ServerClient sender, IDeserializedPacket packet)
        {
            switch (packet)
            {

                case MobInTargetPacket mobInTargetPacket:
                    HandleMobInTarget(mobInTargetPacket);
                    break;

                case PlayerInTargetPacket playerInTargetPacket:
                    HandlePlayerInTarget(playerInTargetPacket);
                    break;

                case SkillBarPacket skillBarPacket:
                    HandleSkillBarPacket(skillBarPacket);
                    break;

                case AttackStart attackStartPacket:
                    // Not sure, but maybe I should not permit any attack start?
                    sender.SendPacket(new Packet(PacketType.ATTACK_START));
                    break;

                case MobAutoAttackPacket attackPacket:
                    HandleAutoAttackOnMob(attackPacket.TargetId);
                    break;

                case CharacterAutoAttackPacket attackPacket:
                    HandleAutoAttackOnPlayer(attackPacket.TargetId);
                    break;

                case MobSkillAttackPacket usedSkillMobAttackPacket:
                    HandleUseSkillOnMob(usedSkillMobAttackPacket.Number, usedSkillMobAttackPacket.TargetId);
                    break;

                case CharacterSkillAttackPacket usedSkillPlayerAttackPacket:
                    HandleUseSkillOnPlayer(usedSkillPlayerAttackPacket.Number, usedSkillPlayerAttackPacket.TargetId);
                    break;

                case TargetCharacterGetBuffs targetCharacterGetBuffsPacket:
                    HandleGetCharacterBuffs(targetCharacterGetBuffsPacket.TargetId);
                    break;

                case TargetMobGetBuffs targetMobGetBuffsPacket:
                    HandleGetMobBuffs(targetMobGetBuffsPacket.TargetId);
                    break;

                case TargetGetMobStatePacket targetGetMobStatePacket:
                    HandleGetMobState(targetGetMobStatePacket.MobId);
                    break;

                case CharacterShapePacket characterShapePacket:
                    HandleCharacterShape(characterShapePacket.CharacterId);
                    break;

                case CharacterEnteredPortalPacket enterPortalPacket:
                    HandleEnterPortalPacket(enterPortalPacket);
                    break;

                case CharacterTeleportViaNpcPacket teleportViaNpcPacket:
                    HandleTeleportViaNpc(teleportViaNpcPacket);
                    break;

                case UseItemPacket useItemPacket:
                    TryUseItem(useItemPacket.Bag, useItemPacket.Slot);
                    break;

                case UseItem2Packet useItem2Packet:
                    TryUseItem(useItem2Packet.Bag, useItem2Packet.Slot, useItem2Packet.TargetId);
                    break;

                case ChatNormalPacket chatNormalPacket:
                    _chatManager.SendMessage(this, Chat.MessageType.Normal, chatNormalPacket.Message);
                    break;

                case ChatWhisperPacket chatWisperPacket:
                    _chatManager.SendMessage(this, Chat.MessageType.Whisper, chatWisperPacket.Message, chatWisperPacket.TargetName);
                    break;

                case ChatPartyPacket chatPartyPacket:
                    _chatManager.SendMessage(this, Chat.MessageType.Party, chatPartyPacket.Message);
                    break;

                case ChatMapPacket chatMapPacket:
                    _chatManager.SendMessage(this, Chat.MessageType.Map, chatMapPacket.Message);
                    break;

                case ChatWorldPacket chatWorldPacket:
                    _chatManager.SendMessage(this, Chat.MessageType.World, chatWorldPacket.Message);
                    break;

                case ChatGuildPacket chatGuildPacket:
                    _chatManager.SendMessage(this, Chat.MessageType.Guild, chatGuildPacket.Message);
                    break;

                case DuelDefeatPacket duelDefeatPacket:
                    FinishDuel(Duel.DuelCancelReason.AdmitDefeat);
                    break;

                case FriendRequestPacket friendRequestPacket:
                    HandleFriendRequest(friendRequestPacket.CharacterName);
                    break;

                case FriendResponsePacket friendResponsePacket:
                    ClearFriend(friendResponsePacket.Accepted);
                    break;

                case FriendDeletePacket friendDeletePacket:
                    DeleteFriend(friendDeletePacket.CharacterId);
                    break;

                case MapPickUpItemPacket mapPickUpItemPacket:
                    var mapItem = Map.GetItem(mapPickUpItemPacket.ItemId, this);
                    if (mapItem is null)
                    {
                        _packetsHelper.SendItemDoesNotBelong(Client);
                        return;
                    }
                    if (mapItem.Item.Type == Item.MONEY_ITEM_TYPE)
                    {
                        Map.RemoveItem(CellId, mapItem.Id);
                        mapItem.Item.Bag = 1;
                        ChangeGold(Gold + (uint)mapItem.Item.Gem1.TypeId);
                        SendAddItemToInventory(mapItem.Item);
                    }
                    else
                    {
                        var inventoryItem = AddItemToInventory(mapItem.Item);
                        if (inventoryItem is null)
                        {
                            _packetsHelper.SendFullInventory(Client);
                        }
                        else
                        {
                            Map.RemoveItem(CellId, mapItem.Id);
                            SendAddItemToInventory(inventoryItem);
                            if (Party != null)
                                Party.MemberGetItem(this, inventoryItem);
                        }
                    }
                    break;

                case QuestStartPacket questStartPacket:
                    var npcQuestGiver = Map.GetNPC(CellId, questStartPacket.NpcId);
                    if (npcQuestGiver is null || !npcQuestGiver.StartQuestIds.Contains(questStartPacket.QuestId))
                    {
                        _logger.LogWarning($"Trying to start unknown quest {questStartPacket.QuestId} at npc {questStartPacket.NpcId}");
                        return;
                    }

                    var quest = new Quest(_databasePreloader, questStartPacket.QuestId);
                    StartQuest(quest, npcQuestGiver.Id);
                    break;

                case QuestEndPacket questEndPacket:
                    var npcQuestReceiver = Map.GetNPC(CellId, questEndPacket.NpcId);
                    if (npcQuestReceiver is null || !npcQuestReceiver.EndQuestIds.Contains(questEndPacket.QuestId))
                    {
                        _logger.LogWarning($"Trying to finish unknown quest {questEndPacket.QuestId} at npc {questEndPacket.NpcId}");
                        return;
                    }
                    FinishQuest(questEndPacket.QuestId, npcQuestReceiver.Id);
                    break;

                case QuestQuitPacket questQuitPacket:
                    QuitQuest(questQuitPacket.QuestId);
                    break;

                case RebirthPacket rebirthPacket:
                    var rebirthCoordinate = Map.GetRebirthMap(this);
                    Rebirth(rebirthCoordinate.MapId, rebirthCoordinate.X, rebirthCoordinate.Y, rebirthCoordinate.Z);
                    break;

                case PartySearchRegistrationPacket searchPartyPacket:
                    HandleSearchParty();
                    break;

                case VehicleRequestPacket vehicleRequestPacket:
                    HandleVehicleRequestPacket(vehicleRequestPacket.CharacterId);
                    break;

                case VehicleResponsePacket vehicleResponsePacket:
                    HandleVehicleResponsePacket(vehicleResponsePacket.Rejected);
                    break;

                case UseVehicle2Packet useVehicle2Packet:
                    HandleUseVehicle2Packet();
                    break;

                case DyeRerollPacket dyeRerollPacket:
                    HandleDyeReroll();
                    break;

                case DyeConfirmPacket dyeConfirmPacket:
                    HandleDyeConfirm(dyeConfirmPacket.DyeItemBag, dyeConfirmPacket.DyeItemSlot, dyeConfirmPacket.TargetItemBag, dyeConfirmPacket.TargetItemSlot);
                    break;

                case ItemComposeAbsolutePacket itemComposeAbsolutePacket:
                    HandleAbsoluteCompose(itemComposeAbsolutePacket.RuneBag, itemComposeAbsolutePacket.RuneSlot, itemComposeAbsolutePacket.ItemBag, itemComposeAbsolutePacket.ItemSlot);
                    break;

                case ItemComposePacket itemComposePacket:
                    HandleItemComposePacket(itemComposePacket.RuneBag, itemComposePacket.RuneSlot, itemComposePacket.ItemBag, itemComposePacket.ItemSlot);
                    break;

                case UpdateStatsPacket updateStatsPacket:
                    HandleUpdateStats(updateStatsPacket.Str, updateStatsPacket.Dex, updateStatsPacket.Rec, updateStatsPacket.Int, updateStatsPacket.Wis, updateStatsPacket.Luc);
                    break;

                case GuildCreatePacket guildCreatePacket:
                    HandleCreateGuild(guildCreatePacket.Name, guildCreatePacket.Message);
                    break;

                case GuildAgreePacket guildAgreePacket:
                    HandleGuildAgree(guildAgreePacket.Ok);
                    break;

                case GuildJoinRequestPacket guildJoinRequestPacket:
                    HandleGuildJoinRequest(guildJoinRequestPacket.GuildId);
                    break;

                case GuildJoinResultPacket guildJoinResultPacket:
                    HandleJoinResult(guildJoinResultPacket.Ok, guildJoinResultPacket.CharacterId);
                    break;

                case GuildKickPacket guildKickPacket:
                    HandleGuildKick(guildKickPacket.CharacterId);
                    break;

                case GuildUserStatePacket guildUserStatePacket:
                    HandleChangeRank(guildUserStatePacket.Demote, guildUserStatePacket.CharacterId);
                    break;

                case GuildLeavePacket guildLeavePacket:
                    HandleLeaveGuild();
                    break;

                case GuildDismantlePacket guildDismantlePacket:
                    HandleGuildDismantle();
                    break;

                case GuildHouseBuyPacket guildHouseBuyPacket:
                    HandleGuildHouseBuy();
                    break;

                case GuildGetEtinPacket guildGetEtinPacket:
                    HandleGetEtin();
                    break;

                case GuildNpcUpgradePacket guildNpcUpgradePacket:
                    HandleGuildUpgradeNpc(guildNpcUpgradePacket.NpcType, guildNpcUpgradePacket.NpcGroup, guildNpcUpgradePacket.NpcLevel);
                    break;

                case GuildEtinReturnPacket guildEtinReturnPacket:
                    HandleEtinReturn();
                    break;

                case GMNoticeWorldPacket gmNoticeWorldPacket:
                    if (!IsAdmin)
                        return;

                    _noticeManager.SendWorldNotice(gmNoticeWorldPacket.Message, gmNoticeWorldPacket.TimeInterval);
                    _packetsHelper.SendGmCommandSuccess(Client);
                    break;

                case GMNoticePlayerPacket gmNoticePlayerPacket:
                    if (!IsAdmin)
                        return;

                    if (_noticeManager.TrySendPlayerNotice(gmNoticePlayerPacket.Message, gmNoticePlayerPacket.TargetName,
                        gmNoticePlayerPacket.TimeInterval))
                        _packetsHelper.SendGmCommandSuccess(Client);
                    else
                        _packetsHelper.SendGmCommandError(Client, PacketType.NOTICE_PLAYER);
                    break;

                case GMNoticeFactionPacket gmNoticeFactionPacket:
                    if (!IsAdmin)
                        return;

                    _noticeManager.SendFactionNotice(gmNoticeFactionPacket.Message, this.Country, gmNoticeFactionPacket.TimeInterval);
                    _packetsHelper.SendGmCommandSuccess(Client);
                    break;

                case GMNoticeMapPacket gmNoticeMapPacket:
                    if (!IsAdmin)
                        return;

                    _noticeManager.SendMapNotice(gmNoticeMapPacket.Message, this.MapId, gmNoticeMapPacket.TimeInterval);
                    _packetsHelper.SendGmCommandSuccess(Client);
                    break;

                case GMNoticeAdminsPacket gmNoticeAdminsPacket:
                    if (!IsAdmin)
                        return;

                    _noticeManager.SendAdminNotice(gmNoticeAdminsPacket.Message);
                    _packetsHelper.SendGmCommandSuccess(Client);
                    break;

                case GMCurePlayerPacket gmCurePlayerPacket:
                    if (!IsAdmin)
                        return;

                    HandleGMCurePlayerPacket(gmCurePlayerPacket);
                    break;

                case GMWarningPacket gmWarningPacket:
                    if (!IsAdmin)
                        return;

                    HandleGMWarningPlayer(gmWarningPacket);
                    break;

                case GMTeleportPlayerPacket gmTeleportPlayerPacket:
                    if (!IsAdmin)
                        return;

                    HandleGMTeleportPlayer(gmTeleportPlayerPacket);
                    break;

                case BankClaimItemPacket bankClaimItemPacket:
                    var result = TryClaimBankItem(bankClaimItemPacket.Slot, out _);
                    if (!result)
                        _packetsHelper.SendFullInventory(Client);
                    break;
            }
        }*/
    }
}
