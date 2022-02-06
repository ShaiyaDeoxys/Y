using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Portals;
using System.Collections.Generic;

namespace Imgeneus.World.Packets
{
    public interface IGamePacketFactory
    {
        #region Handshake
        void SendGameHandshake(IWorldClient IWorldClient);
        void SendLogout(IWorldClient client);
        void SendQuitGame(IWorldClient client);
        #endregion

        #region Selection screen
        void SendCreatedCharacter(IWorldClient client, bool isCreated);
        void SendCheckName(IWorldClient client, bool isAvailable);
        void SendFaction(IWorldClient client, Fraction faction, Mode maxMode);
        void SendCharacterList(IWorldClient client, IEnumerable<DbCharacter> characters);
        void SendCharacterSelected(IWorldClient client, bool ok, int id);
        void SendDeletedCharacter(IWorldClient client, bool ok, int id);
        void SendRestoredCharacter(IWorldClient client, bool ok, int id);
        void SendRenamedCharacter(IWorldClient client, bool ok, int id);
        #endregion

        #region Character
        void SendDetails(IWorldClient client, Character character);
        void SendAdditionalStats(IWorldClient client, Character character);
        void SendSkillBar(IWorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems);
        void SendAttribute(IWorldClient client, CharacterAttributeEnum attribute, uint attributeValue);
        void SendStatsUpdate(IWorldClient client, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc);
        void SendLearnedNewSkill(IWorldClient client, bool ok, Skill skill);
        void SendLearnedSkills(IWorldClient client, Character character);
        void SendActiveBuffs(IWorldClient client, ICollection<Buff> activeBuffs);
        void SendAutoStats(IWorldClient client, byte str, byte dex, byte rec, byte intl, byte wis, byte luc);
        #endregion

        #region Inventory
        void SendInventoryItems(IWorldClient client, ICollection<Item> inventoryItems);
        void SendItemExpiration(IWorldClient client, Item item);
        void SendAddItem(IWorldClient client, Item item);
        void SendMoveItem(IWorldClient client, Item sourceItem, Item destinationItem);
        void SendRemoveItem(IWorldClient client, Item item, bool fullRemove);
        void SendItemDoesNotBelong(IWorldClient client);
        void SendFullInventory(IWorldClient client);
        void SendCanNotUseItem(IWorldClient client, int characterId);
        void SendBoughtItem(IWorldClient client, BuyResult result, Item boughtItem, uint gold);
        void SendSoldItem(IWorldClient client, bool success, Item itemToSell, uint gold);
        #endregion

        #region Vehicle

        void SendUseVehicle(IWorldClient client, bool ok, bool isOnVehicle);

        #endregion

        #region Map
        void SendCharacterMotion(IWorldClient client, int characterId, Motion motion);
        void SendCharacterChangedEquipment(IWorldClient client, int characterId, Item equipmentItem, byte slot);
        void SendCharacterShape(IWorldClient client, Character character);
        void SendShapeUpdate(IWorldClient client, int senderId, ShapeEnum shape, int? param1 = null, int? param2 = null);
        void SendMaxHitpoints(IWorldClient client, int characterId, HitpointType type, int value);
        void SendRecoverCharacter(IWorldClient client, int characterId, int hp, int mp, int sp);
        void SendMobRecover(IWorldClient client, int mobId, int hp);
        void SendAppearanceChanged(IWorldClient client, int characterId, byte hair, byte face, byte size, byte gender);
        void SendPortalTeleportNotAllowed(IWorldClient client, PortalTeleportNotAllowedReason reason);
        void SendWeather(IWorldClient client, Map map);
        #endregion

        #region Linking
        void SendGemPossibility(IWorldClient client, double rate, int gold);
        void SendAddGem(IWorldClient client, bool success, Item gem, Item item, byte slot, uint gold, Item hammer);
        void SendGemRemovePossibility(IWorldClient client, double rate, int gold);
        void SendRemoveGem(IWorldClient client, bool success, Item item, byte slot, List<Item> savedGems, uint gold);
        #endregion

        #region Dyeing

        void SendSelectDyeItem(IWorldClient client, bool success);
        void SendDyeColors(IWorldClient client, IEnumerable<DyeColor> availableColors);
        void SendDyeConfirm(IWorldClient client, bool ok, DyeColor color);

        #endregion

        #region Party
        void SendPartyRequest(IWorldClient client, int requesterId);
        void SendDeclineParty(IWorldClient client, int charId);
        void SendPartyInfo(IWorldClient client, IEnumerable<Character> partyMembers, byte leaderIndex);
        void SendPlayerJoinedParty(IWorldClient client, Character character);
        void SendPlayerLeftParty(IWorldClient client, Character character);
        void SendPartyKickMember(IWorldClient client, Character character);
        void SendRegisteredInPartySearch(IWorldClient client, bool isSuccess);
        void SendPartySearchList(IWorldClient client, IEnumerable<Character> partySearchers);
        void SendNewPartyLeader(IWorldClient client, Character character);
        void SendAddPartyBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel);
        void SendRemovePartyBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel);
        void SendPartySingle_HP_SP_MP(IWorldClient client, int id, int value, byte type);
        void SendPartySingle_Max_HP_SP_MP(IWorldClient client, int id, int value, byte type);
        void SendParty_HP_SP_MP(IWorldClient client, Character partyMember);
        void SendParty_Max_HP_SP_MP(IWorldClient client, Character partyMember);
        void SendPartyMemberGetItem(IWorldClient client, int characterId, Item item);
        void SendPartyLevel(IWorldClient client, Character sender);
        void SendPartyError(IWorldClient client, PartyErrorType partyError, int id = 0);
        void SendRaidDismantle(IWorldClient client);
        #endregion

        #region Raid
        void SendRaidCreated(IWorldClient client, Raid raid);
        void SendRaidInfo(IWorldClient client, Raid raid);
        void SendPlayerJoinedRaid(IWorldClient client, Character character, ushort position);
        void SendPlayerLeftRaid(IWorldClient client, Character character);
        void SendAutoJoinChanged(IWorldClient client, bool autoJoin);
        void SendDropType(IWorldClient client, RaidDropType dropType);
        void SendAddRaidBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel);
        void SendRemoveRaidBuff(IWorldClient client, int senderId, ushort skillId, byte skillLevel);
        void SendRaid_Single_HP_SP_MP(IWorldClient client, int id, int value, byte type);
        void SendRaid_Single_Max_HP_SP_MP(IWorldClient client, int id, int value, byte type);
        void SendRaidNewLeader(IWorldClient client, Character character);
        void SendNewRaidSubLeader(IWorldClient client, Character character);
        void SendRaidKickMember(IWorldClient client, Character character);
        void SendPlayerMove(IWorldClient client, int sourceIndex, int destinationIndex, int leaderIndex, int subLeaderIndex);
        void SendMemberGetItem(IWorldClient client, int characterId, Item item);
        void SendRaidInvite(IWorldClient client, int charId);
        void SendDeclineRaid(IWorldClient client, int charId);
        #endregion

        #region GM
        void SendGmCommandSuccess(IWorldClient client);
        void SendGmCommandError(IWorldClient client, PacketType error);
        void SendCharacterPosition(IWorldClient client, Character player);
        void SendGmTeleportToPlayer(IWorldClient client, Character player);
        void SendGmSummon(IWorldClient client, Character player, PacketType type);
        #endregion
    }
}
