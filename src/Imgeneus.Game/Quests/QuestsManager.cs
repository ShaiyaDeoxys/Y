using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Quests
{
    public class QuestsManager : IQuestsManager
    {
        private readonly ILogger<QuestsManager> _logger;
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IDatabase _database;
        private readonly IPartyManager _partyManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly ILevelingManager _levelingManager;
        private uint _ownerId;

        public QuestsManager(ILogger<QuestsManager> logger, IGameDefinitionsPreloder definitionsPreloader, IMapProvider mapProvider, IGameWorld gameWorld, IDatabase database, IPartyManager partyManager, IInventoryManager inventoryManager, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, ILevelingManager levelingManager)
        {
            _logger = logger;
            _definitionsPreloader = definitionsPreloader;
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _database = database;
            _partyManager = partyManager;
            _inventoryManager = inventoryManager;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _levelingManager = levelingManager;
#if DEBUG
            _logger.LogDebug("QuestsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~QuestsManager()
        {
            _logger.LogDebug("QuestsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, IEnumerable<DbCharacterQuest> quests)
        {
            _ownerId = ownerId;

            foreach (var x in quests)
            {
                if (!_definitionsPreloader.Quests.ContainsKey(x.QuestId))
                    continue;

                var q = new Quest(_definitionsPreloader.Quests[x.QuestId], x);
                Quests.Add(q);
            }

            foreach (var quest in Quests)
                quest.QuestTimeElapsed += Quest_QuestTimeElapsed;
        }

        public async Task Clear()
        {
            foreach (var quest in Quests)
                quest.QuestTimeElapsed -= Quest_QuestTimeElapsed;

            foreach (var quest in Quests)
            {
                var dbCharacterQuest = _database.CharacterQuests.First(cq => cq.CharacterId == _ownerId && cq.QuestId == quest.Id);
                dbCharacterQuest.Delay = quest.RemainingTime;
                dbCharacterQuest.Count1 = quest.CountMob1;
                dbCharacterQuest.Count2 = quest.CountMob2;
                dbCharacterQuest.Count3 = quest.Count3;
                dbCharacterQuest.Finish = quest.IsFinished;
                dbCharacterQuest.Success = quest.IsSuccessful;

                quest.Dispose();
            }

            await _database.SaveChangesAsync();

            Quests.Clear();
        }

        #endregion

        #region Quests

        public List<Quest> Quests { get; init; } = new List<Quest>();

        public event Action<short, byte, byte> OnQuestMobCountChanged;

        public event Action<uint, Quest, bool> OnQuestFinished;

        private void Quest_QuestTimeElapsed(Quest quest)
        {
            quest.Finish(false);
            OnQuestFinished?.Invoke(0, quest, false);
        }

        public async Task<bool> TryStartQuest(uint npcId, short questId)
        {
            if (npcId != 0)
            {
                var npcQuestGiver = _mapProvider.Map.GetNPC(_gameWorld.Players[_ownerId].CellId, npcId);
                if (npcQuestGiver is null || !npcQuestGiver.StartQuestIds.Contains(questId))
                {
                    _logger.LogWarning("Trying to start unknown quest {id} at npc {npcId}", questId, npcId);
                    //return false;
                }
            }

            if (Quests.Any(x => x.Id == questId))
                return false;

            var quest = new Quest(_definitionsPreloader.Quests[questId]);
            quest.QuestTimeElapsed += Quest_QuestTimeElapsed;
            quest.StartQuestTimer();
            foreach (var item in quest.StartRequiredItems)
                _inventoryManager.AddItem(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, item.Type, item.TypeId, item.Count), "quest_start");

            Quests.Add(quest);

            var dbCharacterQuest = new DbCharacterQuest();
            dbCharacterQuest.CharacterId = _ownerId;
            dbCharacterQuest.QuestId = quest.Id;
            dbCharacterQuest.Delay = quest.RemainingTime;
            _database.CharacterQuests.Add(dbCharacterQuest);

            var count = await _database.SaveChangesAsync();
            return count > 0;
        }

        public void QuitQuest(short questId)
        {
            var quest = Quests.FirstOrDefault(q => q.Id == questId && !q.IsFinished);
            if (quest is null)
                return;

            quest.Finish(false);
            OnQuestFinished?.Invoke(0, quest, false);
        }

        public bool TryFinishQuest(uint npcId, short questId, out Quest quest)
        {
            quest = quest = Quests.FirstOrDefault(q => q.Id == questId && !q.IsFinished);
            if (quest is null)
                return false;

            if (npcId != 0 && quest.Config.EndNpcId != 0 && quest.Config.EndNpcType != 0)
            {
                var npcQuestReceiver = _mapProvider.Map.GetNPC(_gameWorld.Players[_ownerId].CellId, npcId);
                if (npcQuestReceiver is null || !npcQuestReceiver.EndQuestIds.Contains(questId))
                {
                    _logger.LogWarning("Trying to finish unknown quest {id} at npc {npcId}", questId, npcId);
                    return false;
                }
            }

            if (!quest.RequirementsFulfilled(_inventoryManager.InventoryItems.Values.ToList()))
                return false;

            // Remove farm items from inventory.
            var count1 = quest.FarmItemCount_1;
            var count2 = quest.FarmItemCount_2;
            var count3 = quest.FarmItemCount_3;
            foreach (var item in _inventoryManager.InventoryItems.Values)
            {
                if (count1 == 0 && count2 == 0 && count3 == 0)
                    break;

                if (count1 != 0)
                    if (item.Type == quest.FarmItemType_1 && item.TypeId == quest.FarmItemTypeId_1)
                    {
                        if (item.Count == count1)
                        {
                            count1 = 0;
                        }
                        else if (item.Count > count1)
                        {
                            item.TradeQuantity = count1;
                            count1 = 0;
                        }
                        else
                        {
                            count1 -= item.Count;
                        }

                        _inventoryManager.RemoveItem(item, "quest_complete");
                    }

                if (count2 != 0)
                    if (item.Type == quest.FarmItemType_2 && item.TypeId == quest.FarmItemTypeId_2)
                    {
                        if (item.Count == count2)
                        {
                            count2 = 0;
                        }
                        else if (item.Count > count2)
                        {
                            item.TradeQuantity = count2;
                            count2 = 0;
                        }
                        else
                        {
                            count2 -= item.Count;
                        }

                        _inventoryManager.RemoveItem(item, "quest_complete");
                    }

                if (count3 != 0)
                    if (item.Type == quest.FarmItemType_3 && item.TypeId == quest.FarmItemTypeId_3)
                    {
                        if (item.Count == count3)
                        {
                            count3 = 0;
                        }
                        else if (item.Count > count3)
                        {
                            item.TradeQuantity = count3;
                            count3 = 0;
                        }
                        else
                        {
                            count3 -= item.Count;
                        }

                        _inventoryManager.RemoveItem(item, "quest_complete");
                    }
            }

            if (!quest.CanChooseRevard)
            {
                foreach (var itm in quest.RevardItems)
                    _inventoryManager.AddItem(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, itm.Type, itm.TypeId, itm.Count), "quest_finish");

                if (quest.Gold > 0)
                    _inventoryManager.Gold += quest.Gold;

                if (quest.XP > 0)
                    _levelingManager.TryChangeExperience(_levelingManager.Exp + quest.XP * 10);

                quest.Finish(true);
            }

            OnQuestFinished?.Invoke(npcId, quest, true);
            return true;
        }

        public bool TryFinishQuestSelect(uint npcId, short questId, byte index)
        {
            var quest = Quests.FirstOrDefault(q => q.Id == questId && !q.IsFinished);
            if (quest is null)
                return false;

            if (npcId != 0 && quest.Config.EndNpcId != 0 && quest.Config.EndNpcType != 0)
            {
                var npcQuestReceiver = _mapProvider.Map.GetNPC(_gameWorld.Players[_ownerId].CellId, npcId);
                if (npcQuestReceiver is null || !npcQuestReceiver.EndQuestIds.Contains(questId))
                {
                    _logger.LogWarning("Trying to finish unknown quest {id} at npc {npcId}", questId, npcId);
                    return false;
                }
            }

            if (quest.RevardItems.Count < index || quest.RevardItems[index].Type == 0 || quest.RevardItems[index].TypeId == 0)
                return false;

            _inventoryManager.AddItem(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, quest.RevardItems[index].Type, quest.RevardItems[index].TypeId, quest.RevardItems[index].Count), "quest_finish");

            quest.Finish(true);
            return true;
        }

        public void UpdateQuestMobCount(ushort mobId)
        {
            if (_partyManager.HasParty)
            {
                foreach (var m in _partyManager.Party.Members)
                {
                    if (m.MapProvider.Map == _mapProvider.Map)
                        m.QuestsManager.TryChangeMobCount(mobId);
                }
            }
            else
            {
                TryChangeMobCount(mobId);
            }
        }

        public void TryChangeMobCount(ushort mobId)
        {
            var quests = Quests.Where(q => q.RequiredMobId_1 == mobId || q.RequiredMobId_2 == mobId);
            foreach (var q in quests)
            {
                if (q.RequiredMobId_1 == mobId)
                {
                    q.IncreaseCountMob1();
                    OnQuestMobCountChanged?.Invoke(q.Id, 0, q.CountMob1);
                }
                if (q.RequiredMobId_2 == mobId)
                {
                    q.IncreaseCountMob2();
                    OnQuestMobCountChanged?.Invoke(q.Id, 1, q.CountMob2);
                }
            }
        }

        #endregion
    }
}
