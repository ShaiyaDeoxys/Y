using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Data;
using Imgeneus.Network.PacketProcessor;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Serialization;

#if EP8_V1
using Imgeneus.World.Serialization.EP_8_V1;
#elif EP8_V2
using Imgeneus.World.Serialization.EP_8_V2;
#else
using Imgeneus.World.Serialization.SHAIYA_US;
#endif

namespace Imgeneus.World.Packets
{
    /// <summary>
    /// Helps to creates packets and send them to players.
    /// </summary>
    internal class PacketsHelper
    {
        internal void SendInventoryItems(IWorldClient client, Item[] inventoryItems)
        {
            var steps = inventoryItems.Length / 50;
            var left = inventoryItems.Length % 50;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 50;
                var length = i == steps ? left : 50;
                var endIndex = startIndex + length;

                using var packet = new ImgeneusPacket(PacketType.CHARACTER_ITEMS);
                packet.Write(new InventoryItems(inventoryItems[startIndex..endIndex]).Serialize());
                client.Send(packet);
            }
        }

        internal void SendGuildList(IWorldClient client, DbGuild[] guilds)
        {
            using var start = new ImgeneusPacket(PacketType.GUILD_LIST_LOADING_START);
            client.Send(start);

            var steps = guilds.Length / 15;
            var left = guilds.Length % 15;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 15;
                var length = i == steps ? left : 15;
                var endIndex = startIndex + length;

                using var packet = new ImgeneusPacket(PacketType.GUILD_LIST);
                packet.Write(new GuildList(guilds[startIndex..endIndex]).Serialize());
                client.Send(packet);
            }

