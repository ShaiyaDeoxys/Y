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

        #region Party

        public void SendPartyRequest(IWorldClient client, int requesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_REQUEST);
            packet.Write(requesterId);
            client.Send(packet);
        }

        public void SendDeclineParty(IWorldClient client, int charId)
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

        public void SendAddPartyBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_ADDED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendRemovePartyBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_REMOVED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendPartySingle_HP_SP_MP(IWorldClient client, int id, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_CHARACTER_SP_MP);
            packet.Write(id);
            packet.Write(type);
            packet.Write(value);
            client.Send(packet);
        }

        public void SendPartySingle_Max_HP_SP_MP(IWorldClient client, int id, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_SET_MAX);
            packet.Write(id);
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

        public void SendPartyMemberGetItem(IWorldClient client, int characterId, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_MEMBER_GET_ITEM);
            packet.Write(characterId);
            packet.Write(item.Type);
            packet.Write(item.TypeId);
            client.Send(packet);
        }

        public void SendPartyLevel(IWorldClient client, Character sender)
        {
            using var packet = new ImgeneusPacket(PacketType.PARTY_MEMBER_LEVEL);
            packet.Write(new PartyMemberLevelChange(sender).Serialize());
            client.Send(packet);
        }

        public void SendPartyError(IWorldClient client, PartyErrorType partyError, int id = 0)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_PARTY_ERROR);
            packet.Write((int)partyError);
            packet.Write(id);
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

        public void SendAddRaidBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_ADDED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendRemoveRaidBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_REMOVED_BUFF);
            packet.Write(senderId);
            packet.Write(skillId);
            packet.Write(skillLevel);
            client.Send(packet);
        }

        public void SendRaid_Single_HP_SP_MP(IWorldClient client, int id, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_CHARACTER_SP_MP);
            packet.Write(id);
            packet.Write(type);
            packet.Write(value);
            client.Send(packet);
        }

        public void SendRaid_Single_Max_HP_SP_MP(IWorldClient client, int id, int value, byte type)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_SET_MAX);
            packet.Write(id);
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

        public void SendMemberGetItem(IWorldClient client, int characterId, Item item)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_MEMBER_GET_ITEM);
            packet.Write(characterId);
            packet.Write(item.Type);
            packet.Write(item.TypeId);
            client.Send(packet);
        }

        public void SendRaidInvite(IWorldClient client, int requesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_INVITE);
            packet.Write(requesterId);
            client.Send(packet);
        }

        public void SendDeclineRaid(IWorldClient client, int charId)
        {
            using var packet = new ImgeneusPacket(PacketType.RAID_RESPONSE);
            packet.Write(false);
            packet.Write(charId);
            client.Send(packet);
        }
        #endregion

        #region Trade

        public void SendTradeRequest(IWorldClient client, int tradeRequesterId)
        {
            using var packet = new ImgeneusPacket(PacketType.TRADE_REQUEST);
            packet.Write(tradeRequesterId);
            client.Send(packet);
        }

        public void SendTradeStart(IWorldClient client, int traderId)
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

        public void SendMobInTarget(IWorldClient client, Mob target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_SELECT_MOB);
            packet.Write(new MobInTarget(target).Serialize());
            client.Send(packet);
        }

        public void SendPlayerInTarget(IWorldClient client, Character target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_SELECT_CHARACTER);
            packet.Write(new CharacterInTarget(target).Serialize());
            client.Send(packet);
        }

        public void SendCurrentBuffs(IWorldClient client, IKillable target)
        {
            using var packet = new ImgeneusPacket(PacketType.TARGET_BUFFS);
            packet.Write(new TargetBuffs(target).Serialize());
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

        public void SendAutoAttackFailed(IWorldClient client, int senderId, IKillable target, AttackSuccess reason)
        {
            var type = target is Character ? PacketType.CHARACTER_CHARACTER_AUTO_ATTACK : PacketType.CHARACTER_MOB_AUTO_ATTACK;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new UsualAttack(senderId, 0, new AttackResult() { Success = reason }).Serialize());
            client.Send(packet);
        }

        public void SendUseSkillFailed(IWorldClient client, int senderId, Skill skill, IKillable target, AttackSuccess reason)
        {
            var type = target is Character ? PacketType.USE_CHARACTER_TARGET_SKILL : PacketType.USE_MOB_TARGET_SKILL;
            using var packet = new ImgeneusPacket(type);
            packet.Write(new SkillRange(senderId, 0, skill, new AttackResult() { Success = reason }).Serialize());
            client.Send(packet);
        }


        #endregion

        #region Mobs

        public void SendMobPosition(IWorldClient client, int senderId, float x, float z, MoveMotion motion)
        {
            using var packet = new ImgeneusPacket(PacketType.MOB_MOVE);
            packet.Write(new MobMove(senderId, x, z, motion).Serialize());
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

        public void SendFriendDeleted(IWorldClient client, int id)
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

        public void SendFriendOnline(IWorldClient client, int id, bool isOnline)
        {
            using var packet = new ImgeneusPacket(PacketType.FRIEND_ONLINE);
            packet.Write(id);
            packet.Write(isOnline);
            client.Send(packet);
        }

        #endregion

        #region Duel

        public void SendWaitingDuel(IWorldClient client, int duelStarterId, int duelOpponentId)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_REQUEST);
            packet.Write(duelStarterId);
            packet.Write(duelOpponentId);
            client.Send(packet);
        }

        public void SendDuelResponse(IWorldClient client, DuelResponse response, int characterId)
        {
            using var packet = new ImgeneusPacket(PacketType.DUEL_RESPONSE);
            packet.Write((byte)response);
            packet.Write(characterId);
            client.Send(packet);
        }

        public void SendDuelStartTrade(IWorldClient client, int characterId)
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
            packet.Write(isApproved ? (byte)0: (byte)1);
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

        public void SendDuelCancel(IWorldClient client, DuelCancelReason cancelReason, int playerId)
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

        public void SendGmSummon(IWorldClient client, Character player, PacketType type)
        {
            using var packet = new ImgeneusPacket(type);
            packet.Write(player.Id);
            packet.Write(player.MapProvider.NextMapId);
            packet.Write(player.PosX);
            packet.Write(player.PosY);
            packet.Write(player.PosZ);
            client.Send(packet);
        }
        #endregion
    }
}