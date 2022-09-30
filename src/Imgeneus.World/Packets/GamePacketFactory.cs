#if EP8_V1
using Imgeneus.World.Serialization.EP_8_V1;
#elif EP8_V2
using Imgeneus.World.Serialization.EP_8_V2;
#else
using Imgeneus.World.Serialization.SHAIYA_US;
#endif

using Imgeneus.Database.Entities;
using Imgeneus.Network.PacketProcessor;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using Imgeneus.World.SelectionScreen;
using System.Collections.Generic;
using System.Linq;
using System;
using Imgeneus.World.Game.Player;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Serialization;
using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Health;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Duel;
using System.Text;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Country;
using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.NPCs;
using Parsec.Shaiya.NpcQuest;
using Quest = Imgeneus.World.Game.Quests.Quest;
using Imgeneus.Game.Skills;
using Imgeneus.Game.Crafting;
using Imgeneus.Game.Market;

namespace Imgeneus.World.Packets
{
    public class GamePacketFactory : IGamePacketFactory
    {
        #region Handshake
        public void SendGameHandshake(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GAME_HANDSHAKE);
            packet.WriteByte(0); // 0 means there was no error.
            packet.WriteByte(2); // no idea what is it, it just works.
            packet.Write(CryptoManager.XorKey);
            client.Send(packet);
        }

