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

                case CharacterTeleportViaNpcPacket teleportViaNpcPacket:
                    HandleTeleportViaNpc(teleportViaNpcPacket);
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

                case VehicleRequestPacket vehicleRequestPacket:
                    HandleVehicleRequestPacket(vehicleRequestPacket.CharacterId);
                    break;

                case VehicleResponsePacket vehicleResponsePacket:
                    HandleVehicleResponsePacket(vehicleResponsePacket.Rejected);
                    break;

                case UseVehicle2Packet useVehicle2Packet:
                    HandleUseVehicle2Packet();
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

                case GMNoticeMapPacket gmNoticeMapPacket:
                    if (!IsAdmin)
                        return;

                    _noticeManager.SendMapNotice(gmNoticeMapPacket.Message, this.MapId, gmNoticeMapPacket.TimeInterval);
                    _packetsHelper.SendGmCommandSuccess(Client);
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
