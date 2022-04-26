using Microsoft.Extensions.Logging;
using Parsec.Common;
using Parsec.Readers;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.GameDefinitions
{
    public class GameDefinitionsPreloder : IGameDefinitionsPreloder
    {
        private readonly ILogger<GameDefinitionsPreloder> _logger;

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
                //PrealodNpcs(_database);
                PreloadQuests();
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
        private void PreloadQuests()
        {
            var npcQuest = Reader.ReadFromFile<NpcQuest>("config/SData/NpcQuest.SData", Format.EP8);
            foreach (var quest in npcQuest.Quests)
            {
                Quests.Add(quest.Id, quest);
            }
        }
    }
}