using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Quests
{
    public interface IQuestsManager : ISessionedService
    {
        void Init(int ownerId, IEnumerable<DbCharacterQuest> quests);

        /// <summary>
        /// Collection of currently started quests.
        /// </summary>
        List<Quest> Quests { get; }

        /// <summary>
        /// Event, that is fired, when number of killed mobs for quest changes.
        /// </summary>
        event Action<short, byte, byte> OnQuestMobCountChanged;

        /// <summary>
        /// Event, that is fired, when quest is finished.
        /// </summary>
        event Action<Quest, int> OnQuestFinished;

        /// <summary>
        /// Tries to start new quest.
        /// </summary>
        /// <returns>true if quest is started</returns>
        Task<bool> TryStartQuest(int npcId, short questId);

        /// <summary>
        /// Changes number of killed mobs in quest.
        /// </summary>
        /// <param name="mobId">mob id</param>
        void UpdateQuestMobCount(ushort mobId);

        /// <summary>
        /// Use <see cref="UpdateQuestMobCount"/>
        /// </summary>
        void TryChangeMobCount(ushort mobId);

        /// <summary>
        /// Finished quest without success.
        /// </summary>
        void QuitQuest(short questId);

        /// <summary>
        /// Tries successfully finish quest.
        /// </summary>
        void TryFinishQuest(int npcId, short questId);
    }
}
