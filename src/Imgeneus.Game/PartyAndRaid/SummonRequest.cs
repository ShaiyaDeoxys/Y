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
        public uint OwnerId { get; private set; }

        /// <summary>
        /// Answers of party members.
        /// </summary>
        public Dictionary<uint, bool?> MemberAnswers { get; private init; } = new Dictionary<uint, bool?>();

        /// <summary>
        /// Item from inventory, that should be used, if summoning success.
        /// </summary>
        public Item SummonItem { get; init; }

        public SummonRequest(uint ownerId, Item summonItem)
        {
            OwnerId = ownerId;
            SummonItem = summonItem;
        }
    }
}
