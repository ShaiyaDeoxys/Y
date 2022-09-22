using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.PartyAndRaid
{
    public class Party : BaseParty
    {
        public const byte MAX_PARTY_MEMBERS_COUNT = 7;

        public Party(IGamePacketFactory packetFactory) : base(packetFactory)
        {
        }

        protected override IList<Character> _members { get; set; } = new List<Character>();

        public override IList<Character> GetShortMembersList(Character character) => _members;

        private object _syncObject = new object();

        /// <summary>
        /// Tries to enter party, if it's enough place.
        /// </summary>
        /// <returns>true if player could enter party, otherwise false</returns>
        public override bool EnterParty(Character newPartyMember)
        {
            lock (_syncObject)
            {
                // Check if party is not full.
                if (_members.Count == MAX_PARTY_MEMBERS_COUNT)
                    return false;

                _members.Add(newPartyMember);

                if (Members.Count == 1)
                    Leader = newPartyMember;

                SubcribeToCharacterChanges(newPartyMember);

                CallMemberEnter();

                // Notify others, that new party member joined.
                foreach (var member in Members.Where(m => m != newPartyMember))
                    _packetFactory.SendPlayerJoinedParty(member.GameSession.Client, newPartyMember);

                return true;
            }
        }

        /// <summary>
        /// Leaves party.
        /// </summary>
        public override void LeaveParty(Character leftPartyMember)
        {
            lock (_syncObject)
            {
                foreach (var member in Members)
                    _packetFactory.SendPlayerLeftParty(member.GameSession.Client, leftPartyMember);

                RemoveMember(leftPartyMember);
                CallMemberLeft();
            }
        }

        /// <summary>
        /// Only party leader can kick member.
        /// </summary>
        public override void KickMember(Character playerToKick)
        {
            lock (_syncObject)
            {
                foreach (var member in Members)
                    _packetFactory.SendPartyKickMember(member.GameSession.Client, playerToKick);

                RemoveMember(playerToKick);
            }
        }

        /// <summary>
        /// Removes character from party, checks if he was leader or if it's the last member.
        /// </summary>
        private void RemoveMember(Character character)
        {
            // Unsubscribe.
            UnsubcribeFromCharacterChanges(character);

            _members.Remove(character);

            // If it was the last member.
            if (Members.Count == 1)
            {
                var lastMember = Members[0];
                _members.Clear();

                lastMember.PartyManager.Party = null;
                _packetFactory.SendPlayerLeftParty(lastMember.GameSession.Client, lastMember);

                CallAllMembersLeft();
            }
            else if (character == Leader)
            {
                Leader = Members[0];
            }
        }

        #region Distribute drop

        private int _lastDropIndex = -1;

        /// <summary>
        /// Tries to distribute drop 1 by 1.
        /// </summary>
        /// <param name="items">drop items</param>
        /// <param name="dropCreator">player, that killed mob and generated drop</param>
        /// <returns>items, that we couldn't assign to any party member (i.e. members have full inventory)</returns>
        public override IList<Item> DistributeDrop(IList<Item> items, Character dropCreator)
        {
            lock (_syncObject)
            {
                var notDistibutedItems = new List<Item>();
                foreach (var item in items)
                {
                    bool itemAdded = false;
                    int numberOfIterations = 0;
                    do
                    {
                        _lastDropIndex++;
                        if (_lastDropIndex == Members.Count)
                            _lastDropIndex = 0;

                        var dropReceiver = Members[_lastDropIndex];
                        if (dropReceiver.Map == dropCreator.Map && MathExtensions.Distance(dropReceiver.PosX, dropCreator.PosX, dropReceiver.PosZ, dropCreator.PosZ) <= 100)
                        {
                            if (item.Type != Item.MONEY_ITEM_TYPE)
                            {
                                var inventoryItem = dropReceiver.InventoryManager.AddItem(item, "party_drop");
                                if (inventoryItem != null)
                                {
                                    itemAdded = true;
                                    foreach (var member in Members.Where(m => m != dropReceiver))
                                        SendMemberGetItem(member.GameSession.Client, dropReceiver.Id, inventoryItem);
                                }
                            }
                            else
                            {
                                itemAdded = true;
                                // Money is not counted as item. That's why return index to prev value.
                                if (_lastDropIndex == 0)
                                    _lastDropIndex = Members.Count - 1;
                                else
                                    _lastDropIndex--;

                                DistributeMoney(item);
                            }
                        }

                        numberOfIterations++;
                    }
                    while (!itemAdded && numberOfIterations < MAX_PARTY_MEMBERS_COUNT);

                    if (!itemAdded)
                        notDistibutedItems.Add(item);
                }

                return notDistibutedItems;
            }
        }

        public override void MemberGetItem(Character player, Item item)
        {
            foreach (var member in Members.Where(m => m != player))
                SendMemberGetItem(member.GameSession.Client, player.Id, item);
        }

        #endregion

        #region Senders

        protected override void SendNewLeader(IWorldClient client, Character character)
        {
            _packetFactory.SendNewPartyLeader(client, character);
        }

        protected override void SendAddBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            _packetFactory.SendAddPartyBuff(client, senderId, skillId, skillLevel);
        }

        protected override void SendRemoveBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            _packetFactory.SendRemovePartyBuff(client, senderId, skillId, skillLevel);
        }

        protected override void Send_Single_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type)
        {
            _packetFactory.SendPartySingle_HP_SP_MP(client, senderId, value, type);
        }

        protected override void Send_Single_Max_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type)
        {
            _packetFactory.SendPartySingle_Max_HP_SP_MP(client, senderId, value, type);
        }

        protected override void Send_HP_SP_MP(IWorldClient client, Character partyMember)
        {
            _packetFactory.SendParty_HP_SP_MP(client, partyMember);
        }

        protected override void Send_Max_HP_SP_MP(IWorldClient client, Character partyMember)
        {
            _packetFactory.SendParty_Max_HP_SP_MP(client, partyMember);
        }

        public override void Dismantle()
        {
            throw new NotImplementedException();
        }

        protected override void SendNewSubLeader(IWorldClient client, Character leader)
        {
        }

        private void SendMemberGetItem(IWorldClient client, uint characterId, Item item)
        {
            _packetFactory.SendPartyMemberGetItem(client, characterId, item);
        }

        protected override void SendLevel(IWorldClient client, uint senderId, ushort level)
        {
            _packetFactory.SendPartyLevel(client, senderId, level);
        }

        #endregion
    }
}
