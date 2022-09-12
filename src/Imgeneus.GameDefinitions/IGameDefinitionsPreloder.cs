using Parsec.Shaiya.Item;
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
        /// Preloaded items.
        /// </summary>
        Dictionary<(byte Type, byte TypeId), DbItem> Items { get; }

        /// <summary>
        /// Preloaded items based by grade.
        /// </summary>
        Dictionary<ushort, List<DbItem>> ItemsByGrade { get; }

        /// <summary>
        /// Preloaded skills.
        /// </summary>
        Dictionary<(ushort SkillId, byte SkillLevel), DbSkill> Skills { get; }

        /// <summary>
        /// Preloaded NPCs.
        /// </summary>
        Dictionary<(NpcType Type, short TypeId), BaseNpc> NPCs { get; }

        /// <summary>
        /// Preloaded quests.
        /// </summary>
        Dictionary<short, Quest> Quests { get; }

        /// <summary>
        /// Preloaded mobs.
        /// </summary>
        Dictionary<ushort, DbMob> Mobs { get; }

        /// <summary>
        /// Preloaded mob items.
        /// </summary>
        Dictionary<(ushort MobId, byte ItemOrder), DbMobItems> MobItems { get; }
    }
}
