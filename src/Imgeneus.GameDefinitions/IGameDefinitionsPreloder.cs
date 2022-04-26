using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.GameDefinitions
{
    public interface IGameDefinitionsPreloder
    {
        /// <summary>
        /// Preloads all needed game definitions from SData files.
        /// </summary>
        void Preload();

        /// <summary>
        /// Preloaded quests.
        /// </summary>
        Dictionary<short, Quest> Quests { get; }
    }
}
