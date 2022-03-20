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
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Quests;
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

        internal void SendWorldDay(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.WORLD_DAY);
            packet.Write(new DateTime(2020, 01, 01, 12, 30, 00).ToShaiyaTime());
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

        internal void SendItemExpired(IWorldClient client, Item item, ExpireType expireType)
        {
            // This is from UZC, but seems to be different in US version.
            using var packet = new ImgeneusPacket(PacketType.ITEM_EXPIRED);
            packet.Write(item.Bag);
            packet.Write(item.Slot);
            packet.Write(item.Type);
            packet.Write(item.TypeId);
            packet.WriteByte(0); // remaining mins
            packet.Write((ushort)expireType);
            client.Send(packet);
        }

        public void SendTradeCanceled(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_STOP);
            packet.WriteByte(2);
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

        internal void SendRemoveItem(IWorldClient client, Item item, bool fullRemove)
        {
            using var packet = new ImgeneusPacket(PacketType.REMOVE_ITEM);
            packet.Write(new RemovedInventoryItem(item, fullRemove).Serialize());
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

        internal void VehiclePassengerChanged(IWorldClient client, int passengerId, int vehicleCharId)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE_2);
            packet.Write(passengerId);
            packet.Write(vehicleCharId);
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

        internal void SendFinishedQuests(IWorldClient client, IEnumerable<Quest> quests)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_FINISHED_LIST);
            packet.Write(new CharacterFinishedQuests(quests).Serialize());
            client.Send(packet);
        }

        internal void SendFriendOnline(IWorldClient client, int id, bool isOnline)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_ONLINE);
            packet.Write(id);
            packet.Write(isOnline);
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

        internal void SendUseVehicle(IWorldClient client, bool success, bool status)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE);
            packet.Write(success);
            packet.Write(status);
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

        internal void SendCharacterKilled(IWorldClient client, int characterId, IKiller killer)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_DEATH);
            packet.Write(characterId);
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

        internal void SendUsedItem(IWorldClient client, int senderId, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_ITEM);
            packet.Write(senderId);
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
            packet.Write((byte)RebirthType.KillSoulByItem);
            packet.Write(sender.LevelingManager.Exp);
            packet.Write(sender.PosX);
            packet.Write(sender.PosY);
            packet.Write(sender.PosZ);
            client.Send(packet);
        }

        internal void SendCharacterRebirth(IWorldClient client, int senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_LEAVE_DEAD);
            packet.Write(senderId);
            client.Send(packet);
        }

        internal void SendMobDead(IWorldClient client, int senderId, IKiller killer)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_DEATH);
            packet.Write(senderId);
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

        internal void SendLevelUp(IWorldClient client, int characterId, ushort level, ushort statPoint, ushort skillPoint, uint minExp, uint nextExp, bool hasParty = false)
        {
            var type = hasParty ? PacketType.GM_CHARACTER_LEVEL_UP : PacketType.CHARACTER_LEVEL_UP;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new CharacterLevelUp(characterId, level, statPoint, skillPoint, minExp, nextExp).Serialize());
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

        internal void SendAccountPoints(IWorldClient client, uint points)
        {
            using var packet = new ImgeneusPacket(PacketType.ACCOUNT_POINTS);
            packet.Write(new AccountPoints(points).Serialize());
            client.Send(packet);
        }
    }
}
