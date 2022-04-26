using Microsoft.Extensions.Logging;
using Parsec.Common;
using Parsec.Readers;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.GameDefinitions
{
    public class GameDefinitionsPreloder : IGameDefinitionsPreloder
    {
        private readonly ILogger<GameDefinitionsPreloder> _logger;

        public Dictionary<(byte Type, short TypeId), BaseNpc> NPCs { get; private set; } = new Dictionary<(byte Type, short TypeId), BaseNpc>();
        public Dictionary<short, Quest> Quests { get; private set; } = new Dictionary<short, Quest>();

        public GameDefinitionsPreloder(ILogger<GameDefinitionsPreloder> logger)
        {
            _logger = logger;
        }

        public void Preload()
        {
            try
            {
                //PreloadItems(_database);
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
        /// Preloads all available quests from NpcQuest.SData.
        /// </summary>
        private void PreloadNpcsAndQuests()
        {
            var npcQuest = Reader.ReadFromFile<NpcQuest>("config/SData/NpcQuest.SData", Format.EP8);

            foreach (var npc in npcQuest.Merchants)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Gatekeepers)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Blacksmiths)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.PvpManagers)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.GamblingHouses)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Warehouses)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.NormalNpcs)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Guards)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Animals)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.Apprentices)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.GuildMasters)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.DeadNpcs)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var npc in npcQuest.CombatCommanders)
                NPCs.Add(((byte)npc.Type, npc.TypeId), npc);

            foreach (var quest in npcQuest.Quests)
                Quests.Add(quest.Id, quest);
        }
    }
}