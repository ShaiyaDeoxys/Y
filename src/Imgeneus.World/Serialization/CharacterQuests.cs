using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Quests;
using System.Collections.Generic;

namespace Imgeneus.World.Serialization
{
    public class CharacterQuests : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Count;

        [FieldOrder(1)]
        [FieldCount(nameof(Count))]
        public List<CharacterQuest> Quests { get; } = new List<CharacterQuest>();

        public CharacterQuests(IEnumerable<Quest> quests)
        {
            foreach (var q in quests)
            {
                Quests.Add(new CharacterQuest(q));
            }
        }
    }
}