            using var end = new ImgeneusPacket(PacketType.GUILD_LIST_LOADING_END);
            client.Send(end);
        }

        internal void SendGuildMembersOnline(IWorldClient client, List<DbCharacter> members, bool online)
        {
            var steps = members.Count / 40;
            var left = members.Count % 40;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 40;
                var length = i == steps ? left : 40;
                var endIndex = startIndex + length;

                ImgeneusPacket packet;
                if (online)
                    packet = new ImgeneusPacket(PacketType.GUILD_USER_LIST_ONLINE);
                else
                    packet = new ImgeneusPacket(PacketType.GUILD_USER_LIST_NOT_ONLINE);

                packet.Write(new GuildListOnline(members.GetRange(startIndex, endIndex)).Serialize());
                client.Send(packet);
                packet.Dispose();
            }
        }

        internal void SendWorldDay(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.WORLD_DAY);
            packet.Write(new DateTime(2020, 01, 01, 12, 30, 00).ToShaiyaTime());
            client.Send(packet);
        }

        internal void SendCharacterTeleport(IWorldClient client, Character player, bool teleportedByAdmin)
        {
            using var packet = new ImgeneusPacket(teleportedByAdmin ? PacketType.CHARACTER_MAP_TELEPORT : PacketType.GM_TELEPORT_MAP_COORDINATES);
            packet.Write(player.Id);
            packet.Write(player.MapId);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }

        internal void SendGuildNpcs(IWorldClient client, IEnumerable<DbGuildNpcLvl> npcs)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_NPC_LIST);
            packet.Write(new GuildNpcList(npcs).Serialize());
            client.Send(packet);
        }

        internal void SendCurrentHitpoints(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_CURRENT_HITPOINTS);
            packet.Write(new CharacterHitpoints(character).Serialize());
            client.Send(packet);
        }

        internal void SendDetails(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_DETAILS);
            packet.Write(new CharacterDetails(character).Serialize());
            client.Send(packet);
        }

        internal void SendLearnedSkills(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILLS);
            packet.Write(new CharacterSkills(character).Serialize());
            client.Send(packet);
        }

        internal void SendAddBuff(IWorldClient client, Buff buff)
        {
            using var packet = new ImgeneusPacket(PacketType.BUFF_ADD);
            packet.Write(new SerializedActiveBuff(buff).Serialize());
            client.Send(packet);
        }

        internal void SendRemoveBuff(IWorldClient client, Buff buff)
        {
            using var packet = new ImgeneusPacket(PacketType.BUFF_REMOVE);
            packet.Write(buff.Id);
            client.Send(packet);
        }

        internal void SendMoveItemInInventory(IWorldClient client, Item sourceItem, Item destinationItem)
        {
            // Send move item.
            using var packet = new ImgeneusPacket(PacketType.INVENTORY_MOVE_ITEM);

#if (EP8_V2 || SHAIYA_US)
            packet.Write(0); // Unknown int in V2.
#endif

            var bytes = new MovedItem(sourceItem).Serialize();
            packet.Write(bytes);

            bytes = new MovedItem(destinationItem).Serialize();
            packet.Write(bytes);

            client.Send(packet);
        }

        internal void SetMobInTarget(IWorldClient client, Mob target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_SELECT_MOB);
            packet.Write(new MobInTarget(target).Serialize());
            client.Send(packet);
        }

        internal void SetPlayerInTarget(IWorldClient client, Character target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_SELECT_CHARACTER);
            packet.Write(new CharacterInTarget(target).Serialize());
            client.Send(packet);
        }

        internal void SendVehicleRequest(IWorldClient client, int requesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.VEHICLE_REQUEST);
            packet.Write(requesterId);
            client.Send(packet);
        }

        internal void SendVehicleResponse(IWorldClient client, VehicleResponse status)
        {
            using var packet = new ImgeneusPacket(PacketType.VEHICLE_RESPONSE);
            packet.Write((byte)status);
            client.Send(packet);
        }

        internal void SendSkillBar(IWorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILL_BAR);
            packet.Write((byte)quickItems.Count());
            packet.Write(0); // Unknown int.

            foreach (var item in quickItems)
            {
                packet.Write(item.Bar);
                packet.Write(item.Slot);
                packet.Write(item.Bag);
                packet.Write(item.Number);
                packet.Write(0); // Unknown int.
            }

            client.Send(packet);
        }

        internal void SendAddItem(IWorldClient client, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ADD_ITEM);
            packet.Write(new AddedInventoryItem(item).Serialize());
            client.Send(packet);
        }

        internal void SendPortalTeleportNotAllowed(IWorldClient client, PortalTeleportNotAllowedReason reason)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ENTERED_PORTAL);
            packet.Write(false); // success
            packet.Write((byte)reason);
            client.Send(packet);
        }

        internal void SendRemoveItem(IWorldClient client, Item item, bool fullRemove)
        {
            using var packet = new ImgeneusPacket(PacketType.REMOVE_ITEM);
            packet.Write(new RemovedInventoryItem(item, fullRemove).Serialize());
            client.Send(packet);
        }

        internal void SendGuildMemberIsOnline(IWorldClient client, int playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(104);
            packet.Write(playerId);
            client.Send(packet);
        }

        internal void SendGuildJoinRequestAdd(IWorldClient client, DbCharacter character)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_LIST_ADD);
            packet.Write(new GuildJoinUserUnit(character).Serialize());
            client.Send(packet);
        }

        internal void SendGuildJoinRequestRemove(IWorldClient client, int playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_LIST_REMOVE);
            packet.Write(playerId);
            client.Send(packet);
        }

        internal void SendGuildMemberIsOffline(IWorldClient client, int playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(105);
            packet.Write(playerId);
            client.Send(packet);
        }

        internal void SendGuildMemberRemove(IWorldClient client, int playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(103);
            packet.Write(playerId);
            client.Send(packet);
        }

        internal void SendGuildMemberLeave(IWorldClient client, int playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(102);
            packet.Write(playerId);
            client.Send(packet);
        }

        internal void SendGuildDismantle(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(101);
            packet.Write(0);
            client.Send(packet);
        }

        internal void SendGuildListAdd(IWorldClient client, DbGuild guild)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_LIST_ADD);
            packet.Write(new GuildUnit(guild).Serialize());
            client.Send(packet);
        }

        internal void SendGRBNotice(IWorldClient client, GRBNotice notice)
        {
            using var packet = new ImgeneusPacket(PacketType.GRB_NOTICE);
            packet.Write((ushort)50); // GRB map is always 50. Technically this doesn't really matter, because it's not used anywhere...
            packet.Write((byte)notice);
            client.Send(packet);
        }

        internal void SendGuildRanksCalculated(IWorldClient client, IEnumerable<(int GuildId, int Points, byte Rank)> results)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_RANK_UPDATE);
            packet.Write(new GuildRankUpdate(results).Serialize());
            client.Send(packet);
        }

        internal void SendGBRPoints(IWorldClient client, int currentPoints, int maxPoints, int topGuild)
        {
            using var packet = new ImgeneusPacket(PacketType.GRB_POINTS);
            packet.Write(currentPoints);
            packet.Write(maxPoints);
            packet.Write(topGuild);
            client.Send(packet);
        }

        internal void SendGuildListRemove(IWorldClient client, int guildId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_LIST_REMOVE);
            packet.Write(guildId);
            client.Send(packet);
        }

        internal void SendGuildMemberLeaveResult(IWorldClient client, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_LEAVE);
            packet.Write(ok);
            client.Send(packet);
        }

        internal void SendGuildUserChangeRank(IWorldClient client, int playerId, byte rank)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte((byte)(rank + 200));
            packet.Write(playerId);
            client.Send(packet);
        }

        internal void SendGuildKickMember(IWorldClient client, bool ok, int characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_KICK);
            packet.Write(ok);
            packet.Write(characterId);
            client.Send(packet);
        }

        internal void SendGuildUserListAdd(IWorldClient client, DbCharacter character, bool online)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_LIST_ADD);
            packet.Write(online);
            packet.Write(new GuildUserUnit(character).Serialize());
            client.Send(packet);
        }

        internal void SendTeleportViaNpc(IWorldClient client, NpcTeleportNotAllowedReason reason, uint money)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_TELEPORT_VIA_NPC);
            packet.Write((byte)reason);
            packet.Write(money);
            client.Send(packet);
        }

        internal void SendItemExpiration(IWorldClient client, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_EXPIRATION);
            packet.Write(new InventoryItemExpiration(item).Serialize());
            client.Send(packet);
        }

        internal void SendAdditionalStats(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ADDITIONAL_STATS);
            packet.Write(new CharacterAdditionalStats(character).Serialize());
            client.Send(packet);
        }

        internal void SendAutoStats(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.AUTO_STATS_LIST);
            packet.Write(character.AutoStr);
            packet.Write(character.AutoDex);
            packet.Write(character.AutoRec);
            packet.Write(character.AutoInt);
            packet.Write(character.AutoWis);
            packet.Write(character.AutoLuc);
            client.Send(packet);
        }

        internal void SendGuildCreateRequest(IWorldClient client, int creatorId, string guildName, string guildMessage)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_CREATE_AGREE);
            packet.Write(creatorId);
            packet.WriteString(guildName, 25);
            packet.WriteString(guildMessage, 65);
            client.Send(packet);
        }

        internal void SendGoldUpdate(IWorldClient client, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.SET_MONEY);
            packet.Write(gold);
            client.Send(packet);
        }

        internal void SendPartyInfo(IWorldClient client, IEnumerable<Character> partyMembers, byte leaderIndex)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_LIST);
            packet.Write(new UsualParty(partyMembers, leaderIndex).Serialize());
            client.Send(packet);
        }

        internal void SendRaidInfo(IWorldClient client, Raid raid)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_LIST);
            packet.Write(new RaidParty(raid).Serialize());
            client.Send(packet);
        }

        internal void SendResetSkills(IWorldClient client, ushort skillPoint)
        {
            using var packet = new ImgeneusPacket(PacketType.RESET_SKILLS);
            packet.Write(true); // is success?
            packet.Write(skillPoint);
            client.Send(packet);
        }

        internal void SendAttackStart(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.ATTACK_START);
            client.Send(packet);
        }

        internal void SendAutoAttackWrongTarget(IWorldClient client, Character sender, IKillable target)
        {
            PacketType type = target is Character ? PacketType.CHARACTER_CHARACTER_AUTO_ATTACK : PacketType.CHARACTER_MOB_AUTO_ATTACK;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new UsualAttack(sender.Id, 0, new AttackResult() { Success = AttackSuccess.WrongTarget }).Serialize());
            client.Send(packet);
        }

        internal void SendAutoAttackWrongEquipment(IWorldClient client, Character sender, IKillable target)
        {
            PacketType type = target is Character ? PacketType.CHARACTER_CHARACTER_AUTO_ATTACK : PacketType.CHARACTER_MOB_AUTO_ATTACK;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new UsualAttack(sender.Id, 0, new AttackResult() { Success = AttackSuccess.WrongEquipment }).Serialize());
            client.Send(packet);
        }

        internal void SendAutoAttackCanNotAttack(IWorldClient client, Character sender, IKillable target)
        {
            PacketType type = target is Character ? PacketType.CHARACTER_CHARACTER_AUTO_ATTACK : PacketType.CHARACTER_MOB_AUTO_ATTACK;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new UsualAttack(sender.Id, 0, new AttackResult() { Success = AttackSuccess.CanNotAttack }).Serialize());
            client.Send(packet);
        }

        internal void SendItemDoesNotBelong(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.ADD_ITEM);
            packet.WriteByte(0);
            packet.WriteByte(0); // Item doesn't belong to player.
            client.Send(packet);
        }

        internal void SendFullInventory(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.ADD_ITEM);
            packet.WriteByte(0);
            packet.WriteByte(1); // Inventory is full.
            client.Send(packet);
        }

        internal void SendBoughtItem(IWorldClient client, Item boughtItem, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.NPC_BUY_ITEM);
            packet.WriteByte(0); // success
            packet.Write(boughtItem.Bag);
            packet.Write(boughtItem.Slot);
            packet.Write(boughtItem.Type);
            packet.Write(boughtItem.TypeId);
            packet.Write(boughtItem.Count);
            packet.Write(gold);
            client.Send(packet);
        }

        internal void SendSoldItem(IWorldClient client, Item soldItem, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.NPC_SELL_ITEM);
            packet.WriteByte(0); // success
            packet.Write(soldItem.Bag);
            packet.Write(soldItem.Slot);
            packet.Write(soldItem.Type);
            packet.Write(soldItem.TypeId);
            packet.Write(soldItem.Count);
            packet.Write(gold);
            client.Send(packet);
        }

        internal void VehiclePassengerChanged(IWorldClient client, int passengerId, int vehicleCharId)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE_2);
            packet.Write(passengerId);
            packet.Write(vehicleCharId);
            client.Send(packet);
        }

        internal void SendBuyItemIssue(IWorldClient client, byte issue)
        {
            using var packet = new ImgeneusPacket(PacketType.NPC_BUY_ITEM);
            packet.Write(issue);
            // empty fields about item, because it wasn't bought.
            packet.WriteByte(0); // bag
            packet.WriteByte(0); // slot
            packet.WriteByte(0); // type
            packet.WriteByte(0); // type id
            packet.WriteByte(0); // count
            packet.Write(0); // gold
            client.Send(packet);
        }

        internal void SendSkillWrongTarget(IWorldClient client, Character sender, Skill skill, IKillable target)
        {
            PacketType type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(sender.Id, 0, skill, new AttackResult() { Success = AttackSuccess.WrongTarget }).Serialize());
            client.Send(packet);
        }

        internal void SendSkillAttackCanNotAttack(IWorldClient client, Character sender, Skill skill, IKillable target)
        {
            PacketType type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(sender.Id, 0, skill, new AttackResult() { Success = AttackSuccess.CanNotAttack }).Serialize());
            client.Send(packet);
        }

        internal void SendGmCommandSuccess(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_CMD_ERROR);
            packet.Write<ushort>(0); // 0 == no error
            client.Send(packet);
        }

        internal void SendGmCommandError(IWorldClient client, PacketType error)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_CMD_ERROR);
            packet.Write((ushort)error);
            client.Send(packet);
        }

        internal void SendSkillWrongEquipment(IWorldClient client, Character sender, IKillable target, Skill skill)
        {
            PacketType type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(sender.Id, 0, skill, new AttackResult() { Success = AttackSuccess.WrongEquipment }).Serialize());
            client.Send(packet);
        }

        internal void SendNotEnoughMPSP(IWorldClient client, Character sender, IKillable target, Skill skill)
        {
            PacketType type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(sender.Id, 0, skill, new AttackResult() { Success = AttackSuccess.NotEnoughMPSP }).Serialize());
            client.Send(packet);
        }

        internal void SendAbsorbValue(IWorldClient client, ushort absorb)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ABSORPTION_DAMAGE);
            packet.Write(absorb);
            client.Send(packet);
        }

        internal void SendCooldownNotOver(IWorldClient client, Character sender, IKillable target, Skill skill)
        {
            PacketType type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(sender.Id, 0, skill, new AttackResult() { Success = AttackSuccess.NotEnoughMPSP }).Serialize());
            client.Send(packet);
        }

        internal void SendUseSMMP(IWorldClient client, ushort MP, ushort SP)
        {
            using var packet = new ImgeneusPacket(PacketType.USED_SP_MP);
            packet.Write(new UseSPMP(SP, MP).Serialize());
            client.Send(packet);
        }

        internal void SendResetStats(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.STATS_RESET);
            packet.Write(true); // success
            packet.Write(character.StatsManager.StatPoint);
            packet.Write(character.StatsManager.Strength);
            packet.Write(character.StatsManager.Reaction);
            packet.Write(character.StatsManager.Intelligence);
            packet.Write(character.StatsManager.Wisdom);
            packet.Write(character.StatsManager.Dexterity);
            packet.Write(character.StatsManager.Luck);
            client.Send(packet);
        }

        internal void SendCurrentBuffs(IWorldClient client, IKillable target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_BUFFS);
            packet.Write(new TargetBuffs(target).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterShape(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE);
            packet.Write(new CharacterShape(character).Serialize());
            client.Send(packet);
        }

        internal void SendRunMode(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.RUN_MODE);
            packet.Write(character.MovementManager.MoveMotion);
            client.Send(packet);
        }

        internal void SendTargetAddBuff(IWorldClient client, int targetId, Buff buff, bool isMob)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_BUFF_ADD);
            if (isMob)
            {
                packet.WriteByte(2);
            }
            else
            {
                packet.WriteByte(1);
            }
            packet.Write(targetId);
            packet.Write(buff.SkillId);
            packet.Write(buff.SkillLevel);

            client.Send(packet);
        }

        internal void SendTargetRemoveBuff(IWorldClient client, int targetId, Buff buff, bool isMob)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_BUFF_REMOVE);
            if (isMob)
            {
                packet.WriteByte(2);
            }
            else
            {
                packet.WriteByte(1);
            }
            packet.Write(targetId);
            packet.Write(buff.SkillId);
            packet.Write(buff.SkillLevel);

            client.Send(packet);
        }

        internal void SendMobPosition(IWorldClient client, int senderId, float x, float z, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_MOVE);
            packet.Write(new MobMove(senderId, x, z, motion).Serialize());
            client.Send(packet);
        }

        internal void SendMobState(IWorldClient client, Mob target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_MOB_GET_STATE);
            packet.Write(target.Id);
            packet.Write(target.HealthManager.CurrentHP);
            packet.Write((byte)target.SpeedManager.TotalAttackSpeed);
            packet.Write((byte)target.SpeedManager.TotalMoveSpeed);
            client.Send(packet);
        }

        internal void SendQuests(IWorldClient client, IEnumerable<Quest> quests)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_LIST);
            packet.Write(new CharacterQuests(quests).Serialize());
            client.Send(packet);
        }

        internal void SendFinishedQuests(IWorldClient client, IEnumerable<Quest> quests)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_FINISHED_LIST);
            packet.Write(new CharacterFinishedQuests(quests).Serialize());
            client.Send(packet);
        }

        internal void SendQuestStarted(IWorldClient client, ushort questId, int npcId)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_START);
            packet.Write(npcId);
            packet.Write(questId);
            client.Send(packet);
        }

        internal void SendQuestFinished(IWorldClient client, Quest quest, int npcId)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_END);
            packet.Write(npcId);
            packet.Write(quest.Id);
            packet.Write(quest.IsSuccessful);
            packet.WriteByte(0); // ResultType
            packet.Write(quest.IsSuccessful ? quest.XP : 0);
            packet.Write(quest.IsSuccessful ? quest.Gold : 0);
            packet.WriteByte(0); // bag
            packet.WriteByte(0); // slot
            packet.WriteByte(0); // item type
            packet.WriteByte(0); // item id
            client.Send(packet);
        }

        internal void SendQuestCountUpdate(IWorldClient client, ushort questId, byte index, byte count)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_UPDATE_COUNT);
            packet.Write(questId);
            packet.Write(index);
            packet.Write(count);
            client.Send(packet);
        }

        internal void SendFriendRequest(IWorldClient client, Character requester)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_REQUEST);
            packet.WriteString(requester.Name, 21);
            client.Send(packet);
        }

        internal void SendFriendAdded(IWorldClient client, Character friend)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_ADD);
            packet.Write(friend.Id);
            packet.Write((byte)friend.Class);
            packet.WriteString(friend.Name);
            client.Send(packet);
        }

        internal void SendFriendDelete(IWorldClient client, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_DELETE);
            packet.Write(id);
            client.Send(packet);
        }

        internal void SendFriends(IWorldClient client, IEnumerable<Friend> friends)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_LIST);
            packet.Write(new FriendsList(friends).Serialize());
            client.Send(packet);
        }

        internal void SendFriendOnline(IWorldClient client, int id, bool isOnline)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_ONLINE);
            packet.Write(id);
            packet.Write(isOnline);
            client.Send(packet);
        }

        internal void SendFriendResponse(IWorldClient client, bool accepted)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_RESPONSE);
            packet.Write(accepted);
            client.Send(packet);
        }

        internal void SendGuildJoinRequest(IWorldClient client, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_REQUEST);
            packet.Write(ok);
            client.Send(packet);
        }

        internal void SendGuildJoinResult(IWorldClient client, bool ok, DbGuild guild, byte rank = 9)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_RESULT_USER);
            packet.Write(ok);
            packet.Write(guild.Id);
            packet.Write(rank);
            packet.WriteString(guild.Name, 25);
            client.Send(packet);
        }

        internal void SendWeather(IWorldClient client, Map map)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_WEATHER);
            packet.Write(new MapWeather(map).Serialize());
            client.Send(packet);
        }

        internal void SendObelisks(IWorldClient client, IEnumerable<Obelisk> obelisks)
        {
            using var packet = new ImgeneusPacket(PacketType.OBELISK_LIST);
            packet.Write(new ObeliskList(obelisks).Serialize());
            client.Send(packet);
        }

        internal void SendObeliskBroken(IWorldClient client, Obelisk obelisk)
        {
            using var packet = new ImgeneusPacket(PacketType.OBELISK_CHANGE);
            packet.Write(obelisk.Id);
            packet.Write((byte)obelisk.ObeliskCountry);
            client.Send(packet);
        }

        internal void SendRegisteredInPartySearch(IWorldClient client, bool isSuccess)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_SEARCH_REGISTRATION);
            packet.Write(isSuccess);
            client.Send(packet);
        }

        internal void SendPartySearchList(IWorldClient client, IEnumerable<Character> partySearchers)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_SEARCH_LIST);
            packet.Write(new PartySearchList(partySearchers).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterPosition(IWorldClient client, Character player)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_FIND_PLAYER);
            packet.Write(player.MapId);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }

        internal void SendGetEtin(IWorldClient client, int etin)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_GET_ETIN);
            packet.Write(etin);
            client.Send(packet);
        }

        internal void SendGmSummon(IWorldClient client, Character player)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_SUMMON_PLAYER);
            packet.Write(player.Id);
            packet.Write(player.MapId);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }

        internal void SendGuildHouseBuy(IWorldClient client, GuildHouseBuyReason reason, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_HOUSE_BUY);
            packet.Write((byte)reason);
            packet.Write(gold);
            client.Send(packet);
        }

        internal void SendGuildHouseActionError(IWorldClient client, GuildHouseActionError error, byte rank)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_HOUSE_ACTION_ERR);
            packet.Write((byte)error);
            packet.Write(rank);
            client.Send(packet);
        }

        internal void SendGmTeleportToPlayer(IWorldClient client, Character player)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_TELEPORT_TO_PLAYER);
            packet.Write(player.Id);
            packet.Write(player.MapId);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }

        internal void SendUseVehicle(IWorldClient client, bool success, bool status)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE);
            packet.Write(success);
            packet.Write(status);
            client.Send(packet);
        }
        internal void SendGuildCreateFailed(IWorldClient client, GuildCreateFailedReason reason)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_CREATE);
            packet.Write((byte)reason);
            client.Send(packet);
        }

        internal void SendGuildCreateSuccess(IWorldClient client, int guildId, byte rank, string guildName, string guildMessage)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_CREATE);
            packet.Write((byte)GuildCreateFailedReason.Success);
            packet.Write(guildId);
            packet.WriteByte(rank);
            packet.WriteString(guildName, 25);
            packet.WriteString(guildMessage, 65);
            client.Send(packet);
        }

        internal void SendAddGem(IWorldClient client, bool success, Item gem, Item destinationItem, byte gemSlot, uint gold, Item saveItem, Item hammer)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_ADD);
            packet.Write(success);
            packet.Write(gem.Bag);
            packet.Write(gem.Slot);
            packet.Write(gem.Count);
            packet.Write(destinationItem.Bag);
            packet.Write(destinationItem.Slot);
            packet.Write(gemSlot);
            packet.Write(gem.TypeId);
            packet.WriteByte(0); // unknown, old eps: byBag
            packet.WriteByte(0); // unknown, old eps: bySlot
            packet.WriteByte(0); // unknown, old eps: byTypeID; maybe in new ep TypeId is int?
            packet.Write(gold);
            if (hammer is null)
            {
                packet.WriteByte(0);
                packet.WriteByte(0);
            }
            else
            {
                packet.Write(hammer.Bag);
                packet.Write(hammer.Slot);
            }

            client.Send(packet);
        }

        internal void SendEtinReturnResult(IWorldClient client, IList<Item> etins)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_ETIN_RETURN);
            packet.WriteByte((byte)etins.Count);
            foreach (var etin in etins)
            {
                packet.WriteByte(etin.Bag);
                packet.WriteByte(etin.Slot);
            }
            client.Send(packet);
        }

        internal void SendGuildUpgradeNpc(IWorldClient client, GuildNpcUpgradeReason reason, byte npcType, byte npcGroup, byte npcLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_NPC_UPGRADE);
            packet.Write((byte)reason);
            packet.Write(npcType);
            packet.Write(npcGroup);
            packet.Write(npcLevel);
            packet.WriteByte(0); // TODO: number? what is it?!
            client.Send(packet);
        }

        //internal void SendGemPossibility(IWorldClient client, double rate, int gold)
        //{
        //    using var packet = new ImgeneusPacket(PacketType.GEM_ADD_POSSIBILITY);
        //    packet.WriteByte(1); // TODO: unknown, maybe bool, that we can link?
        //    packet.Write(rate);
        //    packet.Write(gold);
        //    client.Send(packet);
        //}

        internal void SendGemRemovePossibility(IWorldClient client, double rate, int gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_REMOVE_POSSIBILITY);
            packet.WriteByte(1); // TODO: unknown, maybe bool, that we can link?
            packet.Write(rate);
            packet.Write(gold);
            client.Send(packet);
        }

        internal void SendRemoveGem(IWorldClient client, bool success, Item item, byte gemPosition, List<Item> savedGems, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_REMOVE);
            packet.Write(success);
            packet.Write(item.Bag);
            packet.Write(item.Slot);
            packet.Write(gemPosition);

            for (var i = 0; i < 6; i++)
                if (savedGems[i] is null)
                    packet.WriteByte(0); // bag
                else
                    packet.Write(savedGems[i].Bag);

            for (var i = 0; i < 6; i++)
                if (savedGems[i] is null)
                    packet.WriteByte(0); // slot
                else
                    packet.Write(savedGems[i].Slot);

            for (var i = 0; i < 6; i++) // NB! in old eps this value was byte.
                if (savedGems[i] is null)
                    packet.Write(0); // type id
                else
                    packet.Write((int)savedGems[i].TypeId);

            for (var i = 0; i < 6; i++)
                if (savedGems[i] is null)
                    packet.WriteByte(0); // count
                else
                    packet.Write(savedGems[i].Count);

            packet.Write(gold);
            client.Send(packet);
        }

        internal void SendSelectDyeItem(IWorldClient client, bool success)
        {
            using var packet = new ImgeneusPacket(PacketType.DYE_SELECT_ITEM);
            packet.Write(success);
            client.Send(packet);
        }

        internal void SendDyeColors(IWorldClient client, IEnumerable<DyeColor> availableColors)
        {
            using var packet = new ImgeneusPacket(PacketType.DYE_REROLL);
            foreach (var color in availableColors)
            {
                packet.Write(color.IsEnabled);
                packet.Write(color.Alpha);
                packet.Write(color.Saturation);
                packet.Write(color.R);
                packet.Write(color.G);
                packet.Write(color.B);

                for (var i = 0; i < 21; i++)
                    packet.WriteByte(0); // unknown bytes.
            }
            client.Send(packet);
        }

        internal void SendDyeConfirm(IWorldClient client, bool success, DyeColor color)
        {
            using var packet = new ImgeneusPacket(PacketType.DYE_CONFIRM);
            packet.Write(success);
            if (success)
            {
                packet.Write(color.Alpha);
                packet.Write(color.Saturation);
                packet.Write(color.R);
                packet.Write(color.G);
                packet.Write(color.B);
            }
            else
            {
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
            }
            client.Send(packet);
        }

        internal void SendAbsoluteComposition(IWorldClient client, bool isFailure, string craftName)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_COMPOSE_ABSOLUTE);
            packet.Write(isFailure);
            packet.Write(new CraftName(craftName).Serialize());
            packet.Write(true); // ?

            client.Send(packet);
        }

        internal void SendComposition(IWorldClient client, bool isFailure, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_COMPOSE);
            packet.Write(isFailure);
            packet.Write(item.Bag);
            packet.Write(item.Slot);
            packet.Write(new CraftName(item.GetCraftName()).Serialize());
            client.Send(packet);
        }

        internal void SendCanNotUseItem(IWorldClient client, int characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_ITEM);
            packet.Write(characterId);
            packet.WriteByte(0); // bag
            packet.WriteByte(0); // slot
            packet.WriteByte(0); // type
            packet.WriteByte(0); // type id
            packet.WriteByte(0); // count
            client.Send(packet);
        }

        internal void SendStatsUpdate(IWorldClient client, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc)
        {
            using var packet = new ImgeneusPacket(PacketType.UPDATE_STATS);
            packet.Write(str);
            packet.Write(dex);
            packet.Write(rec);
            packet.Write(intl);
            packet.Write(wis);
            packet.Write(luc);
            client.Send(packet);
        }

        internal void SendCharacterLeave(IWorldClient client, Character removedCharacter)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_LEFT_MAP);
            packet.Write(removedCharacter.Id);
            client.Send(packet);
        }

        internal void SendCharacterMoves(IWorldClient client, int senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MOVE);
            packet.Write(new CharacterMove(senderId, x, y, z, a, motion).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterMotion(IWorldClient client, int characterId, Motion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MOTION);
            packet.Write(characterId);
            packet.WriteByte((byte)motion);
            client.Send(packet);
        }

        internal void SendCharacterUsedSkill(IWorldClient client, int senderId, IKillable target, Skill skill, AttackResult attackResult)
        {
            PacketType skillType;
            if (target is Character)
            {
                skillType = PacketType.USE_CHARACTER_TARGET_SKILL;
            }
            else if (target is Mob)
            {
                skillType = PacketType.USE_MOB_TARGET_SKILL;
            }
            else
            {
                skillType = PacketType.USE_CHARACTER_TARGET_SKILL;
            }

            var packet = new ImgeneusPacket(skillType);
            var targetId = target is null ? 0 : target.Id;
            packet.Write(new SkillRange(senderId, targetId, skill, attackResult).Serialize());
            client.Send(packet);
            packet.Dispose();
        }

        internal void SendCharacterEnter(IWorldClient client, Character character)
        {
            using var packet0 = new ImgeneusPacket(PacketType.CHARACTER_ENTERED_MAP);
            packet0.Write(new CharacterEnteredMap(character).Serialize());
            client.Send(packet0);

            using var packet1 = new ImgeneusPacket(PacketType.CHARACTER_MOVE);
            packet1.Write(new CharacterMove(character.Id,
                                            character.MovementManager.PosX,
                                            character.MovementManager.PosY,
                                            character.MovementManager.PosZ,
                                            character.MovementManager.Angle,
                                            character.MovementManager.MoveMotion).Serialize());
            client.Send(packet1);

            SendAttackAndMovementSpeed(client, character.Id, character.SpeedManager.TotalAttackSpeed, character.SpeedManager.TotalMoveSpeed);

            SendCharacterShape(client, character); // Fix for admin in stealth + dye.

            SendShapeUpdate(client, character.Id, character.ShapeManager.Shape, character.InventoryManager.Mount is null ? 0 : character.InventoryManager.Mount.Type, character.InventoryManager.Mount is null ? 0 : character.InventoryManager.Mount.TypeId);
        }

        internal void SendMobEnter(IWorldClient client, Mob mob, bool isNew)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_ENTER);
            packet.Write(new MobEnter(mob, isNew).Serialize());
            client.Send(packet);
        }

        internal void SendMobLeave(IWorldClient client, Mob mob)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_LEAVE);
            packet.Write(mob.Id);
            client.Send(packet);
        }

        internal void SendMobMove(IWorldClient client, int senderId, float x, float z, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_MOVE);
            packet.Write(new MobMove(senderId, x, z, motion).Serialize());
            client.Send(packet);
        }

        internal void SendMobAttack(IWorldClient client, Mob mob, int targetId, AttackResult attackResult)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_ATTACK);
            packet.Write(new MobAttack(mob, targetId, attackResult).Serialize());
            client.Send(packet);
        }

        internal void SendMobUsedSkill(IWorldClient client, Mob mob, int targetId, Skill skill, AttackResult attackResult)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_SKILL_USE);
            packet.Write(new MobSkillAttack(mob, targetId, skill, attackResult).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterChangedEquipment(IWorldClient client, int characterId, Item equipmentItem, byte slot)
        {
            using var packet = new ImgeneusPacket(PacketType.SEND_EQUIPMENT);
            packet.Write(new CharacterEquipmentChange(characterId, slot, equipmentItem).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterUsualAttack(IWorldClient client, int senderId, IKillable target, AttackResult attackResult)
        {
            PacketType attackType;
            if (target is Character)
            {
                attackType = PacketType.CHARACTER_CHARACTER_AUTO_ATTACK;
            }
            else if (target is Mob)
            {
                attackType = PacketType.CHARACTER_MOB_AUTO_ATTACK;
            }
            else
            {
                attackType = PacketType.CHARACTER_CHARACTER_AUTO_ATTACK;
            }
            using var packet = new ImgeneusPacket(attackType);
            packet.Write(new UsualAttack(senderId, target.Id, attackResult).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterPartyChanged(IWorldClient client, int characterId, PartyMemberType type)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_PARTY_SET);
            packet.Write(characterId);
            packet.Write((byte)type);
            client.Send(packet);
        }

        internal void SendAttackAndMovementSpeed(IWorldClient client, int senderId, AttackSpeed attack, MoveSpeed move)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ATTACK_MOVEMENT_SPEED);
            packet.Write(new CharacterAttackAndMovement(senderId, attack, move).Serialize());
            client.Send(packet);
        }

        internal void SendCharacterKilled(IWorldClient client, Character character, IKiller killer)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_DEATH);
            packet.Write(character.Id);
            packet.WriteByte(1); // killer type. 1 - another player.
            packet.Write(killer.Id);
            client.Send(packet);
        }

        internal void SendSkillCastStarted(IWorldClient client, int senderId, IKillable target, Skill skill)
        {
            PacketType type;
            if (target is Character)
                type = PacketType.CHARACTER_SKILL_CASTING;
            else if (target is Mob)
                type = PacketType.MOB_SKILL_CASTING;
            else
                type = PacketType.CHARACTER_SKILL_CASTING;

            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillCasting(senderId, target is null ? 0 : target.Id, skill).Serialize());
            client.Send(packet);
        }

        internal void SendUsedItem(IWorldClient client, Character sender, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_ITEM);
            packet.Write(sender.Id);
            packet.Write(item.Bag);
            packet.Write(item.Slot);
            packet.Write(item.Type);
            packet.Write(item.TypeId);
            packet.Write(item.Count);
            client.Send(packet);
        }

        internal void SendRecoverCharacter(IWorldClient client, IKillable sender, int hp, int mp, int sp)
        {
            // NB!!! In previous episodes and in china ep 8 with recover packet it's sent how much hitpoints recovered.
            // But in os ep8 this packet sends current hitpoints.
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_RECOVER);
            packet.Write(sender.Id);
            packet.Write(sender.HealthManager.CurrentHP); // old eps: packet.Write(hp);
            packet.Write(sender.HealthManager.CurrentMP); // old eps: packet.Write(mp);
            packet.Write(sender.HealthManager.CurrentSP); // old eps: packet.Write(sp);
            client.Send(packet);
        }

        internal void SendSkillKeep(IWorldClient client, int id, ushort skillId, byte skillLevel, AttackResult result)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILL_KEEP);
            packet.Write(id);
            packet.Write(skillId);
            packet.Write(skillLevel);
            packet.Write(result.Damage.HP);
            packet.Write(result.Damage.MP);
            packet.Write(result.Damage.SP);
            client.Send(packet);
        }

        internal void SendShapeUpdate(IWorldClient client, int senderId, ShapeEnum shape, int? param1 = null, int? param2 = null)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE_UPDATE);
            packet.Write(senderId);
            packet.Write((byte)shape);

            // Only for ep 8.
            if (param1 != null && param2 != null)
            {
                packet.Write((int)param1);
                packet.Write((int)param2);
            }

            client.Send(packet);
        }

        internal void SendUsedRangeSkill(IWorldClient client, int senderId, IKillable target, Skill skill, AttackResult attackResult)
        {
            PacketType type;
            if (target is Character)
                type = PacketType.USE_CHARACTER_RANGE_SKILL;
            else if (target is Mob)
                type = PacketType.USE_MOB_RANGE_SKILL;
            else
                type = PacketType.USE_CHARACTER_RANGE_SKILL;

            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(senderId, target.Id, skill, attackResult).Serialize());
            client.Send(packet);
        }

        internal void SendDeadRebirth(IWorldClient client, Character sender)
        {
            using var packet = new ImgeneusPacket(PacketType.DEAD_REBIRTH);
            packet.Write(sender.Id);
            packet.WriteByte(4); // rebirth type.
            packet.Write(sender.Exp);
            packet.Write(sender.PosX);
            packet.Write(sender.PosY);
            packet.Write(sender.PosZ);
            client.Send(packet);
        }

        internal void SendCharacterRebirth(IWorldClient client, IKillable sender)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_LEAVE_DEAD);
            packet.Write(sender.Id);
            client.Send(packet);
        }

        internal void SendMobDead(IWorldClient client, IKillable sender, IKiller killer)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_DEATH);
            packet.Write(sender.Id);
            packet.WriteByte(1); // killer type. Always 1, since only player can kill the mob.
            packet.Write(killer.Id);
            client.Send(packet);
        }

        internal void SendMobRecover(IWorldClient client, IKillable sender)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_RECOVER);
            packet.Write(sender.Id);
            packet.Write(sender.HealthManager.CurrentHP);
            client.Send(packet);
        }

        internal void SendAddItem(IWorldClient client, MapItem mapItem)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_ADD_ITEM);
            packet.Write(mapItem.Id);
            packet.WriteByte(1); // kind of item
            packet.Write(mapItem.Item.Type);
            packet.Write(mapItem.Item.TypeId);
            packet.Write(mapItem.Item.Count);
            packet.Write(mapItem.PosX);
            packet.Write(mapItem.PosY);
            packet.Write(mapItem.PosZ);
            if (mapItem.Item.Type != Item.MONEY_ITEM_TYPE && mapItem.Item.ReqDex > 4) // Highlights valuable items.
                packet.Write(mapItem.Owner.Id);
            else
                packet.Write(0);
            client.Send(packet);
        }

        internal void SendRemoveItem(IWorldClient client, MapItem mapItem)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_REMOVE_ITEM);
            packet.Write(mapItem.Id);
            client.Send(packet);
        }

        internal void SendNpcEnter(IWorldClient client, Npc npc)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_NPC_ENTER);
            packet.Write(npc.Id);
            packet.Write(npc.Type);
            packet.Write(npc.TypeId);
            packet.Write(npc.PosX);
            packet.Write(npc.PosY);
            packet.Write(npc.PosZ);
            packet.Write(npc.Angle);
            client.Send(packet);
        }

        internal void SendNpcLeave(IWorldClient client, Npc npc)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_NPC_LEAVE);
            packet.Write(npc.Id);
            client.Send(packet);
        }

        internal void SendAppearanceChanged(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHANGE_APPEARANCE);
            packet.Write(character.Id);
            packet.Write(character.Hair);
            packet.Write(character.Face);
            packet.Write(character.Height);
            packet.Write((byte)character.Gender);
            client.Send(packet);
        }

        internal void SendStartSummoningVehicle(IWorldClient client, int senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE_READY);
            packet.Write(senderId);
            client.Send(packet);
        }

        internal void SendAttribute(IWorldClient client, CharacterAttributeEnum attribute, uint attributeValue)
        {
#if SHAIYA_US
            using var packet = new ImgeneusPacket(PacketType.GM_SHAIYA_US_ATTRIBUTE_SET);
#else
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ATTRIBUTE_SET);
#endif

            packet.Write(new CharacterAttribute(attribute, attributeValue).Serialize());
            client.Send(packet);
        }

        internal void SendExperienceGain(IWorldClient client, uint exp)
        {
            using var packet = new ImgeneusPacket(PacketType.EXPERIENCE_GAIN);
            packet.Write(new CharacterExperienceGain(exp).Serialize());
            client.Send(packet);
        }

        internal void SendMax_HP_MP_SP(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MAX_HP_MP_SP);
            packet.Write(new CharacterMax_HP_MP_SP(character));
            client.Send(packet);
        }

        internal void SendLevelUp(IWorldClient client, Character character, bool isAdminLevelUp = false)
        {
            var type = isAdminLevelUp ? PacketType.GM_CHARACTER_LEVEL_UP : PacketType.CHARACTER_LEVEL_UP;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new CharacterLevelUp(character).Serialize());
            client.Send(packet);
        }

        internal void SendWarning(IWorldClient client, string message)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_WARNING_PLAYER);
            packet.WriteByte((byte)(message.Length + 1));
            packet.Write(message);
            packet.WriteByte(0);
            client.Send(packet);
        }

        internal void SendBankItems(IWorldClient client, ICollection<BankItem> bankItems)
        {
            using var packet = new ImgeneusPacket(PacketType.BANK_ITEM_LIST);
            packet.Write(new BankItemList(bankItems).Serialize());
            client.Send(packet);
        }

        internal void SendBankItemClaim(IWorldClient client, byte bankSlot, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.BANK_CLAIM_ITEM);
            packet.Write(new BankItemClaim(bankSlot, item).Serialize());
            client.Send(packet);
        }
        internal void SendAccountPoints(IWorldClient client, uint points)
        {
            using var packet = new ImgeneusPacket(PacketType.ACCOUNT_POINTS);
            packet.Write(new AccountPoints(points).Serialize());
            client.Send(packet);
        }
    }
}
