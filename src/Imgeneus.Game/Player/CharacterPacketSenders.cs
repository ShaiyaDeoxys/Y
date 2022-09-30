using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Zone.Obelisks;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        private void SendAdditionalStats() => _packetFactory.SendAdditionalStats(GameSession.Client, this);

        private void SendResetStats() => _packetFactory.SendResetStats(GameSession.Client, this);

        private void SendQuestFinished(uint npcId, Quest quest, bool success) => _packetFactory.SendQuestFinished(GameSession.Client, npcId, quest.Id, quest, success);

        public void SendFriendOnline(uint friendId, bool isOnline) => _packetFactory.SendFriendOnline(GameSession.Client, friendId, isOnline);

        private void SendQuestCountUpdate(short questId, byte index, byte count) => _packetFactory.SendQuestCountUpdate(GameSession.Client, questId, index, count);

        private void SendAddBuff(uint senderId, Buff buff) => _packetFactory.SendAddBuff(GameSession.Client, buff.Id, buff.Skill.SkillId, buff.Skill.SkillLevel, buff.CountDownInSeconds);

        private void SendRemoveBuff(uint senderId, Buff buff) => _packetFactory.SendRemoveBuff(GameSession.Client, buff.Id);

        private void SendAutoAttackStop() => _packetFactory.SendAutoAttackStop(GameSession.Client);

        private void SendUseMPSP(ushort needMP, ushort needSP) => _packetFactory.SendUseSMMP(GameSession.Client, needMP, needSP);

        private void SendCurrentHitpoints(int hp, int mp, int sp) => _packetFactory.SendCurrentHitpoints(GameSession.Client, hp, mp, sp);

        private void SendTargetAddBuff(IKillable target, Buff buff) => _packetFactory.SendTargetAddBuff(GameSession.Client, target.Id, buff, target is Mob);

        private void SendTargetRemoveBuff(IKillable target, Buff buff) => _packetFactory.SendTargetRemoveBuff(GameSession.Client, target.Id, buff, target is Mob);

        private void SendMobTargetHP(uint senderId, int hp, int mp, int sp) => _packetFactory.SendMobTargetHP(GameSession.Client, senderId, hp, AttackSpeed.Normal, MoveSpeed.Normal);

        private void SendCharacterTargetMaxHP(uint senderId, int maxHp)
       {
            if (AttackManager.Target is not null)
                _packetFactory.SendPlayerInTarget(GameSession.Client, senderId, maxHp, AttackManager.Target.HealthManager.CurrentHP);

        }

        private void SendDuelResponse(uint senderId, DuelResponse response) => _packetFactory.SendDuelResponse(GameSession.Client, response, senderId);

        private void SendDuelStart() => _packetFactory.SendDuelStart(GameSession.Client);

        private void SendDuelCancel(uint senderId, DuelCancelReason reason)
        {
            if (reason != DuelCancelReason.Other)
                _packetFactory.SendDuelCancel(GameSession.Client, reason, senderId);
        }

        private void SendDuelFinish(bool isWin) => _packetFactory.SendDuelFinish(GameSession.Client, isWin);

        public void SendAddItemToInventory(Item item)
        {
            _packetFactory.SendAddItem(GameSession.Client, item);

            if (item.ExpirationTime != null)
                _packetFactory.SendItemExpiration(GameSession.Client, item);
        }

        public void SendRemoveItemFromInventory(Item item, bool fullRemove) => _packetFactory.SendRemoveItem(GameSession.Client, item, fullRemove);

        public void SendItemExpired(Item item) => _packetFactory.SendItemExpired(GameSession.Client, item, ExpireType.ExpireItemDuration);

        public void SendObeliskBroken(Obelisk obelisk) => _packetFactory.SendObeliskBroken(GameSession.Client, obelisk);

        public void SendUseVehicle(bool success, bool status) => _packetFactory.SendUseVehicle(GameSession.Client, success, status);

        public void SendExperienceGain(uint expAmount) => _packetFactory.SendExperienceGain(GameSession.Client, expAmount);

        public void SendResetSkills() => _packetFactory.SendResetSkills(GameSession.Client, SkillsManager.SkillPoints);

        public void SendGuildMemberIsOnline(uint playerId) => _packetFactory.SendGuildMemberIsOnline(GameSession.Client, playerId);

        public void SendGuildMemberIsOffline(uint playerId) => _packetFactory.SendGuildMemberIsOffline(GameSession.Client, playerId);

        public void SendGuildJoinRequestAdd(Character character) => _packetFactory.SendGuildJoinRequestAdd(GameSession.Client, character.Id, character.LevelProvider.Level, character.AdditionalInfoManager.Class, character.AdditionalInfoManager.Name);

        public void SendGuildJoinRequestRemove(uint playerId) => _packetFactory.SendGuildJoinRequestRemove(GameSession.Client, playerId);

        public void SendGBRPoints(int currentPoints, int maxPoints, uint topGuild) => _packetFactory.SendGBRPoints(GameSession.Client, currentPoints, maxPoints, topGuild);

        public void SendGRBStartsSoon() => _packetFactory.SendGRBNotice(GameSession.Client, GRBNotice.StartsSoon);

        public void SendGRBStarted() => _packetFactory.SendGRBNotice(GameSession.Client, GRBNotice.Started);

        public void SendGRB10MinsLeft() => _packetFactory.SendGRBNotice(GameSession.Client, GRBNotice.Min10);

        public void SendGRB1MinLeft() => _packetFactory.SendGRBNotice(GameSession.Client, GRBNotice.Min1);

        public void SendGuildRanksCalculated(IEnumerable<(uint GuildId, int Points, byte Rank)> results) => _packetFactory.SendGuildRanksCalculated(GameSession.Client, results);

        public void SendGoldUpdate() => _packetFactory.SendGoldUpdate(GameSession.Client, InventoryManager.Gold);

        public void SendUseShopClosed() => _packetFactory.SendUseShopClosed(GameSession.Client);

        public void SendUseShopItemCountChanged(byte slot, byte count) => _packetFactory.SendUseShopItemCountChanged(GameSession.Client, slot, count);

        public void SendSoldItem(byte slot, byte count) => _packetFactory.SendMyShopSoldItem(GameSession.Client, slot, count, InventoryManager.Gold);

        public void SendKillCountChanged(byte index, uint count) => _packetFactory.SendKillCountChanged(GameSession.Client, index, count);
    }
}
