using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using System.Collections.Generic;

namespace Imgeneus.World.Game.PartyAndRaid
{
    public class SummonRequest
    {
        /// <summary>
        /// Id of character, who started summonning.
        /// </summary>
        public int OwnerId { get; private set; }

        /// <summary>
        /// Answers of party members.
        /// </summary>
        public Dictionary<int, bool?> MemberAnswers { get; private init; } = new Dictionary<int, bool?>();

        /// <summary>
        /// Item from inventory, that should be used, if summoning success.
        /// </summary>
        public Item SummonItem { get; init; }

        public SummonRequest(int ownerId, Item summonItem)
        {
            OwnerId = ownerId;
            SummonItem = summonItem;
        }
    }
}
