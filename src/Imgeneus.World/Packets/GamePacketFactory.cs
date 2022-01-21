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
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Shape;

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

        public void SendCharacterSelected(IWorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.SELECT_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendDeletedCharacter(IWorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.DELETE_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendRestoredCharacter(IWorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.RESTORE_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendRenamedCharacter(IWorldClient client, bool ok, int id)
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


        public void SendAttribute(IWorldClient client, CharacterAttributeEnum attribute, uint attributeValue)
        {
#if SHAIYA_US || DEBUG
            using var packet = new ImgeneusPacket(PacketType.GM_SHAIYA_US_ATTRIBUTE_SET);
#else
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_ATTRIBUTE_SET);
#endif

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

        public void SendMoveItem(IWorldClient client, Item sourceItem, Item destinationItem)
        {
            using var packet = new ImgeneusPacket(PacketType.INVENTORY_MOVE_ITEM);

#if EP8_V2 || SHAIYA_US || DEBUG
            packet.Write(0); // Unknown int in V2.
#endif
            packet.Write(new MovedItem(sourceItem).Serialize());
            packet.Write(new MovedItem(destinationItem).Serialize());

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

        public void SendCanNotUseItem(IWorldClient client, int characterId)
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
        #endregion

        #region Vehicle

        public void SendUseVehicle(IWorldClient client, bool ok, bool isOnVehicle)
        {
            using var packet = new ImgeneusPacket(PacketType.USE_VEHICLE);
            packet.Write(ok);
            packet.Write(isOnVehicle);
            client.Send(packet);
        }

        #endregion

        #region Map
        public void SendCharacterMotion(IWorldClient client, int characterId, Motion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MOTION);
            packet.Write(characterId);
            packet.WriteByte((byte)motion);
            client.Send(packet);
        }

        public void SendCharacterChangedEquipment(IWorldClient client, int characterId, Item equipmentItem, byte slot)
        {
            using var packet = new ImgeneusPacket(PacketType.SEND_EQUIPMENT);
            packet.Write(new CharacterEquipmentChange(characterId, slot, equipmentItem).Serialize());
            client.Send(packet);
        }

        public void SendCharacterShape(IWorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE);
            packet.Write(new CharacterShape(character).Serialize());
            client.Send(packet);
        }

        public void SendShapeUpdate(IWorldClient client, int senderId, ShapeEnum shape, int? param1 = null, int? param2 = null)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SHAPE_UPDATE);
            packet.Write(senderId);
            packet.Write((byte)shape);

            // Only for ep 8. Type & TypeId for new mounts.
            if (param1 != null && param2 != null)
            {
                packet.Write((int)param1);
                packet.Write((int)param2);
            }

            client.Send(packet);
        }

        public void SendMaxHitpoints(IWorldClient client, int characterId, HitpointType type, int value)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_MAX_HITPOINTS);
            packet.Write(new MaxHitpoint(characterId, type, value).Serialize());
            client.Send(packet);
        }

        public void SendRecoverCharacter(IWorldClient client, int characterId, int hp, int mp, int sp)
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

        public void SendMobRecover(IWorldClient client, int mobId, int hp)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_RECOVER);
            packet.Write(mobId);
            packet.Write(hp);
            client.Send(packet);
        }

        public void SendAppearanceChanged(IWorldClient client, int characterId, byte hair, byte face, byte size, byte gender)
        {
            using var packet = new ImgeneusPacket(PacketType.CHANGE_APPEARANCE);
            packet.Write(characterId);
            packet.Write(hair);
            packet.Write(face);
            packet.Write(size);
            packet.Write(gender);
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
        #endregion
    }
}