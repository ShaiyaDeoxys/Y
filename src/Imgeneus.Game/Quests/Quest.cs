using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SQuest = Parsec.Shaiya.NpcQuest.Quest;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.Quests
{
    public class Quest : IDisposable
    {
        public readonly SQuest Config;

        public Quest(SQuest quest, DbCharacterQuest dbCharacterQuest) : this(quest)
        {
            if (dbCharacterQuest.Delay > 0)
            {
                _endTime = DateTime.UtcNow.AddMinutes(dbCharacterQuest.Delay);

                var interval = dbCharacterQuest.Delay * 60 * 1000;
                if (interval < 0)
                    interval = 10000;
                _endTimer.Interval = interval;
                _endTimer.Start();
            }
            CountMob1 = dbCharacterQuest.Count1;
            CountMob2 = dbCharacterQuest.Count2;
            IsFinished = dbCharacterQuest.Finish;
            IsSuccessful = dbCharacterQuest.Success;
        }

        public Quest(SQuest quest)
        {
            Config = quest;

            _endTimer.AutoReset = false;
            _endTimer.Elapsed += EndTimer_Elapsed;
        }

        public void Dispose()
        {
            _endTimer.Elapsed -= EndTimer_Elapsed;
        }

        /// <summary>
        /// Quest id.
        /// </summary>
        public short Id { get => Config.Id; }

        /// <summary>
        /// Number of killed mobs of first type.
        /// </summary>
        public byte CountMob1 { get; private set; }

        /// <summary>
        /// Sets new value for CountMob1.
        /// </summary>
        public void IncreaseCountMob1()
        {
            if (CountMob1 != byte.MaxValue)
            {
                CountMob1++;
            }
        }

        /// <summary>
        /// Number of killed mobs of second type.
        /// </summary>
        public byte CountMob2 { get; private set; }

        /// <summary>
        /// Sets new value for CountMob2.
        /// </summary>
        public void IncreaseCountMob2()
        {
            if (CountMob2 != byte.MaxValue)
            {
                CountMob2++;
            }
        }

        /// <summary>
        /// TODO: unknown.
        /// </summary>
        public byte Count3 { get; private set; }

        /// <summary>
        /// bool indicator, shows if the quest is completed or not.
        /// </summary>
        public bool IsFinished { get; private set; }

        /// <summary>
        /// bool indicator, shows if the quest was completed successfully.
        /// </summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        /// Checks if all quest requirements fulfilled.
        /// </summary>
        public bool RequirementsFulfilled(IEnumerable<Item> inventoryItems)
        {
            return CountMob1 >= Config.RequiredMobCount1 && CountMob2 >= Config.RequiredMobCount2
                  && inventoryItems.Where(itm => itm.Type == FarmItemType_1 && itm.TypeId == FarmItemTypeId_1).Sum(x => x.Count) >= FarmItemCount_1
                  && inventoryItems.Where(itm => itm.Type == FarmItemType_2 && itm.TypeId == FarmItemTypeId_2).Sum(x => x.Count) >= FarmItemCount_2
                  && inventoryItems.Where(itm => itm.Type == FarmItemType_3 && itm.TypeId == FarmItemTypeId_3).Sum(x => x.Count) >= FarmItemCount_3;
        }

        /// <summary>
        /// Finishes quest.
        /// </summary>
        public void Finish(bool successful)
        {
            _endTimer.Stop();
            IsFinished = true;
            IsSuccessful = successful;
        }

        #region Start items

        /// <summary>
        /// Items, that are given upon quest start.
        /// </summary>
        public List<QuestItem> StartRequiredItems { get => Config.RequiredItems; }

        #endregion

        #region Requirements

        /// <summary>
        /// Item type, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemType_1 { get => Config.FarmItems.Count > 0 ? Config.FarmItems[0].Type : (byte)0; }

        /// <summary>
        /// Item type id, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemTypeId_1 { get => Config.FarmItems.Count > 0 ? Config.FarmItems[0].TypeId : (byte)0; }

        /// <summary>
        /// Number of items, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemCount_1 { get => Config.FarmItems.Count > 0 ? Config.FarmItems[0].Count : (byte)0; }

        /// <summary>
        /// Item type, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemType_2 { get => Config.FarmItems.Count > 1 ? Config.FarmItems[1].Type : (byte)0; }

        /// <summary>
        /// Item type id, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemTypeId_2 { get => Config.FarmItems.Count > 1 ? Config.FarmItems[1].TypeId : (byte)0; }

        /// <summary>
        /// Number of items, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemCount_2 { get => Config.FarmItems.Count > 1 ? Config.FarmItems[1].Count : (byte)0; }

        /// <summary>
        /// Item type, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemType_3 { get => Config.FarmItems.Count > 2 ? Config.FarmItems[2].Type : (byte)0; }

        /// <summary>
        /// Item type id, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemTypeId_3 { get => Config.FarmItems.Count > 2 ? Config.FarmItems[2].TypeId : (byte)0; }

        /// <summary>
        /// Number of items, that player must have in order to complete quest.
        /// </summary>
        public byte FarmItemCount_3 { get => Config.FarmItems.Count > 2 ? Config.FarmItems[2].Count : (byte)0; }

        /// <summary>
        /// Mob 1, that should be killed.
        /// </summary>
        public ushort RequiredMobId_1 { get => Config.RequiredMobId1; }

        /// <summary>
        /// Number of mobs 1, that should be killed.
        /// </summary>
        public byte RequiredMobCount_1 { get => Config.RequiredMobCount1; }

        /// <summary>
        /// Mob 2, that should be killed.
        /// </summary>
        public ushort RequiredMobId_2 { get => Config.RequiredMobId2; }

        /// <summary>
        /// Number of mobs 2, that should be killed.
        /// </summary>
        public byte RequiredMobCount_2 { get => Config.RequiredMobCount2; }

        #endregion

        #region Revards

        /// <summary>
        /// How much experience player gets from this quest.
        /// </summary>
        public uint XP { get => (uint)Config.Results.Sum(x => x.Exp); }

        /// <summary>
        /// How much money player gets from this quest.
        /// </summary>
        public uint Gold { get => (uint)Config.Results.Sum(x => x.Money); }

        /// <summary>
        /// Items, that player gets from quest.
        /// </summary>
        public IList<(byte Type, byte TypeId, byte Count)> RevardItems
        {
            get
            {
                var items = new List<(byte Type, byte TypeId, byte Count)>();
                foreach (var result in Config.Results)
                {
                    if (result.ItemType1 != 0 && result.ItemTypeId1 != 0)
                        items.Add((result.ItemType1, result.ItemTypeId1, result.ItemCount1));

                    if (result.ItemType2 != 0 && result.ItemTypeId2 != 0)
                        items.Add((result.ItemType2, result.ItemTypeId2, result.ItemCount2));

                    if (result.ItemType3 != 0 && result.ItemTypeId3 != 0)
                        items.Add((result.ItemType3, result.ItemTypeId3, result.ItemCount3));
                }

                return items;
            }
        }

        /// <summary>
        /// After quest is finished, is it possible to choose revard?
        /// </summary>
        public bool CanChooseRevard { get => Config.ResultUserSelect > 0; }

        #endregion

        #region Quest timer

        /// <summary>
        /// Time before quest must be finished.
        /// </summary>
        private DateTime _endTime;

        /// <summary>
        /// Time, that is still available to complete the quest. In minutes.
        /// </summary>
        public ushort RemainingTime
        {
            get
            {
                if (Config.Time == 0)
                    return 0;
                return (ushort)_endTime.Subtract(DateTime.UtcNow).TotalMinutes;
            }
        }

        /// <summary>
        /// Quest finishes, because time is over.
        /// </summary>
        public event Action<Quest> QuestTimeElapsed;

        /// <summary>
        /// Starts quest time.
        /// </summary>
        public void StartQuestTimer()
        {
            if (Config.Time > 0)
            {
                _endTime = DateTime.UtcNow.AddMinutes(Config.Time);
                _endTimer.Interval = Config.Time * 60 * 1000;
                _endTimer.Start();
            }
        }

        /// <summary>
        /// Timer for quest finishing.
        /// </summary>
        private Timer _endTimer = new Timer();

        private void EndTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsFinished = true;
            IsSuccessful = false;
            QuestTimeElapsed?.Invoke(this);
        }

        #endregion
    }
}