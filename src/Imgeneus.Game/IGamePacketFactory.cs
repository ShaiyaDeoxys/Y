using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Game.Crafting;
using Imgeneus.Game.Market;
using Imgeneus.Game.Skills;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Duel;
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
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Game.Zone.Portals;
using Parsec.Shaiya.NpcQuest;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Quest = Imgeneus.World.Game.Quests.Quest;

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
        void SendCharacterSelected(IWorldClient client, bool ok, uint id);
        void SendDeletedCharacter(IWorldClient client, bool ok, uint id);
        void SendRestoredCharacter(IWorldClient client, bool ok, uint id);
        void SendRenamedCharacter(IWorldClient client, bool ok, uint id);
        #endregion

        #region Character
        void SendDetails(IWorldClient client, Character character);
        void SendAdditionalStats(IWorldClient client, Character character);
        void SendResetStats(IWorldClient client, Character character);
        void SendResetSkills(IWorldClient client, ushort skillPoint);
        void SendSkillBar(IWorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems);
        void SendAttribute(IWorldClient client, CharacterAttributeEnum attribute, uint attributeValue, PacketType packetType);
        void SendStatsUpdate(IWorldClient client, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc);
        void SendLearnedNewSkill(IWorldClient client, bool ok, Skill skill);
        void SendLearnedSkills(IWorldClient client, Character character);
        void SendActiveBuffs(IWorldClient client, ICollection<Buff> activeBuffs);
        void SendAddBuff(IWorldClient client, uint id, ushort skillId, byte skillLevel, int countdown);
        void SendRemoveBuff(IWorldClient client, uint id);
        void SendAutoStats(IWorldClient client, byte str, byte dex, byte rec, byte intl, byte wis, byte luc);
        void SendCurrentHitpoints(IWorldClient client, int hp, int mp, int sp);
        void SendKillCountChanged(IWorldClient client, byte index, uint count);
        #endregion

        #region Inventory
        void SendInventoryItems(IWorldClient client, ICollection<Item> inventoryItems);
        void SendItemExpiration(IWorldClient client, Item item);
        void SendAddItem(IWorldClient client, Item item);
        void SendMoveItem(IWorldClient client, Item sourceItem, Item destinationItem, uint gold = 0);
        void SendRemoveItem(IWorldClient client, Item item, bool fullRemove);
        void SendItemDoesNotBelong(IWorldClient client);
        void SendFullInventory(IWorldClient client);
        void SendCanNotUseItem(IWorldClient client, uint characterId);
        void SendBoughtItem(IWorldClient client, BuyResult result, Item boughtItem, uint gold);
        void SendSoldItem(IWorldClient client, bool success, Item itemToSell, uint gold);
        void SendGoldUpdate(IWorldClient client, uint gold);
        void SendItemExpired(IWorldClient client, Item item, ExpireType expireType);
        void SendInventorySort(IWorldClient client);
        #endregion

        #region Vehicle
        void SendUseVehicle(IWorldClient client, bool ok, bool isOnVehicle);
        void SendVehicleResponse(IWorldClient client, VehicleResponse status);
        void SendVehicleRequest(IWorldClient client, uint requesterId);
        void SendStartSummoningVehicle(IWorldClient client, uint senderId);
        void SendVehiclePassengerChanged(IWorldClient client, uint passengerId, uint vehicleCharId);
        #endregion

        #region Map
        void SendCharacterMotion(IWorldClient client, uint characterId, Motion motion);
        void SendCharacterMoves(IWorldClient client, uint senderId, float x, float y, float z, ushort a, MoveMotion motion);
        void SendCharacterChangedEquipment(IWorldClient client, uint characterId, Item equipmentItem, byte slot);
        void SendCharacterShape(IWorldClient client, uint characterId, Character character);
        void SendShapeUpdate(IWorldClient client, uint senderId, ShapeEnum shape, uint? param1 = null, uint? param2 = null);
        void SendMaxHitpoints(IWorldClient client, uint characterId, HitpointType type, int value);
        void SendRecoverCharacter(IWorldClient client, uint characterId, int hp, int mp, int sp);
        void SendMobRecover(IWorldClient client, uint mobId, int hp);
        void SendAppearanceChanged(IWorldClient client, uint characterId, byte hair, byte face, byte size, byte gender);
        void SendPortalTeleportNotAllowed(IWorldClient client, PortalTeleportNotAllowedReason reason);
        void SendWeather(IWorldClient client, Map map);
        void SendCharacterTeleport(IWorldClient client, uint characterId, ushort mapId, float x, float y, float z, bool teleportedByAdmin);
        void SendCharacterLeave(IWorldClient client, Character character);
        void SendCharacterEnter(IWorldClient client, Character character);
        void SendAttackAndMovementSpeed(IWorldClient client, uint senderId, AttackSpeed attack, MoveSpeed move);
        void SendCharacterUsedSkill(IWorldClient client, uint senderId, IKillable target, Skill skill, AttackResult attackResult);
        void SendAbsorbValue(IWorldClient client, ushort absorb);
        void SendSkillCastStarted(IWorldClient client, uint senderId, IKillable target, Skill skill);
        void SendUsedItem(IWorldClient client, uint senderId, Item item);
        void SendMax_HP_MP_SP(IWorldClient client, Character character);
        void SendSkillKeep(IWorldClient client, uint senderId, ushort skillId, byte skillLevel, AttackResult result);
        void SendUsedRangeSkill(IWorldClient client, uint senderId, IKillable target, Skill skill, AttackResult attackResult);
        void SendAddItem(IWorldClient client, MapItem mapItem);
        void SendRemoveItem(IWorldClient client, MapItem mapItem);
        void SendTransformation(IWorldClient client, uint senderId, bool isTransformed);
        void SendCharacterMirrorDamage(IWorldClient client, uint senderId, uint targetId, Damage damage);
        void SendMobMirrorDamage(IWorldClient client, uint senderId, uint targetId, Damage damage);
        void SendCharacterTargetHP(IWorldClient client, uint targetId, int currentHP, int maxHP, AttackSpeed attackSpeed, MoveSpeed moveSpeed);
        void SendMobTargetHP(IWorldClient client, uint targetId, int currentHP, AttackSpeed attackSpeed, MoveSpeed moveSpeed);
        void SendKillsUpdate(IWorldClient client, uint senderId, uint kills);
        #endregion

        #region NPC
        void SendNpcLeave(IWorldClient client, Npc npc);
        void SendNpcEnter(IWorldClient client, Npc npc);
        void SendNpcMove(IWorldClient client, uint senderId, float x, float y, float z, MoveMotion motion);
        void SendNpcAttack(IWorldClient client, uint senderId, IKillable target, AttackResult result);
        #endregion

        #region Linking
        void SendGemPossibility(IWorldClient client, double rate, int gold);
        void SendAddGem(IWorldClient client, bool success, Item gem, Item item, byte slot, uint gold, Item hammer);
        void SendGemRemovePossibility(IWorldClient client, double rate, int gold);
        void SendRemoveGem(IWorldClient client, bool success, Item item, byte slot, List<Item> savedGems, uint gold);
        #endregion

        #region Composition
        void SendComposition(IWorldClient client, bool ok, Item item);
        void SendAbsoluteComposition(IWorldClient client, bool ok, Item item);
        void SendRuneSynthesize(IWorldClient client, bool success, Item rune);
        #endregion

        #region Dyeing

        void SendSelectDyeItem(IWorldClient client, bool success);
        void SendDyeColors(IWorldClient client, IEnumerable<DyeColor> availableColors);
        void SendDyeConfirm(IWorldClient client, bool ok, DyeColor color);

        #endregion

        #region Enchantment

        void SendEnchantRate(IWorldClient client, IEnumerable<byte> lapisiaBag, IEnumerable<byte> lapisiaSlot, IEnumerable<int> rate, uint gold);
        void SendEnchantAdd(IWorldClient client, bool success, Item lapisia, Item item, uint gold, bool autoEnchant, bool safetyScrollLeft);

        #endregion

        #region Party
        void SendPartyRequest(IWorldClient client, uint requesterId);
        void SendDeclineParty(IWorldClient client, uint charId);
        void SendPartyInfo(IWorldClient client, IEnumerable<Character> partyMembers, byte leaderIndex);
        void SendPlayerJoinedParty(IWorldClient client, Character character);
        void SendPlayerLeftParty(IWorldClient client, Character character);
        void SendPartyKickMember(IWorldClient client, Character character);
        void SendRegisteredInPartySearch(IWorldClient client, bool isSuccess);
        void SendPartySearchList(IWorldClient client, IEnumerable<Character> partySearchers);
        void SendNewPartyLeader(IWorldClient client, Character character);
        void SendAddPartyBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel);
        void SendRemovePartyBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel);
        void SendPartySingle_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type);
        void SendPartySingle_Max_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type);
        void SendParty_HP_SP_MP(IWorldClient client, Character partyMember);
        void SendParty_Max_HP_SP_MP(IWorldClient client, Character partyMember);
        void SendPartyMemberGetItem(IWorldClient client, uint characterId, Item item);
        void SendPartyLevel(IWorldClient client, uint senderId, ushort level);
        void SendPartyError(IWorldClient client, PartyErrorType partyError, int id = 0);
        void SendCharacterPartyChanged(IWorldClient client, uint characterId, PartyMemberType type);
        #endregion

        #region Raid
        void SendRaidCreated(IWorldClient client, Raid raid);
        void SendRaidInfo(IWorldClient client, Raid raid);
        void SendPlayerJoinedRaid(IWorldClient client, Character character, ushort position);
        void SendPlayerLeftRaid(IWorldClient client, Character character);
        void SendAutoJoinChanged(IWorldClient client, bool autoJoin);
        void SendDropType(IWorldClient client, RaidDropType dropType);
        void SendAddRaidBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel);
        void SendRemoveRaidBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel);
        void SendRaid_Single_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type);
        void SendRaid_Single_Max_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type);
        void SendRaidNewLeader(IWorldClient client, Character character);
        void SendNewRaidSubLeader(IWorldClient client, Character character);
        void SendRaidKickMember(IWorldClient client, Character character);
        void SendPlayerMove(IWorldClient client, int sourceIndex, int destinationIndex, int leaderIndex, int subLeaderIndex);
        void SendMemberGetItem(IWorldClient client, uint characterId, Item item);
        void SendRaidInvite(IWorldClient client, uint charId);
        void SendDeclineRaid(IWorldClient client, uint charId);
        void SendRaidDismantle(IWorldClient client);
        #endregion

        #region Trade
        void SendTradeRequest(IWorldClient client, uint tradeRequesterId);
        void SendTradeStart(IWorldClient client, uint traderId);
        void SendAddedItemToTrade(IWorldClient client, byte bag, byte slot, byte quantity, byte slotInTradeWindow);
        void SendAddedItemToTrade(IWorldClient client, Item tradeItem, byte quantity, byte slotInTradeWindow);
        void SendTradeCanceled(IWorldClient client);
        void SendRemovedItemFromTrade(IWorldClient client, byte byWho);
        void SendAddedMoneyToTrade(IWorldClient client, byte byWho, uint tradeMoney);
        void SendTradeDecide(IWorldClient client, byte byWho, bool isDecided);
        void SendTradeConfirm(IWorldClient client, byte byWho, bool isDeclined);
        void SendTradeFinished(IWorldClient client);
        #endregion

        #region Attack
        void SendAutoAttackStop(IWorldClient client);
        void SendPlayerInTarget(IWorldClient client, uint characterId, int maxHp, int hp);
        void SendCurrentBuffs(IWorldClient client, IKillable target);
        void SendTargetAddBuff(IWorldClient client, uint targetId, Buff buff, bool isMob);
        void SendTargetRemoveBuff(IWorldClient client, uint targetId, Buff buff, bool isMob);
        void SendMobState(IWorldClient client, Mob target);
        void SendAutoAttackFailed(IWorldClient client, uint senderId, IKillable target, AttackSuccess reason);
        void SendUseSkillFailed(IWorldClient client, uint senderId, Skill skill, IKillable target, AttackSuccess reason);
        void SendUseSMMP(IWorldClient client, ushort MP, ushort SP);
        void SendCharacterUsualAttack(IWorldClient client, uint senderId, IKillable target, AttackResult attackResult);
        #endregion

        #region Mobs
        void SendMobPosition(IWorldClient client, uint senderId, float x, float z, MoveMotion motion);
        void SendMobEnter(IWorldClient client, Mob mob, bool isNew);
        void SendMobLeave(IWorldClient client, Mob mob);
        void SendMobMove(IWorldClient client, uint senderId, float x, float z, MoveMotion motion);
        void SendMobAttack(IWorldClient client, uint senderId, uint targetId, AttackResult attackResult);
        void SendMobUsedSkill(IWorldClient client, uint senderId, uint targetId, Skill skill, AttackResult attackResult);
        void SendMobUsedRangeSkill(IWorldClient client, uint senderId, uint targetId, Skill skill, AttackResult attackResult);
        void SendMobDead(IWorldClient client, uint senderId, IKiller killer);
        void SendMobSkillKeep(IWorldClient client, uint senderId, ushort skillId, byte skillLevel, AttackResult attackResult);
        #endregion

        #region Friends
        void SendFriendRequest(IWorldClient client, string name);
        void SendFriendResponse(IWorldClient client, bool accepted);
        void SendFriendAdded(IWorldClient client, Friend friend);
        void SendFriendDeleted(IWorldClient client, uint id);
        void SendFriends(IWorldClient client, IEnumerable<Friend> friends);
        void SendFriendOnline(IWorldClient client, uint characterId, bool isOnline);
        #endregion

        #region Duel
        void SendWaitingDuel(IWorldClient client, uint duelStarterId, uint duelOpponentId);
        void SendDuelResponse(IWorldClient client, DuelResponse response, uint characterId);
        void SendDuelStartTrade(IWorldClient client, uint characterId);
        void SendDuelAddItem(IWorldClient client, Item tradeItem, byte quantity, byte slotInTradeWindow);
        void SendDuelAddItem(IWorldClient client, byte bag, byte slot, byte quantity, byte slotInTradeWindow);
        void SendDuelRemoveItem(IWorldClient client, byte slotInTradeWindow, byte senderType);
        void SendDuelAddMoney(IWorldClient client, byte senderType, uint tradeMoney);
        void SendDuelCloseTrade(IWorldClient client, DuelCloseWindowReason reason);
        void SendDuelApprove(IWorldClient client, byte senderType, bool isApproved);
        void SendDuelReady(IWorldClient client, float x, float z);
        void SendDuelStart(IWorldClient client);
        void SendDuelCancel(IWorldClient client, DuelCancelReason cancelReason, uint playerId);
        void SendDuelFinish(IWorldClient client, bool isWin);
        #endregion

        #region Chat
        void SendNormal(IWorldClient client, uint senderId, string message, bool isAdmin);
        void SendWhisper(IWorldClient client, string senderName, string message, bool isAdmin);
        void SendParty(IWorldClient client, uint senderId, string message, bool isAdmin);
        void SendMap(IWorldClient client, string senderName, string message);
        void SendWorld(IWorldClient client, string senderName, string message);
        void SendMessageToServer(IWorldClient client, string senderName, string message);
        void SendGuild(IWorldClient client, string senderName, string message, bool isAdmin);
        #endregion

        #region Guild
        void SendGuildCreateSuccess(IWorldClient client, uint guildId, byte rank, string guildName, string guildMessage);
        void SendGuildCreateFailed(IWorldClient client, GuildCreateFailedReason reason);
        void SendGuildCreateRequest(IWorldClient client, uint creatorId, string guildName, string guildMessage);
        void SendGuildMemberIsOnline(IWorldClient client, uint playerId);
        void SendGuildMemberIsOffline(IWorldClient client, uint playerId);
        void SendGuildList(IWorldClient client, DbGuild[] guilds);
        void SendGuildMembersOnline(IWorldClient client, List<DbCharacter> members, bool online);
        void SendGuildJoinRequest(IWorldClient client, bool ok);
        void SendGuildJoinRequestAdd(IWorldClient client, uint playerId, ushort level, CharacterProfession job, string name);
        void SendGuildJoinRequestRemove(IWorldClient client, uint playerId);
        void SendGuildJoinResult(IWorldClient client, bool ok, uint guildId = 0, byte rank = 9, string name = "");
        void SendGuildUserListAdd(IWorldClient client, DbCharacter character, bool online);
        void SendGuildKickMember(IWorldClient client, bool ok, uint characterId);
        void SendGuildMemberRemove(IWorldClient client, uint characterId);
        void SendGuildUserChangeRank(IWorldClient client, uint characterId, byte rank);
        void SendGuildMemberLeaveResult(IWorldClient client, bool ok);
        void SendGuildMemberLeave(IWorldClient client, uint characterId);
        void SendGuildDismantle(IWorldClient client);
        void SendGuildListAdd(IWorldClient client, DbGuild guild);
        void SendGuildListRemove(IWorldClient client, uint guildId);
        void SendGuildHouseActionError(IWorldClient client, GuildHouseActionError error, byte rank);
        void SendGuildHouseBuy(IWorldClient client, GuildHouseBuyReason reason, uint gold);
        void SendGetEtin(IWorldClient client, int etin);
        void SendEtinReturnResult(IWorldClient client, IList<Item> etins);
        void SendGuildUpgradeNpc(IWorldClient client, GuildNpcUpgradeReason reason, NpcType npcType, byte npcGroup, byte npcLevel);
        void SendGuildNpcs(IWorldClient client, IEnumerable<DbGuildNpcLvl> npcs);
        void SendGRBNotice(IWorldClient client, GRBNotice notice);
        void SendGBRPoints(IWorldClient client, int currentPoints, int maxPoints, uint topGuild);
        void SendGuildRanksCalculated(IWorldClient client, IEnumerable<(uint GuildId, int Points, byte Rank)> results);
        #endregion

        #region Bank
        void SendBankItemClaim(IWorldClient client, byte bankSlot, Item item);
        void SendBankItems(IWorldClient client, ICollection<BankItem> bankItems);
        #endregion

        #region Warehouse
        void SendWarehouseItems(IWorldClient client, IReadOnlyCollection<Item> items);
        void SendGuildWarehouseItems(IWorldClient client, ICollection<DbGuildWarehouseItem> items);
        void SendGuildWarehouseItemAdd(IWorldClient client, DbGuildWarehouseItem item, uint characterId);
        void SendGuildWarehouseItemRemove(IWorldClient client, DbGuildWarehouseItem item, uint characterId);
        #endregion

        #region Teleport
        void SendTeleportViaNpc(IWorldClient client, NpcTeleportNotAllowedReason reason, uint money);
        void SendTeleportSavedPosition(IWorldClient client, bool success, byte index, ushort mapId, float x, float y, float z);
        void SendTeleportSavedPositions(IWorldClient client, IReadOnlyDictionary<byte, (ushort MapId, float X, float Y, float Z)> positions);
        void SendTeleportPreloadedArea(IWorldClient client, bool success);
        void SendTeleportToBattleground(IWorldClient client, bool success);
        #endregion

        #region Quests
        void SendOpenQuests(IWorldClient client, IEnumerable<Quest> quests);
        void SendQuestStarted(IWorldClient client, uint npcId, short questId);
        void SendQuestFinished(IWorldClient client, uint npcId, short questId, Quest quest, bool success);
        void SendQuestChooseRevard(IWorldClient client, short questId);
        void SendQuestCountUpdate(IWorldClient client, short questId, byte index, byte count);
        void SendFinishedQuests(IWorldClient client, IEnumerable<Quest> quests);
        #endregion

        #region Bless
        void SendBlessAmount(IWorldClient client, CountryType country, int amount, uint remainingTime);
        void SendBlessUpdate(IWorldClient client, CountryType country, int amount);
        #endregion

        #region Obelisks
        void SendObelisks(IWorldClient client, IEnumerable<Obelisk> obelisks);
        void SendObeliskBroken(IWorldClient client, Obelisk obelisk);
        #endregion

        #region Leveling
        void SendExperienceGain(IWorldClient client, uint exp);
        void SendLevelUp(IWorldClient client, PacketType type, uint characterId, ushort level, ushort statPoint, ushort skillPoint, uint minExp, uint nextExp);
        #endregion

        #region Death
        void SendCharacterKilled(IWorldClient client, uint characterId, IKiller killer);
        void SendDeadRebirth(IWorldClient client, Character sender);
        void SendCharacterRebirth(IWorldClient client, uint senderId);
        #endregion

        #region Summon

        void SendItemCasting(IWorldClient client, uint senderId);
        void SendPartycallRequest(IWorldClient client, uint senderId);
        void SendSummonAnswer(IWorldClient client, uint senderId, bool ok);

        #endregion

        #region Shop
        void SendMyShopBegin(IWorldClient client);
        void SendMyShopAddItem(IWorldClient client, byte bag, byte slot, byte shopSlot, uint price);
        void SendMyShopRemoveItem(IWorldClient client, byte shopSlot);
        void SendMyShopStarted(IWorldClient client);
        void SendMyShopCanceled(IWorldClient client);
        void SendMyShopEnded(IWorldClient client);
        void SendMyShopStarted(IWorldClient client, uint senderId, string shopName);
        void SendMyShopFinished(IWorldClient client, uint senderId);
        void SendMyShopVisit(IWorldClient client, bool ok, uint characterId);
        void SendMyShopLeave(IWorldClient client);
        void SendMyShopItems(IWorldClient client, IReadOnlyDictionary<byte, Item> items);
        void SendUseShopClosed(IWorldClient client);
        void SendMyShopBuyItemFailed(IWorldClient client);
        void SendMyShopBuyItemSuccess(IWorldClient client, uint money, byte shopSlot, byte shopCount, Item item);
        void SendUseShopItemCountChanged(IWorldClient client, byte slot, byte count);
        void SendMyShopSoldItem(IWorldClient client, byte slot, byte count, uint gold);
        #endregion

        #region Kill status
        void SendKillStatusInfo(IWorldClient client, byte killLevel, byte deathLevel);
        void SendKillsReward(IWorldClient client, bool ok, ushort stats);
        void SendDeathsReward(IWorldClient client, bool ok, uint money);
        #endregion

        #region Crafting
        void SendCraftList(IWorldClient client, CraftInfo config);
        void SendCraftRecipe(IWorldClient client, Recipe recipe);
        void SendCraftSuccess(IWorldClient client, bool ok);
        #endregion

        #region Market
        void SendMarketSellList(IWorldClient client, IList<DbMarket> items);
        void SendMarketTenderList(IWorldClient client);
        void SendMarketItemRegister(IWorldClient client, bool ok, DbMarket marketItem, Item item, uint gold);
        void SendMarketItemUnregister(IWorldClient client, bool ok, DbMarketCharacterResultItems result);
        void SendMarketEndItems(IWorldClient client, IList<DbMarketCharacterResultItems> items);
        void SendMarketGetItem(IWorldClient client, bool ok, uint marketId, Item item);
        void SendMarketSearchSection(IWorldClient client, byte prevCursor, byte nextCursor, IList<DbMarket> results);
        void SendMarketDirectBuy(IWorldClient client, MarketBuyItemResult ok, uint gold, DbMarketCharacterResultItems item);
        void SendMarketEndMoney(IWorldClient client, IList<DbMarketCharacterResultMoney> items);
        void SendMarketGetMoney(IWorldClient client, bool ok, uint marketId, uint gold);
        void SendMarketAddFavorite(IWorldClient client, MarketAddFavoriteResult ok, DbMarket item);
        void SendMarketFavorites(IWorldClient client, IList<DbMarketCharacterFavorite> results);
        void SendMarketRemoveFavorite(IWorldClient client, bool ok, uint marketId);
        void SendMarketRemoveAllFavorite(IWorldClient client, bool ok);
        #endregion

        #region GM
        void SendGmCommandSuccess(IWorldClient client);
        void SendGmCommandError(IWorldClient client, PacketType error);
        void SendCharacterPosition(IWorldClient client, Character player);
        void SendGmTeleportToPlayer(IWorldClient client, Character player);
        void SendGmSummon(IWorldClient client, uint senderId, ushort mapId, float x, float y, float z);
        void SendWarning(IWorldClient client, string message);
        void SendGmClearInventory(IWorldClient client);
        void SendGmClearEquipment(IWorldClient client);
        void SendGmMutedChat(IWorldClient client);
        void SendGmUnmutedChat(IWorldClient client);
        void SendGmStopOn(IWorldClient client);
        void SendGmStopOff(IWorldClient client);

        #endregion

        #region Other
        void SendWorldDay(IWorldClient client);
        void SendRunMode(IWorldClient client, MoveMotion motion);
        void SendAccountPoints(IWorldClient client, uint points);
        void SendScoutingInfo(IWorldClient client, Element defenceElement, ushort level, Mode grow);
        void SendCashPoint(IWorldClient client, uint points);
        #endregion
    }
}