        public void SendLogout(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.LOGOUT);
            client.Send(packet);
        }
        public void SendQuitGame(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.QUIT_GAME);
            client.Send(packet);
        }
        #endregion

        #region Selection screen
        public void SendCheckName(IWorldClient client, bool isAvailable)
        {
            using var packet = new ImgeneusPacket(PacketType.CHECK_CHARACTER_AVAILABLE_NAME);
            packet.Write(isAvailable);
            client.Send(packet);
        }
        public void SendCreatedCharacter(IWorldClient client, bool isCreated)
        {
            using var packet = new ImgeneusPacket(PacketType.CREATE_CHARACTER);
            packet.Write(isCreated ? 0 : 1); // 0 means character was created.
            client.Send(packet);
        }

        public void SendFaction(IWorldClient client, Fraction faction, Mode maxMode)
        {
            using var packet = new ImgeneusPacket(PacketType.ACCOUNT_FACTION);
            packet.Write((byte)faction);
            packet.Write((byte)maxMode);
            client.Send(packet);
        }

        public void SendCharacterList(IWorldClient client, IEnumerable<DbCharacter> characters)
        {
            var nonExistingCharacters = new List<ImgeneusPacket>();
            var existingCharacters = new List<ImgeneusPacket>();

            for (byte i = 0; i < SelectionScreenManager.MaxCharacterNumber; i++)
            {
                var packet = new ImgeneusPacket(PacketType.CHARACTER_LIST);
                packet.Write(i);
                var character = characters.FirstOrDefault(c => c.Slot == i && (!c.IsDelete || c.IsDelete && c.DeleteTime != null && DateTime.UtcNow.Subtract((DateTime)c.DeleteTime) < TimeSpan.FromHours(2)));
                if (character is null)
                {
                    // No char at this slot.
                    packet.Write(0);
                    nonExistingCharacters.Add(packet);
                }
                else
                {
                    packet.Write(new CharacterSelectionScreen(character).Serialize());
                    existingCharacters.Add(packet);
                }
            }

            foreach (var p in nonExistingCharacters)
                client.Send(p);

            foreach (var p in existingCharacters)
                client.Send(p);
        }

        public void SendCharacterSelected(IWorldClient client, bool ok, uint id)
        {
            using var packet = new ImgeneusPacket(PacketType.SELECT_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendDeletedCharacter(IWorldClient client, bool ok, uint id)
        {
            using var packet = new ImgeneusPacket(PacketType.DELETE_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendRestoredCharacter(IWorldClient client, bool ok, uint id)
        {
            using var packet = new ImgeneusPacket(PacketType.RESTORE_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendRenamedCharacter(IWorldClient client, bool ok, uint id)
        {
            using var packet = new ImgeneusPacket(PacketType.RENAME_CHARACTER);
            packet.Write((byte)(ok ? 1 : 0));
            packet.Write(id);
            client.Send(packet);
        }
        #endregion

        #region Character
        public void SendDetails(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_DETAILS);
            packet.Write(new CharacterDetails(character).Serialize());
            client.Send(packet);
        }

        public void SendSkillBar(IWorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems)
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

        public void SendAdditionalStats(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ADDITIONAL_STATS);
            packet.Write(new CharacterAdditionalStats(character).Serialize());
            client.Send(packet);
        }

        public void SendResetStats(IWorldClient client, Character character)
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

        public void SendResetSkills(IWorldClient client, ushort skillPoint)
        {
            using var packet = new ImgeneusPacket(PacketType.RESET_SKILLS);
            packet.Write(true); // is success?
            packet.Write(skillPoint);
            client.Send(packet);
        }

        public void SendAttribute(IWorldClient client, CharacterAttributeEnum attribute, uint attributeValue, PacketType packetType)
        {
            using var packet = new ImgeneusPacket(packetType);
            packet.Write(new CharacterAttribute(attribute, attributeValue).Serialize());
            client.Send(packet);
        }

        public void SendStatsUpdate(IWorldClient client, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc)
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

        public void SendLearnedNewSkill(IWorldClient client, bool ok, Skill skill)
        {
            using var answerPacket = new ImgeneusPacket(PacketType.LEARN_NEW_SKILL);
            answerPacket.Write((byte)(ok ? 0 : 1));
            answerPacket.Write(new LearnedSkill(skill).Serialize());
            client.Send(answerPacket);
        }

        public void SendLearnedSkills(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILLS);
            packet.Write(new CharacterSkills(character).Serialize());
            client.Send(packet);
        }

        public void SendActiveBuffs(IWorldClient client, ICollection<Buff> activeBuffs)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ACTIVE_BUFFS);
            packet.Write(new CharacterActiveBuffs(activeBuffs).Serialize());
            client.Send(packet);
        }

        public void SendAddBuff(IWorldClient client, uint id, ushort skillId, byte skillLevel, int countdown)
        {
            using var packet = new ImgeneusPacket(PacketType.BUFF_ADD);
            packet.Write(new SerializedActiveBuff(id, skillId, skillLevel, countdown).Serialize());
            client.Send(packet);
        }

        public void SendRemoveBuff(IWorldClient client, uint id)
        {
            using var packet = new ImgeneusPacket(PacketType.BUFF_REMOVE);
            packet.Write(id);
            client.Send(packet);
        }

        public void SendAutoStats(IWorldClient client, byte str, byte dex, byte rec, byte intl, byte wis, byte luc)
        {
            using var packet = new ImgeneusPacket(PacketType.AUTO_STATS_LIST);
            packet.Write(str);
            packet.Write(dex);
            packet.Write(rec);
            packet.Write(intl);
            packet.Write(wis);
            packet.Write(luc);
            client.Send(packet);
        }

        public void SendCurrentHitpoints(IWorldClient client, int hp, int mp, int sp)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_CURRENT_HITPOINTS);
            packet.Write(new CharacterHitpoints(hp, mp, sp).Serialize());
            client.Send(packet);
        }

        public void SendKillCountChanged(IWorldClient client, byte index, uint count)
        {
            using var packet = new ImgeneusPacket(PacketType.USER_KILLCOUNT_UPDATE);
            packet.Write(index);
            packet.Write(count);
            client.Send(packet);
        }

        #endregion

        #region Inventory
        public void SendInventoryItems(IWorldClient client, ICollection<Item> inventoryItems)
        {
            var steps = inventoryItems.Count / 50;
            var left = inventoryItems.Count % 50;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 50;
                var length = i == steps ? left : 50;
                var endIndex = startIndex + length;

                using var packet = new ImgeneusPacket(PacketType.CHARACTER_ITEMS);
                packet.Write(new InventoryItems(inventoryItems.Take(startIndex..endIndex)).Serialize());
                client.Send(packet);
            }
        }

        public void SendItemExpiration(IWorldClient client, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_EXPIRATION);
            packet.Write(new InventoryItemExpiration(item).Serialize());
            client.Send(packet);
        }

        public void SendAddItem(IWorldClient client, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ADD_ITEM);
            packet.Write(new AddedInventoryItem(item).Serialize());
            client.Send(packet);
        }

        public void SendMoveItem(IWorldClient client, Item sourceItem, Item destinationItem, uint gold = 0)
        {
            using var packet = new ImgeneusPacket(PacketType.INVENTORY_MOVE_ITEM);

#if EP8_V2 || SHAIYA_US || SHAIYA_US_DEBUG || DEBUG
            packet.Write(0); // Unknown int in V2.
#endif
            packet.Write(new MovedItem(sourceItem).Serialize());
            packet.Write(new MovedItem(destinationItem).Serialize());

            packet.Write(gold);

            client.Send(packet);
        }

        public void SendRemoveItem(IWorldClient client, Item item, bool fullRemove)
        {
            using var packet = new ImgeneusPacket(PacketType.REMOVE_ITEM);
            packet.Write(new RemovedInventoryItem(item, fullRemove).Serialize());
            client.Send(packet);
        }
        public void SendItemDoesNotBelong(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.ADD_ITEM);
            packet.WriteByte(0);
            packet.WriteByte(0); // Item doesn't belong to player.
            client.Send(packet);
        }

        public void SendFullInventory(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.ADD_ITEM);
            packet.WriteByte(0);
            packet.WriteByte(1); // Inventory is full.
            client.Send(packet);
        }

        public void SendCanNotUseItem(IWorldClient client, uint characterId)
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

        public void SendBoughtItem(IWorldClient client, BuyResult result, Item boughtItem, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.NPC_BUY_ITEM);
            packet.Write((byte)result);
            packet.Write(boughtItem is null ? (byte)0 : boughtItem.Bag);
            packet.Write(boughtItem is null ? (byte)0 : boughtItem.Slot);
            packet.Write(boughtItem is null ? (byte)0 : boughtItem.Type);
            packet.Write(boughtItem is null ? (byte)0 : boughtItem.TypeId);
            packet.Write(boughtItem is null ? (byte)0 : boughtItem.Count);
            packet.Write(gold);
            client.Send(packet);
        }

        public void SendSoldItem(IWorldClient client, bool success, Item soldItem, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.NPC_SELL_ITEM);
            packet.Write(success ? (byte)0 : (byte)1); // success
            packet.Write(soldItem is null ? (byte)0 : soldItem.Bag);
            packet.Write(soldItem is null ? (byte)0 : soldItem.Slot);
            packet.Write(soldItem is null ? (byte)0 : soldItem.Type);
            packet.Write(soldItem is null ? (byte)0 : soldItem.TypeId);
            packet.Write(soldItem is null ? (byte)0 : soldItem.Count);
            packet.Write(gold);
            client.Send(packet);
        }

        public void SendGoldUpdate(IWorldClient client, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.SET_MONEY);
            packet.Write(gold);
            client.Send(packet);
        }

        public void SendItemExpired(IWorldClient client, Item item, ExpireType expireType)
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

        public void SendInventorySort(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.INVENTORY_SORT);
            client.Send(packet);
        }

        #endregion

        #region Vehicle

        public void SendUseVehicle(IWorldClient client, bool ok, bool isOnVehicle)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE);
            packet.Write(ok);
            packet.Write(isOnVehicle);
            client.Send(packet);
        }

        public void SendVehicleResponse(IWorldClient client, VehicleResponse status)
        {
            using var packet = new ImgeneusPacket(PacketType.VEHICLE_RESPONSE);
            packet.Write((byte)status);
            client.Send(packet);
        }

        public void SendVehicleRequest(IWorldClient client, uint requesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.VEHICLE_REQUEST);
            packet.Write(requesterId);
            client.Send(packet);
        }

        public void SendStartSummoningVehicle(IWorldClient client, uint senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE_READY);
            packet.Write(senderId);
            client.Send(packet);
        }

        public void SendVehiclePassengerChanged(IWorldClient client, uint passengerId, uint vehicleCharId)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE_2);
            packet.Write(passengerId);
            packet.Write(vehicleCharId);
            client.Send(packet);
        }

        #endregion

        #region Map
        public void SendCharacterMotion(IWorldClient client, uint characterId, Motion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MOTION);
            packet.Write(characterId);
            packet.WriteByte((byte)motion);
            client.Send(packet);
        }

        public void SendCharacterMoves(IWorldClient client, uint senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MOVE);
            packet.Write(new CharacterMove(senderId, x, y, z, a, motion).Serialize());
            client.Send(packet);
        }

        public void SendCharacterChangedEquipment(IWorldClient client, uint characterId, Item equipmentItem, byte slot)
        {
            using var packet = new ImgeneusPacket(PacketType.SEND_EQUIPMENT);
            packet.Write(new CharacterEquipmentChange(characterId, slot, equipmentItem).Serialize());
            client.Send(packet);
        }

        public void SendCharacterShape(IWorldClient client, uint characterId, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE);
            packet.Write(characterId);
            packet.Write(new CharacterShape(character).Serialize());
            client.Send(packet);
        }

        public void SendShapeUpdate(IWorldClient client, uint senderId, ShapeEnum shape, uint? param1 = null, uint? param2 = null)
        {
            if (shape != ShapeEnum.Mob)
            {
                using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE_UPDATE);
                packet.Write(senderId);
                packet.Write((byte)shape);

                // Only for ep 8. Type & TypeId for new mounts.
                if (param1 != null && param2 != null)
                {
                    packet.Write((uint)param1);
                    packet.Write((uint)param2);
                }
                else
                {
                    packet.Write((uint)0);
                    packet.Write((uint)0);
                }

                client.Send(packet);
            }
            else
            {
                using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE_UPDATE_MOB);
                packet.Write(senderId);
                packet.Write((byte)shape);
                packet.Write((ushort)param1); // Mob id.
                client.Send(packet);
            }
        }

        public void SendMaxHitpoints(IWorldClient client, uint characterId, HitpointType type, int value)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MAX_HITPOINTS);
            packet.Write(new MaxHitpoint(characterId, type, value).Serialize());
            client.Send(packet);
        }

        public void SendRecoverCharacter(IWorldClient client, uint characterId, int hp, int mp, int sp)
        {
            // NB!!! In previous episodes and in china ep 8 with recover packet it's sent how much hitpoints recovered.
            // But in os ep8 this packet sends current hitpoints.
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_RECOVER);
            packet.Write(characterId);
            packet.Write(hp); // old eps: newHP - oldHP
            packet.Write(mp); // old eps: newMP - oldMP
            packet.Write(sp); // old eps: newSP - oldSP
            client.Send(packet);
        }

        public void SendMobRecover(IWorldClient client, uint mobId, int hp)
        {
            // Mob recover causes hp glitch. Instead os server used 0305 packet. Why? =\
            //using var packet = new ImgeneusPacket(PacketType.MOB_RECOVER);
            //packet.Write(mobId);
            //packet.Write(hp);
            //client.Send(packet);
        }

        public void SendAppearanceChanged(IWorldClient client, uint characterId, byte hair, byte face, byte size, byte gender)
        {
            using var packet = new ImgeneusPacket(PacketType.CHANGE_APPEARANCE);
            packet.Write(characterId);
            packet.Write(hair);
            packet.Write(face);
            packet.Write(size);
            packet.Write(gender);
            client.Send(packet);
        }

        public void SendPortalTeleportNotAllowed(IWorldClient client, PortalTeleportNotAllowedReason reason)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ENTERED_PORTAL);
            packet.Write(false); // success
            packet.Write((byte)reason);
            client.Send(packet);
        }

        public void SendWeather(IWorldClient client, Map map)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_WEATHER);
            packet.Write(new MapWeather(map).Serialize());
            client.Send(packet);
        }

        public void SendCharacterTeleport(IWorldClient client, uint characterId, ushort mapId, float x, float y, float z, bool teleportedByAdmin)
        {
            using var packet = new ImgeneusPacket(teleportedByAdmin ? PacketType.CHARACTER_MAP_TELEPORT : PacketType.GM_TELEPORT_MAP_COORDINATES);
            packet.Write(characterId);
            packet.Write(mapId);
            packet.Write(x);
            packet.Write(y);
            packet.Write(z);
            client.Send(packet);
        }

        public void SendCharacterLeave(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_LEFT_MAP);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendCharacterEnter(IWorldClient client, Character character)
        {
            using var packet0 = new ImgeneusPacket(PacketType.CHARACTER_ENTERED_MAP);
            packet0.Write(new CharacterEnteredMap(character).Serialize());
            client.Send(packet0);

            if (character.ShapeManager.Shape != ShapeEnum.None)
                if (character.ShapeManager.Shape != ShapeEnum.Mob)
                    SendShapeUpdate(client, character.Id, character.ShapeManager.Shape, character.InventoryManager.Mount is null ? 0 : (uint)character.InventoryManager.Mount.Type, character.InventoryManager.Mount is null ? 0 : (uint)character.InventoryManager.Mount.TypeId);
                else
                    SendShapeUpdate(client, character.Id, character.ShapeManager.Shape, character.ShapeManager.MobId);

            SendAttackAndMovementSpeed(client, character.Id, character.SpeedManager.TotalAttackSpeed, character.SpeedManager.TotalMoveSpeed);

            if (character.ShapeManager.IsTranformated)
                SendTransformation(client, character.Id, character.ShapeManager.IsTranformated);
        }

        public void SendAttackAndMovementSpeed(IWorldClient client, uint senderId, AttackSpeed attack, MoveSpeed move)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ATTACK_MOVEMENT_SPEED);
            packet.Write(new CharacterAttackAndMovement(senderId, attack, move).Serialize());
            client.Send(packet);
        }

        public void SendCharacterUsedSkill(IWorldClient client, uint senderId, IKillable target, Skill skill, AttackResult attackResult)
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
            packet.Write(new SkillRange(senderId, targetId, skill.SkillId, skill.SkillLevel, attackResult, skill.CanBeActivated).Serialize());
            client.Send(packet);
            packet.Dispose();

            if (skill.IsActivated) // Play activation animation.
            {
                var packet2 = new ImgeneusPacket(PacketType.USE_CHARACTER_TARGET_SKILL);
                targetId = target is null ? 0 : target.Id;
                packet2.Write(new SkillRange(senderId, targetId, skill.SkillId, 2, attackResult, false).Serialize()); // Some animations are not played with level 1. Why?
                client.Send(packet2);
                packet2.Dispose();
            }
        }


        public void SendAbsorbValue(IWorldClient client, ushort absorb)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ABSORPTION_DAMAGE);
            packet.Write(absorb);
            client.Send(packet);
        }

        public void SendSkillCastStarted(IWorldClient client, uint senderId, IKillable target, Skill skill)
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

        public void SendUsedItem(IWorldClient client, uint senderId, Item item)
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

        public void SendMax_HP_MP_SP(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MAX_HP_MP_SP);
            packet.Write(new CharacterMax_HP_MP_SP(character));
            client.Send(packet);
        }

        public void SendSkillKeep(IWorldClient client, uint characterId, ushort skillId, byte skillLevel, AttackResult result)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILL_KEEP);
            packet.Write(characterId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            packet.Write(result.Damage.HP);
            packet.Write(result.Damage.SP);
            packet.Write(result.Damage.MP);
            client.Send(packet);
        }

        public void SendUsedRangeSkill(IWorldClient client, uint senderId, IKillable target, Skill skill, AttackResult attackResult)
        {
            PacketType type;
            if (target is Character)
                type = PacketType.USE_CHARACTER_RANGE_SKILL;
            else if (target is Mob)
                type = PacketType.USE_MOB_RANGE_SKILL;
            else
                type = PacketType.USE_CHARACTER_RANGE_SKILL;

            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(senderId, target.Id, skill.SkillId, skill.SkillLevel, attackResult, skill.CanBeActivated).Serialize());
            client.Send(packet);
        }

        public void SendAddItem(IWorldClient client, MapItem mapItem)
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

        public void SendRemoveItem(IWorldClient client, MapItem mapItem)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_REMOVE_ITEM);
            packet.Write(mapItem.Id);
            client.Send(packet);
        }

        public void SendTransformation(IWorldClient client, uint senderId, bool isTransformed)
        {
            using var packet = new ImgeneusPacket(PacketType.TRANSFORMATION_MODE);
            packet.Write(senderId);
            packet.Write((ushort)1);
            packet.Write(isTransformed);
            client.Send(packet);
        }

        public void SendCharacterMirrorDamage(IWorldClient client, uint senderId, uint targetId, Damage damage)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILL_MIRROR);
            packet.Write(targetId);
            packet.Write(senderId);
            packet.Write(damage.HP);
            packet.Write(damage.SP);
            packet.Write(damage.MP);
            client.Send(packet);
        }

        public void SendMobMirrorDamage(IWorldClient client, uint senderId, uint targetId, Damage damage)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_SKILL_MIRROR);
            packet.Write(targetId);
            packet.Write(senderId);
            packet.Write(damage.HP);
            packet.Write(damage.SP);
            packet.Write(damage.MP);
            client.Send(packet);
        }

        public void SendCharacterTargetHP(IWorldClient client, uint targetId, int currentHP, int maxHP, AttackSpeed attackSpeed, MoveSpeed moveSpeed)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_CHARACTER_HP_UPDATE);
            packet.Write(targetId);
            packet.Write(currentHP);
            packet.Write(maxHP);
            packet.Write((byte)attackSpeed);
            packet.Write((byte)moveSpeed);
            client.Send(packet);
        }

        public void SendMobTargetHP(IWorldClient client, uint targetId, int currentHP, AttackSpeed attackSpeed, MoveSpeed moveSpeed)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_MOB_HP_UPDATE);
            packet.Write(targetId);
            packet.Write(currentHP);
            packet.Write((byte)attackSpeed);
            packet.Write((byte)moveSpeed);
            client.Send(packet);
        }

        public void SendKillsUpdate(IWorldClient client, uint senderId, uint kills)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_KILLINFO);
            packet.Write(senderId);
            packet.Write(kills);
            client.Send(packet);
        }

        #endregion

        #region NPC

        public void SendNpcLeave(IWorldClient client, Npc npc)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_NPC_LEAVE);
            packet.Write(npc.Id);
            client.Send(packet);
        }

        public void SendNpcEnter(IWorldClient client, Npc npc)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_NPC_ENTER);
            packet.Write(npc.Id);
            packet.Write((byte)npc.Type);
            packet.Write(npc.TypeId);
            packet.Write(npc.PosX);
            packet.Write(npc.PosY);
            packet.Write(npc.PosZ);
            packet.Write(npc.Angle);
            client.Send(packet);
        }

        public void SendNpcMove(IWorldClient client, uint senderId, float x, float y, float z, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_NPC_MOVE);
            packet.Write(senderId);
            packet.Write((byte)motion);
            packet.Write(x);
            packet.Write(y);
            packet.Write(z);
            client.Send(packet);
        }

        public void SendNpcAttack(IWorldClient client, uint senderId, IKillable target, AttackResult result)
        {
            using var packet = new ImgeneusPacket(target is Character ? PacketType.MAP_NPC_ATTACK_PLAYER : PacketType.MAP_NPC_ATTACK_MOB);
            packet.Write((byte)result.Success);
            packet.Write(senderId);
            packet.Write(target.Id);
            packet.Write(result.Damage.HP);
            client.Send(packet);
        }

        #endregion

        #region Linking

        public void SendGemPossibility(IWorldClient client, double rate, int gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_ADD_POSSIBILITY);
            packet.WriteByte(1); // TODO: unknown, maybe bool, that we can link?
            packet.Write(rate);
            packet.Write(gold);
            client.Send(packet);
        }

        public void SendAddGem(IWorldClient client, bool success, Item gem, Item item, byte slot, uint gold, Item hammer)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_ADD);
            packet.Write(success);

            if (gem is null)
            {
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
            }
            else
            {
                packet.Write(gem.Bag);
                packet.Write(gem.Slot);
                packet.Write(gem.Count);
            }

            if (item is null)
            {
                packet.WriteByte(0);
                packet.WriteByte(0);
            }
            else
            {
                packet.Write(item.Bag);
                packet.Write(item.Slot);
            }


            packet.Write(slot);

            if (gem is null)
            {
                packet.WriteByte(0);
            }
            else
            {
                packet.Write(gem.TypeId);
            }
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

        public void SendGemRemovePossibility(IWorldClient client, double rate, int gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_REMOVE_POSSIBILITY);
            packet.WriteByte(1); // TODO: unknown, maybe bool, that we can link?
            packet.Write(rate);
            packet.Write(gold);
            client.Send(packet);
        }

        public void SendRemoveGem(IWorldClient client, bool success, Item item, byte slot, List<Item> savedGems, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GEM_REMOVE);
            packet.Write(success);
            packet.Write(item.Bag);
            packet.Write(item.Slot);
            packet.Write(slot);

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

        #endregion

        #region Composition

        public void SendComposition(IWorldClient client, bool ok, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_COMPOSE);
            packet.Write(ok ? (byte)0 : (byte)1);
            packet.Write(item.Bag);
            packet.Write(item.Slot);
            packet.Write(new CraftName(item.GetCraftName()).Serialize());
            client.Send(packet);
        }

        public void SendAbsoluteComposition(IWorldClient client, bool ok, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_COMPOSE_ABSOLUTE);
            packet.Write(ok ? (byte)0 : (byte)1);
            packet.Write(new CraftName(item.GetCraftName()).Serialize());
            packet.Write(true); // ?

            client.Send(packet);
        }

        public void SendRuneSynthesize(IWorldClient client, bool ok, Item rune)
        {
            using var packet = new ImgeneusPacket(PacketType.RUNE_SYNTHESIZE);
            packet.Write(ok ? (byte)0 : (byte)1);
            client.Send(packet);
        }


        #endregion

        #region Dyeing

        public void SendSelectDyeItem(IWorldClient client, bool success)
        {
            using var packet = new ImgeneusPacket(PacketType.DYE_SELECT_ITEM);
            packet.Write(success);
            client.Send(packet);
        }

        public void SendDyeColors(IWorldClient client, IEnumerable<DyeColor> availableColors)
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

        public void SendDyeConfirm(IWorldClient client, bool success, DyeColor color)
        {
            // TODO: in shaiya US this does not work.
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

        #endregion

        #region Enchantment

        public void SendEnchantRate(IWorldClient client, IEnumerable<byte> lapisiaBag, IEnumerable<byte> lapisiaSlot, IEnumerable<int> rates, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.ENCHANT_RATE);

            foreach (var bag in lapisiaBag)
                packet.WriteByte(bag);

            foreach (var slot in lapisiaSlot)
                packet.WriteByte(slot);

            foreach (var rate in rates)
                packet.Write(rate);

            for (var i = 0; i < 10; i++)
                packet.Write(gold);

            client.Send(packet);
        }

        public void SendEnchantAdd(IWorldClient client, bool success, Item lapisia, Item item, uint gold, bool autoEnchant, bool safetyScrollLeft)
        {
            using var packet = new ImgeneusPacket(PacketType.ENCHANT_ADD);

            packet.Write(success);
            packet.WriteByte(lapisia is null ? (byte)0 : lapisia.Bag);
            packet.WriteByte(lapisia is null ? (byte)0 : lapisia.Slot);
            packet.WriteByte(lapisia is null ? (byte)0 : lapisia.Count);
            packet.WriteByte(lapisia is null ? (byte)0 : item.Bag);
            packet.WriteByte(lapisia is null ? (byte)0 : item.Slot);
            packet.Write(gold);
            packet.Write(autoEnchant);
            packet.WriteByte(safetyScrollLeft ? (byte)0: (byte)1); // 0 still has safety scroll

            var craftName = new CraftName(item is null ? Item.DEFAULT_CRAFT_NAME : item.GetCraftName()).Serialize();
            packet.Write(craftName);

            client.Send(packet);
        }

        #endregion

        #region Party

        public void SendPartyRequest(IWorldClient client, uint requesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_REQUEST);
            packet.Write(requesterId);
            client.Send(packet);
        }

        public void SendDeclineParty(IWorldClient client, uint charId)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_RESPONSE);
            packet.Write(false);
            packet.Write(charId);
            client.Send(packet);
        }

        public void SendPartyInfo(IWorldClient client, IEnumerable<Character> partyMembers, byte leaderIndex)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_LIST);
            packet.Write(new UsualParty(partyMembers, leaderIndex).Serialize());
            client.Send(packet);
        }

        public void SendPlayerJoinedParty(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_ENTER);
            packet.Write(new PartyMember(character).Serialize());
            client.Send(packet);
        }

        public void SendPlayerLeftParty(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_LEAVE);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendPartyKickMember(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_KICK);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendRegisteredInPartySearch(IWorldClient client, bool isSuccess)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_SEARCH_REGISTRATION);
            packet.Write(isSuccess);
            client.Send(packet);
        }

        public void SendPartySearchList(IWorldClient client, IEnumerable<Character> partySearchers)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_SEARCH_LIST);
            packet.Write(new PartySearchList(partySearchers).Serialize());
            client.Send(packet);
        }

        public void SendNewPartyLeader(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_CHANGE_LEADER);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendAddPartyBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_ADDED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendRemovePartyBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_REMOVED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendPartySingle_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_CHARACTER_SP_MP);
            packet.Write(senderId);
            packet.Write(type);
            packet.Write(value);
            client.Send(packet);
        }

        public void SendPartySingle_Max_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_SET_MAX);
            packet.Write(senderId);
            packet.Write(type);
            packet.Write(value);
            client.Send(packet);
        }

        public void SendParty_HP_SP_MP(IWorldClient client, Character partyMember)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_MEMBER_HP_SP_MP);
            packet.Write(new PartyMember_HP_SP_MP(partyMember).Serialize());
            client.Send(packet);
        }

        public void SendParty_Max_HP_SP_MP(IWorldClient client, Character partyMember)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_MEMBER_MAX_HP_SP_MP);
            packet.Write(new PartyMemberMax_HP_SP_MP(partyMember).Serialize());
            client.Send(packet);
        }

        public void SendPartyMemberGetItem(IWorldClient client, uint characterId, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_MEMBER_GET_ITEM);
            packet.Write(characterId);
            packet.Write(item.Type);
            packet.Write(item.TypeId);
            client.Send(packet);
        }

        public void SendPartyLevel(IWorldClient client, uint senderId, ushort level)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_MEMBER_LEVEL);
            packet.Write(senderId);
            packet.Write(level);
            client.Send(packet);
        }

        public void SendPartyError(IWorldClient client, PartyErrorType partyError, int id = 0)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_PARTY_ERROR);
            packet.Write((int)partyError);
            packet.Write(id);
            client.Send(packet);
        }

        public void SendCharacterPartyChanged(IWorldClient client, uint characterId, PartyMemberType type)
        {
            using var packet = new ImgeneusPacket(PacketType.MAP_PARTY_SET);
            packet.Write(characterId);
            packet.Write((byte)type);
            client.Send(packet);
        }
        #endregion

        #region Raid

        public void SendRaidCreated(IWorldClient client, Raid raid)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CREATE);
            packet.Write(true); // raid type ?
            packet.Write(raid.AutoJoin);
            packet.Write((int)raid.DropType);
            packet.Write(raid.GetIndex(raid.Leader));
            packet.Write(raid.GetIndex(raid.SubLeader));
            client.Send(packet);
        }

        public void SendRaidDismantle(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_DISMANTLE);
            client.Send(packet);
        }

        public void SendRaidInfo(IWorldClient client, Raid raid)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_LIST);
            packet.Write(new RaidParty(raid).Serialize());
            client.Send(packet);
        }

        public void SendPlayerJoinedRaid(IWorldClient client, Character character, ushort position)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_ENTER);
            packet.Write(new RaidMember(character, position).Serialize());
            client.Send(packet);
        }

        public void SendPlayerLeftRaid(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_LEAVE);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendAutoJoinChanged(IWorldClient client, bool autoJoin)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CHANGE_AUTOINVITE);
            packet.Write(autoJoin);
            client.Send(packet);
        }

        public void SendDropType(IWorldClient client, RaidDropType dropType)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CHANGE_LOOT);
            packet.Write((int)dropType);
            client.Send(packet);
        }

        public void SendAddRaidBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_ADDED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendRemoveRaidBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_REMOVED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendRaid_Single_HP_SP_MP(IWorldClient client, uint characterId, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CHARACTER_SP_MP);
            packet.Write(characterId);
            packet.Write(type);
            packet.Write(value);
            client.Send(packet);
        }

        public void SendRaid_Single_Max_HP_SP_MP(IWorldClient client, uint characterId, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_SET_MAX);
            packet.Write(characterId);
            packet.Write(type);
            packet.Write(value);
            client.Send(packet);
        }

        public void SendRaidNewLeader(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CHANGE_LEADER);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendNewRaidSubLeader(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CHANGE_SUBLEADER);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendRaidKickMember(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_KICK);
            packet.Write(character.Id);
            client.Send(packet);
        }

        public void SendPlayerMove(IWorldClient client, int sourceIndex, int destinationIndex, int leaderIndex, int subLeaderIndex)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_MOVE_PLAYER);
            packet.Write(sourceIndex);
            packet.Write(destinationIndex);
            packet.Write(leaderIndex);
            packet.Write(subLeaderIndex);
            client.Send(packet);
        }

        public void SendMemberGetItem(IWorldClient client, uint characterId, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_MEMBER_GET_ITEM);
            packet.Write(characterId);
            packet.Write(item.Type);
            packet.Write(item.TypeId);
            client.Send(packet);
        }

        public void SendRaidInvite(IWorldClient client, uint requesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_INVITE);
            packet.Write(requesterId);
            client.Send(packet);
        }

        public void SendDeclineRaid(IWorldClient client, uint charId)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_RESPONSE);
            packet.Write(false);
            packet.Write(charId);
            client.Send(packet);
        }
        #endregion

        #region Trade

        public void SendTradeRequest(IWorldClient client, uint tradeRequesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_REQUEST);
            packet.Write(tradeRequesterId);
            client.Send(packet);
        }

        public void SendTradeStart(IWorldClient client, uint traderId)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_START);
            packet.Write(traderId);
            client.Send(packet);
        }

        public void SendAddedItemToTrade(IWorldClient client, byte bag, byte slot, byte quantity, byte slotInTradeWindow)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_OWNER_ADD_ITEM);
            packet.Write(bag);
            packet.Write(slot);
            packet.Write(quantity);
            packet.Write(slotInTradeWindow);
            client.Send(packet);
        }

        public void SendAddedItemToTrade(IWorldClient client, Item tradeItem, byte quantity, byte slotInTradeWindow)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_RECEIVER_ADD_ITEM);
            packet.Write(new TradeItem(slotInTradeWindow, quantity, tradeItem).Serialize());
            client.Send(packet);
        }

        public void SendTradeCanceled(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_STOP);
            packet.WriteByte(2);
            client.Send(packet);
        }

        public void SendRemovedItemFromTrade(IWorldClient client, byte byWho)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_REMOVE_ITEM);
            packet.Write(byWho);
            client.Send(packet);
        }

        public void SendAddedMoneyToTrade(IWorldClient client, byte byWho, uint tradeMoney)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_ADD_MONEY);
            packet.Write(byWho);
            packet.Write(tradeMoney);
            client.Send(packet);
        }

        public void SendTradeDecide(IWorldClient client, byte byWho, bool isDecided)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_DECIDE);
            packet.WriteByte(byWho);
            packet.Write(isDecided);
            client.Send(packet);
        }

        public void SendTradeConfirm(IWorldClient client, byte byWho, bool isDeclined)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_FINISH);
            packet.WriteByte(byWho);
            packet.Write(isDeclined);
            client.Send(packet);
        }

        public void SendTradeFinished(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_STOP);
            packet.WriteByte(0);
            client.Send(packet);
        }

        #endregion

        #region Attack

        public void SendAutoAttackStop(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.AUTO_ATTACK_STOP);
            client.Send(packet);
        }

        public void SendPlayerInTarget(IWorldClient client, uint characterId, int maxHp, int hp)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_CHARACTER_MAX_HP);
            packet.Write(characterId);
            packet.Write(maxHp);
            packet.Write(hp);
            client.Send(packet);
        }

        public void SendCurrentBuffs(IWorldClient client, IKillable target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_BUFFS);
            packet.Write(new TargetBuffs(target).Serialize());
            client.Send(packet);
        }

        public void SendTargetAddBuff(IWorldClient client, uint targetId, Buff buff, bool isMob)
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
            packet.Write(buff.Skill.SkillId);
            packet.Write(buff.Skill.SkillLevel);

            client.Send(packet);
        }

        public void SendTargetRemoveBuff(IWorldClient client, uint targetId, Buff buff, bool isMob)
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
            packet.Write(buff.Skill.SkillId);
            packet.Write(buff.Skill.SkillLevel);

            client.Send(packet);
        }

        public void SendMobState(IWorldClient client, Mob target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_MOB_GET_STATE);
            packet.Write(target.Id);
            packet.Write(target.HealthManager.CurrentHP);
            packet.Write((byte)target.SpeedManager.TotalAttackSpeed);
            packet.Write((byte)target.SpeedManager.TotalMoveSpeed);
            client.Send(packet);
        }

        public void SendAutoAttackFailed(IWorldClient client, uint senderId, IKillable target, AttackSuccess reason)
        {
            var type = target is Character ? PacketType.CHARACTER_CHARACTER_AUTO_ATTACK : PacketType.CHARACTER_MOB_AUTO_ATTACK;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new UsualAttack(senderId, 0, new AttackResult() { Success = reason }).Serialize());
            client.Send(packet);
        }

        public void SendUseSkillFailed(IWorldClient client, uint senderId, Skill skill, IKillable target, AttackSuccess reason)
        {
            var type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(senderId, 0, skill.SkillId, skill.SkillLevel, new AttackResult() { Success = reason }, skill.CanBeActivated).Serialize());
            client.Send(packet);
        }

        public void SendUseSMMP(IWorldClient client, ushort MP, ushort SP)
        {
            using var packet = new ImgeneusPacket(PacketType.USED_SP_MP);
            packet.Write(new UseSPMP(SP, MP).Serialize());
            client.Send(packet);
        }

        public void SendCharacterUsualAttack(IWorldClient client, uint senderId, IKillable target, AttackResult attackResult)
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

        #endregion

        #region Mobs

        public void SendMobPosition(IWorldClient client, uint senderId, float x, float z, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_MOVE);
            packet.Write(new MobMove(senderId, x, z, motion).Serialize());
            client.Send(packet);
        }

        public void SendMobEnter(IWorldClient client, Mob mob, bool isNew)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_ENTER);
            packet.Write(new MobEnter(mob, isNew).Serialize());
            client.Send(packet);
        }

        public void SendMobLeave(IWorldClient client, Mob mob)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_LEAVE);
            packet.Write(mob.Id);
            client.Send(packet);
        }

        public void SendMobMove(IWorldClient client, uint senderId, float x, float z, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_MOVE);
            packet.Write(new MobMove(senderId, x, z, motion).Serialize());
            client.Send(packet);
        }

        public void SendMobAttack(IWorldClient client, uint mobId, uint targetId, AttackResult attackResult)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_ATTACK);
            packet.Write(new MobAttack(mobId, targetId, attackResult).Serialize());
            client.Send(packet);
        }

        public void SendMobUsedSkill(IWorldClient client, uint mobId, uint targetId, Skill skill, AttackResult attackResult)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_SKILL_USE);
            packet.Write(new MobSkillAttack(mobId, targetId, skill, attackResult).Serialize());
            client.Send(packet);
        }

        public void SendMobUsedRangeSkill(IWorldClient client, uint senderId, uint targetId, Skill skill, AttackResult attackResult)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_RANGE_SKILL_USE);
            packet.Write(new SkillRange(senderId, targetId, skill.SkillId, skill.SkillLevel, attackResult, false).Serialize());
            client.Send(packet);
        }

        public void SendMobDead(IWorldClient client, uint senderId, IKiller killer)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_DEATH);
            packet.Write(senderId);
            packet.WriteByte(killer is GuardNpc ? (byte)3 : (byte)1);
            packet.Write(killer.Id);
            client.Send(packet);
        }

        public void SendMobSkillKeep(IWorldClient client, uint senderId, ushort skillId, byte skillLevel, AttackResult result)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_SKILL_KEEP);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            packet.Write(result.Damage.HP);
            packet.Write(result.Damage.SP);
            packet.Write(result.Damage.MP);
            client.Send(packet);
        }

        #endregion

        #region Friends

        public void SendFriendRequest(IWorldClient client, string name)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_REQUEST);
            packet.WriteString(name, 21);
            client.Send(packet);
        }

        public void SendFriendResponse(IWorldClient client, bool accepted)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_RESPONSE);
            packet.Write(accepted);
            client.Send(packet);
        }

        public void SendFriendAdded(IWorldClient client, Friend friend)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_ADD);
            packet.Write(friend.Id);
            packet.Write((byte)friend.Job);
            packet.WriteString(friend.Name, 21);
            client.Send(packet);
        }

        public void SendFriendDeleted(IWorldClient client, uint id)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_DELETE);
            packet.Write(id);
            client.Send(packet);
        }

        public void SendFriends(IWorldClient client, IEnumerable<Friend> friends)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_LIST);
            packet.Write(new FriendsList(friends).Serialize());
            client.Send(packet);
        }

        public void SendFriendOnline(IWorldClient client, uint id, bool isOnline)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_ONLINE);
            packet.Write(id);
            packet.Write(isOnline);
            client.Send(packet);
        }

        #endregion

        #region Duel

        public void SendWaitingDuel(IWorldClient client, uint duelStarterId, uint duelOpponentId)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_REQUEST);
            packet.Write(duelStarterId);
            packet.Write(duelOpponentId);
            client.Send(packet);
        }

        public void SendDuelResponse(IWorldClient client, DuelResponse response, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_RESPONSE);
            packet.Write((byte)response);
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendDuelStartTrade(IWorldClient client, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_TRADE);
            packet.Write(characterId);
            packet.WriteByte(0); // ?
            client.Send(packet);
        }

        public void SendDuelAddItem(IWorldClient client, Item tradeItem, byte quantity, byte slotInTradeWindow)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_TRADE_OPPONENT_ADD_ITEM);
            packet.Write(new TradeItem(slotInTradeWindow, quantity, tradeItem).Serialize());
            client.Send(packet);
        }

        public void SendDuelAddItem(IWorldClient client, byte bag, byte slot, byte quantity, byte slotInTradeWindow)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_TRADE_ADD_ITEM);
            packet.Write(bag);
            packet.Write(slot);
            packet.Write(quantity);
            packet.Write(slotInTradeWindow);
            client.Send(packet);
        }

        public void SendDuelRemoveItem(IWorldClient client, byte slotInTradeWindow, byte senderType)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_TRADE_REMOVE_ITEM);
            packet.Write(senderType);
            packet.Write(slotInTradeWindow);
            client.Send(packet);
        }

        public void SendDuelAddMoney(IWorldClient client, byte senderType, uint tradeMoney)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_TRADE_ADD_MONEY);
            packet.Write(senderType);
            packet.Write(tradeMoney);
            client.Send(packet);
        }

        public void SendDuelCloseTrade(IWorldClient client, DuelCloseWindowReason reason)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_CLOSE_TRADE);
            packet.Write((byte)reason);
            client.Send(packet);
        }

        public void SendDuelApprove(IWorldClient client, byte senderType, bool isApproved)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_TRADE_OK);
            packet.Write(senderType);
            packet.Write(isApproved ? (byte)0 : (byte)1);
            client.Send(packet);
        }

        public void SendDuelReady(IWorldClient client, float x, float z)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_READY);
            packet.Write(x);
            packet.Write(z);
            client.Send(packet);
        }

        public void SendDuelStart(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_START);
            client.Send(packet);
        }

        public void SendDuelCancel(IWorldClient client, DuelCancelReason cancelReason, uint playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_CANCEL);
            packet.Write((byte)cancelReason);
            packet.Write(playerId);
            client.Send(packet);
        }

        public void SendDuelFinish(IWorldClient client, bool isWin)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_WIN_LOSE);
            packet.WriteByte(isWin ? (byte)1 : (byte)2); // 1 - win, 2 - lose
            client.Send(packet);
        }

        #endregion

        #region Chat

        public void SendNormal(IWorldClient client, uint senderId, string message, bool isAdmin)
        {
            SendMessage(isAdmin ? PacketType.CHAT_NORMAL_ADMIN : PacketType.CHAT_NORMAL, client, senderId, message);
        }

        public void SendWhisper(IWorldClient client, string senderName, string message, bool isAdmin)
        {
            SendMessage(isAdmin ? PacketType.CHAT_WHISPER_ADMIN : PacketType.CHAT_WHISPER, client, senderName, message, true);
        }

        public void SendParty(IWorldClient client, uint senderId, string message, bool isAdmin)
        {
            SendMessage(isAdmin ? PacketType.CHAT_PARTY_ADMIN : PacketType.CHAT_PARTY, client, senderId, message);
        }

        public void SendMap(IWorldClient client, string senderName, string message)
        {
            SendMessage(PacketType.CHAT_MAP, client, senderName, message);
        }

        public void SendWorld(IWorldClient client, string senderName, string message)
        {
            SendMessage(PacketType.CHAT_WORLD, client, senderName, message);
        }

        public void SendMessageToServer(IWorldClient client, string senderName, string message)
        {
            SendMessage(PacketType.CHAT_MESSAGE_TO_SERVER, client, senderName, message);
        }

        public void SendGuild(IWorldClient client, string senderName, string message, bool isAdmin)
        {
            SendMessage(isAdmin ? PacketType.CHAT_GUILD_ADMIN : PacketType.CHAT_GUILD, client, senderName, message);
        }

        private void SendMessage(PacketType packetType, IWorldClient client, uint senderId, string message)
        {
            using var packet = new ImgeneusPacket(packetType);
            packet.Write(senderId);

#if EP8_V2
            packet.WriteByte((byte)(message.Length + 1));
#endif

            packet.WriteByte((byte)message.Length);

#if (EP8_V2 || SHAIYA_US || SHAIYA_US_DEBUG || DEBUG)
            packet.WriteString(message, message.Length, Encoding.Unicode);
#else
            packet.WriteString(message);
#endif

            client.Send(packet);
        }

        private void SendMessage(PacketType packetType, IWorldClient client, string senderName, string message, bool includeNameFlag = false)
        {
            using var packet = new ImgeneusPacket(packetType);

            if (includeNameFlag)
                packet.Write(false); // false == use sender name, if set to true, sender name will be ignored

            packet.WriteString(senderName, 21);

#if EP8_V2
            packet.WriteByte((byte)(message.Length + 1));
#endif

            packet.WriteByte((byte)message.Length);

#if (EP8_V2 || SHAIYA_US || SHAIYA_US_DEBUG || DEBUG)
            packet.WriteString(message, message.Length, Encoding.Unicode);
#else
            packet.WriteString(message);
#endif
            client.Send(packet);
        }

        #endregion

        #region Guild

        public void SendGuildCreateSuccess(IWorldClient client, uint guildId, byte rank, string guildName, string guildMessage)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_CREATE);
            packet.Write((byte)GuildCreateFailedReason.Success);
            packet.Write(guildId);
            packet.WriteByte(rank);
            packet.WriteString(guildName, 25);
            packet.WriteString(guildMessage, 65);
            client.Send(packet);
        }

        public void SendGuildCreateFailed(IWorldClient client, GuildCreateFailedReason reason)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_CREATE);
            packet.Write((byte)reason);
            client.Send(packet);
        }

        public void SendGuildCreateRequest(IWorldClient client, uint creatorId, string guildName, string guildMessage)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_CREATE_AGREE);
            packet.Write(creatorId);
            packet.WriteString(guildName, 25);
            packet.WriteString(guildMessage, 65);
            client.Send(packet);
        }

        public void SendGuildMemberIsOnline(IWorldClient client, uint playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(104);
            packet.Write(playerId);
            client.Send(packet);
        }

        public void SendGuildMemberIsOffline(IWorldClient client, uint playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(105);
            packet.Write(playerId);
            client.Send(packet);
        }

        public void SendGuildList(IWorldClient client, DbGuild[] guilds)
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

        public void SendGuildMembersOnline(IWorldClient client, List<DbCharacter> members, bool online)
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

        public void SendGuildJoinRequest(IWorldClient client, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_REQUEST);
            packet.Write(ok);
            client.Send(packet);
        }

        public void SendGuildJoinRequestAdd(IWorldClient client, uint playerId, ushort level, CharacterProfession job, string name)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_LIST_ADD);
            packet.Write(new GuildJoinUserUnit(playerId, level, job, name).Serialize());
            client.Send(packet);
        }

        public void SendGuildJoinRequestRemove(IWorldClient client, uint playerId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_LIST_REMOVE);
            packet.Write(playerId);
            client.Send(packet);
        }

        public void SendGuildJoinResult(IWorldClient client, bool ok, uint guildId = 0, byte rank = 9, string name = "")
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_JOIN_RESULT_USER);
            packet.Write(ok);
            packet.Write(guildId);
            packet.Write(rank);
            packet.WriteString(name, 25);
            client.Send(packet);
        }

        public void SendGuildUserListAdd(IWorldClient client, DbCharacter character, bool online)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_LIST_ADD);
            packet.Write(online);
            packet.Write(new GuildUserUnit(character).Serialize());
            client.Send(packet);
        }

        public void SendGuildKickMember(IWorldClient client, bool ok, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_KICK);
            packet.Write(ok);
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendGuildMemberRemove(IWorldClient client, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(103);
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendGuildUserChangeRank(IWorldClient client, uint characterId, byte rank)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte((byte)(rank + 200));
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendGuildMemberLeaveResult(IWorldClient client, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_LEAVE);
            packet.Write(ok);
            client.Send(packet);
        }

        public void SendGuildMemberLeave(IWorldClient client, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(102);
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendGuildDismantle(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_USER_STATE);
            packet.WriteByte(101);
            packet.Write(0);
            client.Send(packet);
        }

        public void SendGuildListAdd(IWorldClient client, DbGuild guild)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_LIST_ADD);
            packet.Write(new GuildUnit(guild).Serialize());
            client.Send(packet);
        }

        public void SendGuildListRemove(IWorldClient client, uint guildId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_LIST_REMOVE);
            packet.Write(guildId);
            client.Send(packet);
        }

        public void SendGuildHouseActionError(IWorldClient client, GuildHouseActionError error, byte rank)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_HOUSE_ACTION_ERR);
            packet.Write((byte)error);
            packet.Write(rank);
            client.Send(packet);
        }

        public void SendGuildHouseBuy(IWorldClient client, GuildHouseBuyReason reason, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_HOUSE_BUY);
            packet.Write((byte)reason);
            packet.Write(gold);
            client.Send(packet);
        }

        public void SendGetEtin(IWorldClient client, int etin)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_GET_ETIN);
            packet.Write(etin);
            client.Send(packet);
        }

        public void SendEtinReturnResult(IWorldClient client, IList<Item> etins)
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

        public void SendGuildUpgradeNpc(IWorldClient client, GuildNpcUpgradeReason reason, NpcType npcType, byte npcGroup, byte npcLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_NPC_UPGRADE);
            packet.Write((byte)reason);
            packet.Write((byte)npcType);
            packet.Write(npcGroup);
            packet.Write(npcLevel);
            packet.WriteByte(0); // TODO: number? what is it?!
            client.Send(packet);
        }

        public void SendGuildNpcs(IWorldClient client, IEnumerable<DbGuildNpcLvl> npcs)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_NPC_LIST);
            packet.Write(new GuildNpcList(npcs).Serialize());
            client.Send(packet);
        }

        public void SendGRBNotice(IWorldClient client, GRBNotice notice)
        {
            using var packet = new ImgeneusPacket(PacketType.GRB_NOTICE);
            packet.Write((ushort)50); // GRB map is always 50. Technically this doesn't really matter, because it's not used anywhere...
            packet.Write((byte)notice);
            client.Send(packet);
        }

        public void SendGBRPoints(IWorldClient client, int currentPoints, int maxPoints, uint topGuild)
        {
            using var packet = new ImgeneusPacket(PacketType.GRB_POINTS);
            packet.Write(currentPoints);
            packet.Write(maxPoints);
            packet.Write(topGuild);
            client.Send(packet);
        }

        public void SendGuildRanksCalculated(IWorldClient client, IEnumerable<(uint GuildId, int Points, byte Rank)> results)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_RANK_UPDATE);
            packet.Write(new GuildRankUpdate(results).Serialize());
            client.Send(packet);
        }
        #endregion

        #region Bank

        public void SendBankItemClaim(IWorldClient client, byte bankSlot, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.BANK_CLAIM_ITEM);
            packet.Write(new BankItemClaim(bankSlot, item).Serialize());
            client.Send(packet);
        }

        public void SendBankItems(IWorldClient client, ICollection<BankItem> bankItems)
        {
            using var packet = new ImgeneusPacket(PacketType.BANK_ITEM_LIST);
            packet.Write(new BankItemList(bankItems).Serialize());
            client.Send(packet);
        }

        #endregion

        #region Warehouse

        public void SendWarehouseItems(IWorldClient client, IReadOnlyCollection<Item> items)
        {
            var steps = items.Count / 50;
            var left = items.Count % 50;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 50;
                var length = i == steps ? left : 50;
                var endIndex = startIndex + length;

                using var packet = new ImgeneusPacket(PacketType.WAREHOUSE_ITEM_LIST);
                packet.Write(0); // gold
                packet.Write(new WarehouseItems(items.Take(startIndex..endIndex)).Serialize());
                client.Send(packet);
            }
        }

        public void SendGuildWarehouseItems(IWorldClient client, ICollection<DbGuildWarehouseItem> items)
        {
            var steps = items.Count / 50;
            var left = items.Count % 50;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 50;
                var length = i == steps ? left : 50;
                var endIndex = startIndex + length;

                using var packet = new ImgeneusPacket(PacketType.GUILD_WAREHOUSE_ITEM_LIST);
                packet.Write(new GuildWarehouseItems(items.Take(startIndex..endIndex)).Serialize());
                client.Send(packet);
            }
        }

        public void SendGuildWarehouseItemAdd(IWorldClient client, DbGuildWarehouseItem item, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_WAREHOUSE_ITEM_ADD);
            packet.Write(new GuildWarehouseItem(item).Serialize());
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendGuildWarehouseItemRemove(IWorldClient client, DbGuildWarehouseItem item, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.GUILD_WAREHOUSE_ITEM_REMOVE);
            packet.Write(new GuildWarehouseItem(item).Serialize());
            packet.Write(characterId);
            client.Send(packet);
        }

        #endregion

        #region Teleport

        public void SendTeleportViaNpc(IWorldClient client, NpcTeleportNotAllowedReason reason, uint money)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_TELEPORT_VIA_NPC);
            packet.Write((byte)reason);
            packet.Write(money);
            client.Send(packet);
        }

        public void SendTeleportSavedPosition(IWorldClient client, bool success, byte index, ushort mapId, float x, float y, float z)
        {
            using var packet = new ImgeneusPacket(PacketType.TELEPORT_SAVE_POSITION);
            packet.Write(success ? (byte)0 : (byte)1);
            packet.Write(index);
            packet.Write(mapId);
            packet.Write(x);
            packet.Write(y);
            packet.Write(z);
            client.Send(packet);
        }

        public void SendTeleportSavedPositions(IWorldClient client, IReadOnlyDictionary<byte, (ushort MapId, float X, float Y, float Z)> positions)
        {
            using var packet = new ImgeneusPacket(PacketType.TELEPORT_SAVE_POSITION_LIST);
            packet.Write((byte)positions.Count);

            foreach (var p in positions)
            {
                packet.Write(p.Key);
                packet.Write(p.Value.MapId);
                packet.Write(p.Value.X);
                packet.Write(p.Value.Y);
                packet.Write(p.Value.Z);
            }

            client.Send(packet);
        }

        public void SendTeleportPreloadedArea(IWorldClient client, bool success)
        {
            using var packet = new ImgeneusPacket(PacketType.TELEPORT_PRELOADED_AREA);
            packet.Write(success);
            client.Send(packet);
        }

        public void SendTeleportToBattleground(IWorldClient client, bool success)
        {
            using var packet = new ImgeneusPacket(PacketType.TELEPORT_TO_BATTLEGROUND);
            packet.Write(success);
            client.Send(packet);
        }

        #endregion

        #region Quests

        public void SendOpenQuests(IWorldClient client, IEnumerable<Quest> quests)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_LIST);
            packet.Write(new CharacterQuests(quests).Serialize());
            client.Send(packet);
        }

        public void SendQuestStarted(IWorldClient client, uint npcId, short questId)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_START);
            packet.Write(npcId);
            packet.Write(questId);
            client.Send(packet);
        }

        public void SendQuestFinished(IWorldClient client, uint npcId, short questId, Quest quest, bool success)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_END);
            packet.Write(npcId);
            packet.Write(questId);
            packet.Write(success);
            packet.Write(quest.Config.ResultType);
            packet.Write(success ? quest.XP : 0);
            packet.Write(success ? quest.Gold : 0);
            packet.WriteByte(0); // bag
            packet.WriteByte(0); // slot
            packet.WriteByte(0); // item type
            packet.WriteByte(0); // item id
            client.Send(packet);
        }

        public void SendQuestChooseRevard(IWorldClient client, short questId)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_END_SELECT);
            packet.Write(questId);
            packet.Write(0); // probably npc id?
            client.Send(packet);
        }

        public void SendQuestCountUpdate(IWorldClient client, short questId, byte index, byte count)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_UPDATE_COUNT);
            packet.Write(questId);
            packet.Write(index);
            packet.Write(count);
            client.Send(packet);
        }

        public void SendFinishedQuests(IWorldClient client, IEnumerable<Quest> quests)
        {
            using var packet = new ImgeneusPacket(PacketType.QUEST_FINISHED_LIST);
            packet.Write(new CharacterFinishedQuests(quests).Serialize());
            client.Send(packet);
        }

        #endregion

        #region Bless

        public void SendBlessAmount(IWorldClient client, CountryType country, int amount, uint remainingTime)
        {
            using var packet = new ImgeneusPacket(PacketType.BLESS_INIT);
            packet.Write((byte)country);

            packet.Write(amount);
            packet.Write(remainingTime);

            client.Send(packet);
        }

        public void SendBlessUpdate(IWorldClient client, CountryType country, int amount)
        {
            using var packet = new ImgeneusPacket(PacketType.BLESS_UPDATE);
            packet.Write((byte)country);
            packet.Write(amount);

            client.Send(packet);
        }

        #endregion

        #region Obelisks

        public void SendObelisks(IWorldClient client, IEnumerable<Obelisk> obelisks)
        {
            using var packet = new ImgeneusPacket(PacketType.OBELISK_LIST);
            packet.Write(new ObeliskList(obelisks).Serialize());
            client.Send(packet);
        }

        public void SendObeliskBroken(IWorldClient client, Obelisk obelisk)
        {
            using var packet = new ImgeneusPacket(PacketType.OBELISK_CHANGE);
            packet.Write(obelisk.Id);
            packet.Write((byte)obelisk.ObeliskCountry);
            client.Send(packet);
        }

        #endregion

        #region Leveling

        public void SendExperienceGain(IWorldClient client, uint exp)
        {
            using var packet = new ImgeneusPacket(PacketType.EXPERIENCE_GAIN);
            packet.Write(new CharacterExperienceGain(exp).Serialize());
            client.Send(packet);
        }

        public void SendLevelUp(IWorldClient client, PacketType type, uint characterId, ushort level, ushort statPoint, ushort skillPoint, uint minExp, uint nextExp)
        {
            using var packet = new ImgeneusPacket(type);
            packet.Write(new CharacterLevelUp(characterId, level, statPoint, skillPoint, minExp, nextExp).Serialize());
            client.Send(packet);
        }

        #endregion

        #region Death

        public void SendCharacterKilled(IWorldClient client, uint characterId, IKiller killer)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_DEATH);
            packet.Write(characterId);
            packet.WriteByte(1); // killer type. 1 - another player.
            packet.Write(killer.Id);
            client.Send(packet);
        }

        public void SendDeadRebirth(IWorldClient client, Character sender)
        {
            using var packet = new ImgeneusPacket(PacketType.DEAD_REBIRTH);
            packet.Write(sender.Id);
            packet.Write((byte)RebirthType.KillSoulByItem);
            packet.Write(sender.LevelingManager.Exp / 10);
            packet.Write(sender.PosX);
            packet.Write(sender.PosY);
            packet.Write(sender.PosZ);
            client.Send(packet);
        }

        public void SendCharacterRebirth(IWorldClient client, uint senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_LEAVE_DEAD);
            packet.Write(senderId);
            client.Send(packet);
        }

        #endregion

        #region Summon

        public void SendItemCasting(IWorldClient client, uint senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.ITEM_CASTING);
            packet.Write(senderId);
            client.Send(packet);
        }

        public void SendPartycallRequest(IWorldClient client, uint senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_CALL_REQUEST);
            packet.Write(senderId);
            client.Send(packet);
        }

        public void SendSummonAnswer(IWorldClient client, uint senderId, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_CALL_ANSWER);
            packet.Write(senderId);
            packet.Write(ok);
            client.Send(packet);
        }

        #endregion

        #region Shop

        public void SendMyShopBegin(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_BEGIN);
            client.Send(packet);
        }

        public void SendMyShopAddItem(IWorldClient client, byte bag, byte slot, byte shopSlot, uint price)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_ADD_ITEM);
            packet.Write(bag);
            packet.Write(slot);
            packet.Write(shopSlot);
            packet.Write(price);
            client.Send(packet);
        }

        public void SendMyShopRemoveItem(IWorldClient client, byte shopSlot)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_REMOVE_ITEM);
            packet.Write(shopSlot);
            client.Send(packet);
        }

        public void SendMyShopStarted(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_START);
            client.Send(packet);
        }

        public void SendMyShopCanceled(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_CANCEL);
            client.Send(packet);
        }

        public void SendMyShopEnded(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_END);
            client.Send(packet);
        }

        public void SendMyShopStarted(IWorldClient client, uint senderId, string shopName)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_INFO);
            packet.Write(senderId);
            packet.Write(true); // Is open?
            packet.WriteByte((byte)(shopName.Length + 1));

