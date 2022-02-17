using System;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Game.Zone.Portals;
using System.Linq;
using Imgeneus.World.Game.Guild;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Server;
using System.Collections.Generic;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Duel;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        /// <summary>
        /// Sends to client character start-up information.
        /// </summary>
        public void SendCharacterInfo()
        {
            // SendWorldDay(); // TODO: why do we need it?
            //SendGuildList();
            //SendGuildMembersOnline();
            SendDetails();
            //SendAdditionalStats();
            //SendCurrentHitpoints();
            SendInventoryItems();
            SendLearnedSkills();
            SendOpenQuests();
            SendFinishedQuests();
            //SendActiveBuffs();
            //SendMoveAndAttackSpeed();
            //SendFriends();
            SendBlessAmount();
            SendBankItems();
            SendGuildNpcLvlList();
            //SendAutoStats();
#if !EP8_V2
            SendAccountPoints(); // WARNING: This is necessary if you have an in-game item mall.
#endif
        }

        private void SendWorldDay() => _packetsHelper.SendWorldDay(Client);

        private void SendDetails() => _packetsHelper.SendDetails(Client, this);

        private void SendInventoryItems()
        {
            var inventoryItems = InventoryManager.InventoryItems.Values.ToArray();
            _packetsHelper.SendInventoryItems(Client, inventoryItems); // WARNING: some servers expanded invetory to 6 bags(os is 5 bags), if you send item in 6 bag, client will crash!

            foreach (var item in inventoryItems.Where(i => i.ExpirationTime != null))
                SendItemExpiration(item);
        }

        private void SendAdditionalStats() => _packetsHelper.SendAdditionalStats(Client, this);

        private void SendResetStats() => _packetsHelper.SendResetStats(Client, this);

        private void SendItemExpiration(Item item) => _packetsHelper.SendItemExpiration(Client, item);

        private void SendLearnedSkills() => _packetsHelper.SendLearnedSkills(Client, this);

        private void SendOpenQuests() => _packetsHelper.SendQuests(Client, Quests.Where(q => !q.IsFinished));

        private void SendFinishedQuests() => _packetsHelper.SendFinishedQuests(Client, Quests.Where(q => q.IsFinished));

        private void SendQuestStarted(Quest quest, int npcId = 0) => _packetsHelper.SendQuestStarted(Client, quest.Id, npcId);

        private void SendQuestFinished(Quest quest, int npcId = 0) => _packetsHelper.SendQuestFinished(Client, quest, npcId);

        public void SendFriendOnline(int friendId, bool isOnline) => _packetsHelper.SendFriendOnline(Client, friendId, isOnline);

        private void SendQuestCountUpdate(ushort questId, byte index, byte count) => _packetsHelper.SendQuestCountUpdate(Client, questId, index, count);

        //private void SendActiveBuffs() => _packetsHelper.SendActiveBuffs(Client, ActiveBuffs);

        private void SendAddBuff(Buff buff) => _packetsHelper.SendAddBuff(Client, buff);

        private void SendRemoveBuff(Buff buff) => _packetsHelper.SendRemoveBuff(Client, buff);

        //private void SendMaxHP() => _packetsHelper.SendMaxHitpoints(Client, this, HitpointType.HP);

        //private void SendMaxSP() => _packetsHelper.SendMaxHitpoints(Client, this, HitpointType.SP);

        //private void SendMaxMP() => _packetsHelper.SendMaxHitpoints(Client, this, HitpointType.MP);

        private void SendAttackStart() => _packetsHelper.SendAttackStart(Client);

        private void SendUseSMMP(ushort needMP, ushort needSP) => _packetsHelper.SendUseSMMP(Client, needMP, needSP);

        private void SendCooldownNotOver(IKillable target, Skill skill) => _packetsHelper.SendCooldownNotOver(Client, this, target, skill);

        private void SendRunMode() => _packetsHelper.SendRunMode(Client, this);

        private void SendTargetAddBuff(IKillable target, Buff buff) => _packetsHelper.SendTargetAddBuff(Client, target.Id, buff, target is Mob);

        private void SendTargetRemoveBuff(IKillable target, Buff buff) => _packetsHelper.SendTargetRemoveBuff(Client, target.Id, buff, target is Mob);

        private void SendDuelResponse(int senderId, DuelResponse response) => _packetFactory.SendDuelResponse(GameSession.Client, response, senderId);

        private void SendDuelStart() => _packetFactory.SendDuelStart(GameSession.Client);

        private void SendDuelCancel(int senderId, DuelCancelReason reason)
        {
            if (reason != DuelCancelReason.Other)
                _packetFactory.SendDuelCancel(GameSession.Client, reason, senderId);
        }

        private void SendDuelFinish(bool isWin) => _packetFactory.SendDuelFinish(GameSession.Client, isWin);

        public void SendAddItemToInventory(Item item)
        {
            _packetsHelper.SendAddItem(Client, item);

            if (item.ExpirationTime != null)
                _packetsHelper.SendItemExpiration(Client, item);
        }

        public void SendRemoveItemFromInventory(Item item, bool fullRemove) => _packetsHelper.SendRemoveItem(Client, item, fullRemove);

        public void SendItemExpired(Item item) => _packetsHelper.SendItemExpired(Client, item, ExpireType.ExpireItemDuration);

        public void SendTradeCanceled() => _packetsHelper.SendTradeCanceled(GameSession.Client);

        public void SendObelisks() => _packetsHelper.SendObelisks(Client, Map.Obelisks.Values);

        public void SendObeliskBroken(Obelisk obelisk) => _packetsHelper.SendObeliskBroken(Client, obelisk);

        public void SendTeleportViaNpc(NpcTeleportNotAllowedReason reason) => _packetsHelper.SendTeleportViaNpc(Client, reason, InventoryManager.Gold);

        public void SendUseVehicle(bool success, bool status) => _packetsHelper.SendUseVehicle(Client, success, status);

        public void SendMyShape() => _packetsHelper.SendCharacterShape(Client, this);

        public void SendExperienceGain(uint expAmount) => _packetsHelper.SendExperienceGain(Client, expAmount);

        public void SendWarning(string message) => _packetsHelper.SendWarning(Client, message);

        public void SendBankItems() => _packetsHelper.SendBankItems(Client, BankItems.Values.ToList());

        public void SendBankItemClaim(byte bankSlot, Item item) => _packetsHelper.SendBankItemClaim(Client, bankSlot, item);
        public void SendAccountPoints() => _packetsHelper.SendAccountPoints(Client, Points);

        public void SendResetSkills() => _packetsHelper.SendResetSkills(Client, SkillsManager.SkillPoints);

        public void SendGuildCreateSuccess(int guildId, byte rank, string guildName, string guildMessage) => _packetsHelper.SendGuildCreateSuccess(Client, guildId, rank, guildName, guildMessage);

        public void SendGuildMemberIsOnline(int playerId) => _packetFactory.SendGuildMemberIsOnline(GameSession.Client, playerId);

        public void SendGuildMemberIsOffline(int playerId) => _packetFactory.SendGuildMemberIsOffline(GameSession.Client, playerId);

        public void SendGuildJoinRequestAdd(Character character) => _packetFactory.SendGuildJoinRequestAdd(GameSession.Client, character);

        public void SendGuildJoinRequestRemove(int playerId) => _packetFactory.SendGuildJoinRequestRemove(GameSession.Client, playerId);

        public void SendGuildDismantle() => _packetsHelper.SendGuildDismantle(Client);

        public void SendGuildListAdd(DbGuild guild) => _packetsHelper.SendGuildListAdd(Client, guild);

        public void SendGuildListRemove(int guildId) => _packetsHelper.SendGuildListRemove(Client, guildId);

        public void SendGBRPoints(int currentPoints, int maxPoints, int topGuild) => _packetsHelper.SendGBRPoints(Client, currentPoints, maxPoints, topGuild);

        public void SendGRBStartsSoon() => _packetsHelper.SendGRBNotice(Client, GRBNotice.StartsSoon);

        public void SendGRBStarted() => _packetsHelper.SendGRBNotice(Client, GRBNotice.Started);

        public void SendGRB10MinsLeft() => _packetsHelper.SendGRBNotice(Client, GRBNotice.Min10);

        public void SendGRB1MinLeft() => _packetsHelper.SendGRBNotice(Client, GRBNotice.Min1);

        public void SendGuildRanksCalculated(IEnumerable<(int GuildId, int Points, byte Rank)> results) => _packetsHelper.SendGuildRanksCalculated(Client, results);

        public void SendGoldUpdate() => _packetFactory.SendGoldUpdate(GameSession.Client, InventoryManager.Gold);
    }
}
