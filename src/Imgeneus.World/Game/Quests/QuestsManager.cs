using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Inventory;
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
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IDatabase _database;
        private readonly IPartyManager _partyManager;
        private readonly IInventoryManager _inventoryManager;
        private int _ownerId;

        public QuestsManager(ILogger<QuestsManager> logger, IDatabasePreloader databasePreloader, IMapProvider mapProvider, IGameWorld gameWorld, IDatabase database, IPartyManager partyManager, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _databasePreloader = databasePreloader;
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _database = database;
            _partyManager = partyManager;
            _inventoryManager = inventoryManager;
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

        public void Init(int ownerId, IEnumerable<DbCharacterQuest> quests)
        {
            _ownerId = ownerId;

            Quests.AddRange(quests.Select(x => new Quest(_databasePreloader, x)));

            foreach (var quest in Quests)
                quest.QuestTimeElapsed += Quest_QuestTimeElapsed;
        }

        public async Task Clear()
        {
            foreach (var quest in Quests)
                quest.QuestTimeElapsed -= Quest_QuestTimeElapsed;

            foreach (var quest in Quests.Where(q => q.SaveUpdateToDatabase))
            {
                var dbCharacterQuest = _database.CharacterQuests.First(cq => cq.CharacterId == _ownerId && cq.QuestId == quest.Id);
                dbCharacterQuest.Delay = quest.RemainingTime;
                dbCharacterQuest.Count1 = quest.CountMob1;
                dbCharacterQuest.Count2 = quest.CountMob2;
                dbCharacterQuest.Count3 = quest.Count3;
                dbCharacterQuest.Finish = quest.IsFinished;
                dbCharacterQuest.Success = quest.IsSuccessful;
            }

            await _database.SaveChangesAsync();

            Quests.Clear();
        }

        #endregion

        #region Quests

        public List<Quest> Quests { get; init; } = new List<Quest>();

        public event Action<ushort, byte, byte> OnQuestMobCountChanged;

        public event Action<Quest, int> OnQuestFinished;

        private void Quest_QuestTimeElapsed(Quest quest)
        {
            //SendQuestFinished(quest);
        }

        public async Task<bool> TryStartQuest(int npcId, ushort questId)
        {
            var npcQuestGiver = _mapProvider.Map.GetNPC(_gameWorld.Players[_ownerId].CellId, npcId);
            if (npcQuestGiver is null || !npcQuestGiver.StartQuestIds.Contains(questId))
            {
                _logger.LogWarning("Trying to start unknown quest {id} at npc {npcId}", questId, npcId);
                return false;
            }

            var quest = new Quest(_databasePreloader, questId);
            quest.QuestTimeElapsed += Quest_QuestTimeElapsed;
            quest.StartQuestTimer();
            Quests.Add(quest);

            var dbCharacterQuest = new DbCharacterQuest();
            dbCharacterQuest.CharacterId = _ownerId;
            dbCharacterQuest.QuestId = quest.Id;
            dbCharacterQuest.Delay = quest.RemainingTime;
            _database.CharacterQuests.Add(dbCharacterQuest);

            var count = await _database.SaveChangesAsync();
            return count > 0;
        }

        public void QuitQuest(ushort questId)
        {
            var quest = Quests.FirstOrDefault(q => q.Id == questId && !q.IsFinished);
            if (quest is null)
                return;

            quest.Finish(false);
            OnQuestFinished?.Invoke(quest, 0);
        }

        public void TryFinishQuest(int npcId, ushort questId)
        {
            var npcQuestReceiver = _mapProvider.Map.GetNPC(_gameWorld.Players[_ownerId].CellId, npcId);
            if (npcQuestReceiver is null || !npcQuestReceiver.EndQuestIds.Contains(questId))
            {
                _logger.LogWarning("Trying to finish unknown quest {id} at npc {npcId}", questId, npcId);
                return;
            }

            var quest = Quests.FirstOrDefault(q => q.Id == questId && !q.IsFinished);
            if (quest is null)
                return;
            if (!quest.RequirementsFulfilled(_inventoryManager.InventoryItems.Values.ToList()))
                return;

            // TODO: remove items from inventory.

            // TODO: add reward to player.

            quest.Finish(true);
            OnQuestFinished?.Invoke(quest, npcId);
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