#if (EP8_V2 || SHAIYA_US || SHAIYA_US_DEBUG || DEBUG)
            packet.WriteString(shopName, shopName.Length + 1, Encoding.Unicode);
#else
            packet.WriteString(shopName, shopName.Length + 1);
#endif
            client.Send(packet);
        }

        public void SendMyShopFinished(IWorldClient client, uint senderId)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_INFO);
            packet.Write(senderId);
            packet.Write(false); // Is open?
            client.Send(packet);
        }

        public void SendMyShopVisit(IWorldClient client, bool ok, uint characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_VISIT);
            packet.Write(ok);
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendMyShopLeave(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_LEAVE);
            client.Send(packet);
        }

        public void SendMyShopItems(IWorldClient client, IReadOnlyDictionary<byte, Item> items)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_ITEM_LIST);
            packet.Write(new MyShopItems(items).Serialize());
            client.Send(packet);
        }

        public void SendUseShopClosed(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_USE_STOP);
            client.Send(packet);
        }

        public void SendMyShopBuyItemFailed(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_BUY_ITEM);
            packet.Write(false);
            client.Send(packet);
        }

        public void SendMyShopBuyItemSuccess(IWorldClient client, uint money, byte shopSlot, byte shopCount, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_BUY_ITEM);
            packet.Write(true);
            packet.Write(money);
            packet.Write(shopSlot);
            packet.Write(shopCount);
            packet.Write(new SoldItem(item).Serialize());
            client.Send(packet);
        }

        public void SendUseShopItemCountChanged(IWorldClient client, byte slot, byte count)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_USE_SHOP_ITEM_COUNT);
            packet.Write(slot);
            packet.Write(count);
            client.Send(packet);
        }

        public void SendMyShopSoldItem(IWorldClient client, byte slot, byte count, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.MY_SHOP_SOLD_ITEM);
            packet.Write(slot);
            packet.Write(count);
            packet.Write(gold);
            client.Send(packet);
        }

        #endregion

        #region Kill status

        public void SendKillStatusInfo(IWorldClient client, byte killLevel, byte deathLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.KILLSTATUS_RESULT_INFO);
            packet.Write(killLevel);
            packet.Write(deathLevel);
            client.Send(packet);
        }

        public void SendKillsReward(IWorldClient client, bool ok, ushort stats)
        {
            using var packet = new ImgeneusPacket(PacketType.KILLS_GET_REWARD);
            packet.Write(ok);
            packet.Write(stats);
            client.Send(packet);
        }

        public void SendDeathsReward(IWorldClient client, bool ok, uint money)
        {
            using var packet = new ImgeneusPacket(PacketType.DEATHS_GET_REWARD);
            packet.Write(ok);
            packet.Write(money);
            client.Send(packet);
        }

        #endregion

        #region Crafting

        public void SendCraftList(IWorldClient client, CraftInfo config)
        {
            using var packet = new ImgeneusPacket(PacketType.CHAOTIC_SQUARE_LIST);

            for (var i = 0; i < 10; i++)
            {
                if (config.Recipes.Count > i)
                    packet.WriteByte(config.Recipes[i].Type);
                else
                    packet.WriteByte(0);
            }

            for (var i = 0; i < 10; i++)
            {
                if (config.Recipes.Count > i)
                    packet.WriteByte(config.Recipes[i].TypeId);
                else
                    packet.WriteByte(0);
            }

            for (var i = 0; i < 10; i++)
            {
                if (config.Recipes.Count > i)
                    packet.WriteByte(config.Recipes[i].Count);
                else
                    packet.WriteByte(0);
            }

            client.Send(packet);
        }

        public void SendCraftRecipe(IWorldClient client, Recipe recipe)
        {
            using var packet = new ImgeneusPacket(PacketType.CHAOTIC_SQUARE_RECIPE);
            packet.Write((int)(recipe.Rate * 100));

            for (var i = 0; i < 24; i++)
            {
                if (recipe.Ingredients.Count > i)
                    packet.WriteByte(recipe.Ingredients[i].Type);
                else
                    packet.WriteByte(0);
            }

            packet.WriteByte(recipe.Type);

            for (var i = 0; i < 24; i++)
            {
                if (recipe.Ingredients.Count > i)
                    packet.WriteByte(recipe.Ingredients[i].TypeId);
                else
                    packet.WriteByte(0);
            }

            packet.WriteByte(recipe.TypeId);

            for (var i = 0; i < 24; i++)
            {
                if (recipe.Ingredients.Count > i)
                    packet.WriteByte(recipe.Ingredients[i].Count);
                else
                    packet.WriteByte(0);
            }

            packet.WriteByte(recipe.Count);

            client.Send(packet);
        }

        public void SendCraftSuccess(IWorldClient client, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.CHAOTIC_SQUARE_CREATE_2);
            packet.Write(ok ? (byte)1: (byte)2);
            client.Send(packet);
        }

        #endregion

        #region Market

        public void SendMarketSellList(IWorldClient client, IList<DbMarket> items)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_GET_SELL_LIST);
            packet.WriteByte((byte)items.Count); // count
            foreach (var itm in items)
                packet.Write(new MarketSellItem(itm).Serialize());
            client.Send(packet);
        }

        public void SendMarketTenderList(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_GET_TENDER_LIST);
            packet.WriteByte(2); // count
            //packet.Write(new MarketItem(1).Serialize());
            //packet.Write(new MarketItem(2).Serialize());
            client.Send(packet);
        }

        public void SendMarketItemRegister(IWorldClient client, bool ok, DbMarket marketItem, Item item, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_REGISTER_ITEM);
            packet.Write(ok ? (byte)0 : (byte)1);

            if (ok)
            {
                packet.Write(item.Bag);
                packet.Write(item.Slot);
                packet.Write(marketItem.MarketItem.Count);
                packet.Write(gold);
                packet.Write(new MarketItem(marketItem).Serialize());
            }
            client.Send(packet);
        }

        public void SendMarketItemUnregister(IWorldClient client, bool ok, DbMarketCharacterResultItems result)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_UNREGISTER_ITEM);
            packet.Write(ok ? (byte)0 : (byte)1);

            if (ok)
                packet.Write(new MarketEndItem(result).Serialize());
            client.Send(packet);
        }

        public void SendMarketEndItems(IWorldClient client, IList<DbMarketCharacterResultItems> items)
        {
            var steps = items.Count / 10;
            var left = items.Count % 10;

            for (var i = 0; i <= steps; i++)
            {
                var startIndex = i * 10;
                var length = i == steps ? left : 10;
                var endIndex = startIndex + length;

                var part = items.Take(startIndex..endIndex);

                using var packet = new ImgeneusPacket(PacketType.MARKET_GET_END_ITEM_LIST);
                packet.WriteByte((byte)(length));
                foreach (var x in part)
                    packet.Write(new MarketEndItem(x).Serialize());
                    
                client.Send(packet);
            }
        }

        public void SendMarketGetItem(IWorldClient client, bool ok, uint marketId, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_GET_ITEM);
            packet.Write(ok ? (byte)0 : (byte)1);

            if (ok)
            {
                packet.Write(marketId);
                packet.Write(new MarketGetItem(item).Serialize());
            }
            client.Send(packet);
        }

        public void SendMarketSearchSection(IWorldClient client, byte prevCursor, byte nextCursor, IList<DbMarket> results)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_SEARCH_SECTION);
            packet.Write(prevCursor);
            packet.Write(nextCursor);
            packet.Write((byte)results.Count);
            foreach (var itm in results)
                packet.Write(new MarketItem(itm).Serialize());
            client.Send(packet);
        }

        public void SendMarketDirectBuy(IWorldClient client, MarketBuyItemResult ok, uint gold, DbMarketCharacterResultItems item)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_DIRECT_BUY);
            packet.Write((byte)ok);
            if (ok == MarketBuyItemResult.Ok)
            {
                packet.Write(gold);
                packet.Write(new MarketEndItem(item).Serialize());
            }
            client.Send(packet);
        }

        public void SendMarketEndMoney(IWorldClient client, IList<DbMarketCharacterResultMoney> items)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_GET_END_MONEY_LIST);
            packet.Write((byte)items.Count);
            foreach (var itm in items)
                packet.Write(new MarketEndMoney(itm).Serialize());
            client.Send(packet);
        }

        public void SendMarketGetMoney(IWorldClient client, bool ok, uint moneyId, uint gold)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_GET_MONEY);
            packet.Write(ok ? (byte)0 : (byte)1);

            if (ok)
            {
                packet.Write(moneyId);
                packet.Write(gold);
            }
            client.Send(packet);
        }

        public void SendMarketAddFavorite(IWorldClient client, MarketAddFavoriteResult ok, DbMarket item)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_ADD_CONCERT_MARKET);
            packet.Write((byte)ok);
            if (ok == MarketAddFavoriteResult.Ok)
            {
                packet.Write(new MarketItem(item).Serialize());
            }
            client.Send(packet);
        }

        public void SendMarketFavorites(IWorldClient client, IList<DbMarketCharacterFavorite> results)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_GET_CONCERN_LIST);
            packet.Write((byte)results.Count);
            foreach (var item in results)
                packet.Write(new MarketItem(item.Market).Serialize());
            client.Send(packet);
        }

        public void SendMarketRemoveFavorite(IWorldClient client, bool ok, uint marketId)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_REMOVE_CONCERT_MARKET);
            packet.Write(marketId);
            packet.Write(ok ? (byte)0 : (byte)1);
            client.Send(packet);
        }

        public void SendMarketRemoveAllFavorite(IWorldClient client, bool ok)
        {
            using var packet = new ImgeneusPacket(PacketType.MARKET_CONCERT_REMOVE_ALL);
            packet.Write(ok);
            client.Send(packet);
        }

        #endregion

        #region GM
        public void SendGmCommandSuccess(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_CMD_ERROR);
            packet.Write<ushort>(0); // 0 == no error
            client.Send(packet);
        }
        public void SendGmCommandError(IWorldClient client, PacketType error)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_CMD_ERROR);
            packet.Write((ushort)error);
            client.Send(packet);
        }

        public void SendCharacterPosition(IWorldClient client, Character player)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_FIND_PLAYER);
            packet.Write(player.MapProvider.Map.Id);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }

        public void SendGmTeleportToPlayer(IWorldClient client, Character player)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_TELEPORT_TO_PLAYER);
            packet.Write(player.Id);
            packet.Write(player.MapProvider.NextMapId);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }

        public void SendGmSummon(IWorldClient client, uint senderId, ushort mapId, float x, float y, float z)
        {
#if DEBUG || SHAIYA_US || SHAIYA_US_DEBUG
            using var packet = new ImgeneusPacket(PacketType.GM_SHAIYA_US_SUMMON_PLAYER);
#else
            using var packet = new ImgeneusPacket(PacketType.GM_SUMMON_PLAYER);
#endif
            packet.Write(senderId);
            packet.Write(mapId);
            packet.Write(x);
            packet.Write(y);
            packet.Write(z);
            client.Send(packet);
        }

        public void SendWarning(IWorldClient client, string message)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_WARNING_PLAYER);
            packet.WriteByte((byte)(message.Length + 1));
            packet.Write(message);
            packet.WriteByte(0);
            client.Send(packet);
        }

        public void SendGmClearInventory(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_CLEAR_INVENTORY);
            client.Send(packet);
        }

        public void SendGmClearEquipment(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_CLEAR_EQUIPMENT);
            client.Send(packet);
        }

        public void SendGmMutedChat(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_MUTE_PLAYER);
            client.Send(packet);
        }

        public void SendGmUnmutedChat(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_UNMUTE_PLAYER);
            client.Send(packet);
        }

        public void SendGmStopOn(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_STOP_ON);
            client.Send(packet);
        }

        public void SendGmStopOff(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GM_STOP_OFF);
            client.Send(packet);
        }

        #endregion

        #region Other

        public void SendWorldDay(IWorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.WORLD_DAY);
            packet.Write(new DateTime(2020, 01, 01, 12, 30, 00).ToShaiyaTime());
            client.Send(packet);
        }

        public void SendRunMode(IWorldClient client, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.RUN_MODE);
            packet.Write((byte)motion);
            client.Send(packet);
        }

        public void SendAccountPoints(IWorldClient client, uint points)
        {
            using var packet = new ImgeneusPacket(PacketType.ACCOUNT_POINTS);
            packet.Write(new AccountPoints(points).Serialize());
            client.Send(packet);
        }

        public void SendScoutingInfo(IWorldClient client, Element defenceElement, ushort level, Mode grow)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SCOUTING_INFO);
            packet.Write((int)defenceElement);
            packet.Write(level);
            packet.Write((byte)grow);
            client.Send(packet);
        }

        public void SendCashPoint(IWorldClient client, uint points)
        {
            using var packet = new ImgeneusPacket(PacketType.CASH_POINT);
            packet.Write(points);
            client.Send(packet);
        }

        #endregion
    }
}