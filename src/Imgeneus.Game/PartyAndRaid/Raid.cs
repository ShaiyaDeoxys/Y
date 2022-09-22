using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.PartyAndRaid
{
    public class Raid : BaseParty
    {
        public const byte MAX_RAID_MEMBERS_COUNT = 30;

        private object _syncObject = new object();

        private readonly ConcurrentDictionary<Character, int> _membersDict = new ConcurrentDictionary<Character, int>();
        private readonly ConcurrentDictionary<int, Character> _indexesDict = new ConcurrentDictionary<int, Character>();

        protected override IList<Character> _members { get => _membersDict.Keys.ToList(); set => throw new NotImplementedException(); }

        public override IList<Character> GetShortMembersList(Character character)
        {
            var result = new List<Character>();

            if (!_membersDict.ContainsKey(character))
                return result;

            var index = _membersDict[character];
            var normalizedIndex = index % 6;

            var startIndex = index - normalizedIndex;
            var endIndex = startIndex + 6;

            for (var i = startIndex; i < endIndex; i++)
                if (_indexesDict.ContainsKey(i))
                    result.Add(_indexesDict[i]);

            return result;
        }

        public Raid(bool autoJoin, RaidDropType dropType, IGamePacketFactory packetFactory) : base(packetFactory)
        {
            _autoJoin = autoJoin;
            _dropType = dropType;
        }

        #region Auto join

        private bool _autoJoin;

        /// <summary>
        /// Indicates if player can join raid without invite.
        /// </summary>
        public bool AutoJoin
        {
            get
            {
                return _autoJoin;
            }
        }

        /// <summary>
        /// Changes <see cref="AutoJoin"/> property.
        /// </summary>
        /// <param name="autoJoin"></param>
        public void ChangeAutoJoin(bool autoJoin)
        {
            _autoJoin = autoJoin;

            foreach (var m in Members.ToList())
            {
                _packetFactory.SendAutoJoinChanged(m.GameSession.Client, AutoJoin);
            }
        }

        #endregion

        #region Drop type

        private RaidDropType _dropType;

        /// <summary>
        /// What kind of drop type is enabled.
        /// </summary>
        public RaidDropType DropType
        {
            get
            {
                return _dropType;
            }
        }

        /// <summary>
        /// Changes <see cref="DropType"/> property.
        /// </summary>
        public void ChangeDropType(RaidDropType dropType)
        {
            _dropType = dropType;

            foreach (var m in Members.ToList())
            {
                _packetFactory.SendDropType(m.GameSession.Client, DropType);
            }
        }

        #endregion

        #region Distribute drop

        /// <summary>
        /// Tries to distribute drop based on drop type.
        /// </summary>
        /// <param name="items">drop items</param>
        /// <param name="dropCreator">player, that killed mob and generated drop</param>
        /// <returns>items, that we couldn't assign to any party member (i.e. members have full inventory)</returns>
        public override IList<Item> DistributeDrop(IList<Item> items, Character dropCreator)
        {
            lock (_syncObject)
            {
                List<Item> notDistibutedItems;
                switch (DropType)
                {
                    case RaidDropType.Leader:
                        notDistibutedItems = DropToLeader(items, dropCreator);
                        break;

                    case RaidDropType.Group:
                        notDistibutedItems = DropToGroup(items, dropCreator);
                        break;

                    case RaidDropType.Random:
                        notDistibutedItems = DropToRandom(items, dropCreator);
                        break;

                    default:
                        notDistibutedItems = DropToLeader(items, dropCreator);
                        break;
                }
                return notDistibutedItems;
            }
        }

        /// <summary>
        /// All drop goes to the leader if he is near drop creator and has a place in inventory,
        /// otherwise drop is not assigned and created on the map.
        /// </summary>
        private List<Item> DropToLeader(IList<Item> items, Character dropCreator)
        {
            List<Item> notDistibutedItems = new List<Item>();
            if (Leader.Map != dropCreator.Map || MathExtensions.Distance(Leader.PosX, dropCreator.PosX, Leader.PosZ, dropCreator.PosZ) > 100)
            {
                // Leader is too far away.
                notDistibutedItems.AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    if (item.Type != Item.MONEY_ITEM_TYPE)
                    {
                        var inventoryItem = Leader.InventoryManager.AddItem(item, "raid_drop");
                        if (inventoryItem != null)
                        {
                            foreach (var member in Members.Where(m => m != Leader))
                                _packetFactory.SendMemberGetItem(member.GameSession.Client, Leader.Id, inventoryItem);
                        }
                        else
                        {
                            // Leader inventory is full.
                            notDistibutedItems.Add(item);
                        }
                    }
                    else
                    {
                        Leader.InventoryManager.Gold = (uint)(Leader.InventoryManager.Gold + item.Gem1.TypeId);
                        Leader.SendAddItemToInventory(item);
                    }
                }
            }
            return notDistibutedItems;
        }

        private int _lastDropIndex = -1;

        /// <summary>
        /// Drop is distributed among players 1 by 1.
        /// </summary>
        private List<Item> DropToGroup(IList<Item> items, Character dropCreator)
        {
            List<Item> notDistibutedItems = new List<Item>();

            foreach (var item in items)
            {
                bool itemAdded = false;
                int numberOfIterations = 0;

                do
                {
                    _lastDropIndex++;
                    if (_lastDropIndex == _indexesDict.Keys.Count)
                        _lastDropIndex = 0;

                    var dropReceiver = _indexesDict[_indexesDict.Keys.ElementAt(_lastDropIndex)];
                    if (dropReceiver.Map == dropCreator.Map && MathExtensions.Distance(dropReceiver.PosX, dropCreator.PosX, dropReceiver.PosZ, dropCreator.PosZ) <= 100)
                    {
                        if (item.Type != Item.MONEY_ITEM_TYPE)
                        {
                            var inventoryItem = dropReceiver.InventoryManager.AddItem(item, "raid_drop");
                            if (inventoryItem != null)
                            {
                                itemAdded = true;
                                foreach (var member in Members.Where(m => m != dropReceiver))
                                    _packetFactory.SendMemberGetItem(member.GameSession.Client, dropReceiver.Id, inventoryItem);
                            }
                        }
                        else
                        {
                            itemAdded = true;
                            // Money is not counted as item. That's why return index to prev value.
                            if (_lastDropIndex == 0)
                                _lastDropIndex = _indexesDict.Keys.Count - 1;
                            else
                                _lastDropIndex--;

                            DistributeMoney(item);
                        }
                    }

                    numberOfIterations++;
                }
                while (!itemAdded && numberOfIterations < MAX_RAID_MEMBERS_COUNT);

                if (!itemAdded)
                    notDistibutedItems.Add(item);
            }

            return notDistibutedItems;
        }

        /// <summary>
        /// Drop is assigned to random characters.
        /// </summary>
        private List<Item> DropToRandom(IList<Item> items, Character dropCreator)
        {
            List<Item> notDistibutedItems = new List<Item>();
            foreach (var item in items)
            {
                bool itemAdded = false;
                int numberOfIterations = 0;

                do
                {
                    var randomIndex = new Random().Next(0, _indexesDict.Keys.Count);
                    var dropReceiver = _indexesDict[_indexesDict.Keys.ElementAt(randomIndex)];
                    if (dropReceiver.Map == dropCreator.Map && MathExtensions.Distance(dropReceiver.PosX, dropCreator.PosX, dropReceiver.PosZ, dropCreator.PosZ) <= 100)
                    {
                        if (item.Type != Item.MONEY_ITEM_TYPE)
                        {
                            var inventoryItem = dropReceiver.InventoryManager.AddItem(item, "raid_drop");
                            if (inventoryItem != null)
                            {
                                itemAdded = true;
                                foreach (var member in Members.Where(m => m != dropReceiver))
                                    _packetFactory.SendMemberGetItem(member.GameSession.Client, dropReceiver.Id, inventoryItem);
                            }
                        }
                        else
                        {
                            itemAdded = true;
                            DistributeMoney(item);
                        }
                    }

                    numberOfIterations++;
                }
                while (!itemAdded && numberOfIterations < MAX_RAID_MEMBERS_COUNT);

                if (!itemAdded)
                    notDistibutedItems.Add(item);
            }

            return notDistibutedItems;
        }

        public override void MemberGetItem(Character player, Item item)
        {
            foreach (var member in Members.Where(m => m != player))
                _packetFactory.SendMemberGetItem(member.GameSession.Client, player.Id, item);
        }

        #endregion

        #region Members

        /// <summary>
        /// Gets index of character in raid.
        /// </summary>
        public int GetIndex(Character character)
        {
            if (_membersDict.TryGetValue(character, out var index))
                return index;
            else
                return -1;
        }

        /// <summary>
        /// Tries to find free index.
        /// </summary>
        private int FindFreeIndex()
        {
            lock (_syncObject)
            {
                if (_membersDict.Values.Count == MAX_RAID_MEMBERS_COUNT)
                {
                    return -1;
                }

                var occupiedIndexes = _membersDict.Values.OrderBy(v => v).ToList();
                if (occupiedIndexes.Count == 0)
                {
                    return 0;
                }

                var freeIndex = -1;
                for (var i = 0; i < occupiedIndexes.Count; i++)
                {
                    if (occupiedIndexes[i] != i)
                    {
                        freeIndex = i;
                        break;
                    }
                }

                if (freeIndex != -1)
                {
                    return freeIndex;
                }
                else
                {
                    return occupiedIndexes.Count;
                }
            }
        }

        #endregion

        public override bool EnterParty(Character newPartyMember)
        {
            lock (_syncObject)
            {
                // Check if raid is not full.
                if (_membersDict.Keys.Count == MAX_RAID_MEMBERS_COUNT)
                    return false;

                var index = FindFreeIndex();
                if (index == -1)
                    return false;

                if (!_membersDict.TryAdd(newPartyMember, index) || !_indexesDict.TryAdd(index, newPartyMember))
                {
                    _membersDict.TryRemove(newPartyMember, out index);
                    _indexesDict.TryRemove(index, out var member);
                    return false;
                }

                if (Members.Count == 1)
                    Leader = newPartyMember;
                if (Members.Count == 2)
                    SubLeader = newPartyMember;

                SubcribeToCharacterChanges(newPartyMember);

                CallMemberEnter();

                // Notify others, that new raid member joined.
                foreach (var member in Members)
                    _packetFactory.SendPlayerJoinedRaid(member.GameSession.Client, newPartyMember, (ushort)index);

                return true;
            }
        }

        public override void LeaveParty(Character leftPartyMember)
        {
            lock (_syncObject)
            {
                foreach (var member in Members)
                    _packetFactory.SendPlayerLeftRaid(member.GameSession.Client, leftPartyMember);

                RemoveMember(leftPartyMember);
                CallMemberLeft();
            }
        }

        public override void KickMember(Character player)
        {
            lock (_syncObject)
            {
                foreach (var member in Members)
                    _packetFactory.SendRaidKickMember(member.GameSession.Client, player);

                RemoveMember(player);
            }
        }

        public override void Dismantle()
        {
            lock (_syncObject)
            {
                var members = Members.ToList();
                _membersDict.Clear();
                foreach (var m in members)
                {
                    m.PartyManager.Party = null;
                    _packetFactory.SendRaidDismantle(m.GameSession.Client);
                }
            }
        }

        /// <summary>
        /// Removes character from raid, checks if he was leader or if it's the last member.
        /// </summary>
        private void RemoveMember(Character character)
        {
            _membersDict.TryRemove(character, out var index);
            _indexesDict.TryRemove(index, out var member);

            // If it was the last member.
            if (Members.Count == 1)
            {
                var lastMember = Members[0];
                _membersDict.Clear();
                lastMember.PartyManager.Party = null;
                _packetFactory.SendPlayerLeftRaid(lastMember.GameSession.Client, lastMember);
                CallAllMembersLeft();
            }
            else if (character == Leader)
            {
                var newLeader = SubLeader;
                var newSubLeader = Members.FirstOrDefault(m => m != Leader && m != SubLeader);
                SubLeader = newSubLeader;
                Leader = newLeader;
            }
            else if (character == SubLeader)
            {
                SubLeader = Members.FirstOrDefault(m => m != Leader && m != SubLeader);
            }
        }

        /// <summary>
        /// Moves character inside raid.
        /// </summary>
        /// <param name="sourceIndex">old index</param>
        /// <param name="destinationIndex">new index</param>
        public void MoveCharacter(int sourceIndex, int destinationIndex)
        {
            lock (_syncObject)
            {
                if (_indexesDict.TryRemove(sourceIndex, out var sourceCharacter))
                {
                    _indexesDict.TryRemove(destinationIndex, out var destinationCharacter);
                    if (destinationCharacter is null) // free space
                    {
                        _indexesDict.TryAdd(destinationIndex, sourceCharacter);
                        _membersDict[sourceCharacter] = destinationIndex;
                    }
                    else
                    {
                        _indexesDict.TryAdd(destinationIndex, sourceCharacter);
                        _indexesDict.TryAdd(sourceIndex, destinationCharacter);
                        _membersDict[sourceCharacter] = destinationIndex;
                        _membersDict[destinationCharacter] = sourceIndex;
                    }
                }

                foreach (var member in Members)
                    _packetFactory.SendPlayerMove(member.GameSession.Client, sourceIndex, destinationIndex, GetIndex(Leader), GetIndex(SubLeader));
            }
        }

        #region Senders

        protected override void SendAddBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            _packetFactory.SendAddRaidBuff(client, senderId, skillId, skillLevel);
        }

        protected override void SendRemoveBuff(IWorldClient client, uint senderId, ushort skillId, byte skillLevel)
        {
            _packetFactory.SendRemoveRaidBuff(client, senderId, skillId, skillLevel);
        }

        protected override void Send_Single_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type)
        {
            _packetFactory.SendRaid_Single_HP_SP_MP(client, senderId, value, type);
        }

        protected override void Send_Single_Max_HP_SP_MP(IWorldClient client, uint senderId, int value, byte type)
        {
            _packetFactory.SendRaid_Single_Max_HP_SP_MP(client, senderId, value, type);
        }

        protected override void Send_HP_SP_MP(IWorldClient client, Character partyMember)
        {
            throw new NotImplementedException();
        }

        protected override void Send_Max_HP_SP_MP(IWorldClient client, Character partyMember)
        {
            throw new NotImplementedException();
        }

        protected override void SendNewLeader(IWorldClient client, Character character)
        {
            _packetFactory.SendRaidNewLeader(client, character);
        }

        protected override void SendNewSubLeader(IWorldClient client, Character character)
        {
            _packetFactory.SendNewRaidSubLeader(client, character);
        }

        protected override void SendLevel(IWorldClient client, uint senderId, ushort level)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
