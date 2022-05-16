using Microsoft.Extensions.Logging;
using Parsec;
using Parsec.Common;
using Parsec.Shaiya.Item;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.GameDefinitions
{
    public class GameDefinitionsPreloder : IGameDefinitionsPreloder
    {
        private readonly ILogger<GameDefinitionsPreloder> _logger;

        public Dictionary<(long Type, long TypeId), DBItemDataRecord> Items { get; init; } = new();
        public Dictionary<long, List<DBItemDataRecord>> ItemsByGrade { get; init; } = new();
        public Dictionary<(NpcType Type, short TypeId), BaseNpc> NPCs { get; init; } = new();
        public Dictionary<short, Quest> Quests { get; init; } = new();

        public GameDefinitionsPreloder(ILogger<GameDefinitionsPreloder> logger)
        {
            _logger = logger;
        }

        public void Preload()
        {
            try
            {
                PreloadItems();
                //PreloadSkills(_database);
                //PreloadMobs(_database);
                //PreloadMobItems(_database);
                PreloadNpcsAndQuests();
                //PreloadLevels(_database);

                _logger.LogInformation("Game definitions were successfully preloaded.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during preloading game definitions: {ex.Message}");
            }
        }

        /// <summary>
        /// Preloads all available items from DBItemData.SData.
        /// </summary>
        private void PreloadItems()
        {
            var items = Reader.ReadFromFile<DBItemData>("config/SData/DBItemData.SData");

            foreach (var item in items.Records)
            {
                Items.Add((item.ItemType, item.ItemTypeId), item);
                if (ItemsByGrade.ContainsKey(item.Grade))
                {
                    ItemsByGrade[item.Grade].Add(item);
                }
                else
                {
                    ItemsByGrade.Add(item.Grade, new List<DBItemDataRecord>() { item });
                }
            }
        }

        /// <summary>
        /// Preloads all available quests from NpcQuest.SData.
        /// </summary>
        private void PreloadNpcsAndQuests()
        {
            var npcQuest = Reader.ReadFromFile<NpcQuest>("config/SData/NpcQuest.SData", Format.EP8);

            foreach (var npc in npcQuest.Merchants)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Gatekeepers)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Blacksmiths)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.PvpManagers)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.GamblingHouses)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Warehouses)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.NormalNpcs)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Guards)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Animals)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Apprentices)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.GuildMasters)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.DeadNpcs)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.CombatCommanders)
                NPCs.Add((npc.Type, npc.TypeId), npc);

            foreach (var quest in npcQuest.Quests)
                Quests.Add(quest.Id, quest);
        }
    }
}