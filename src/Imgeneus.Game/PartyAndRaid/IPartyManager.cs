using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.PartyAndRaid
{
    public interface IPartyManager : ISessionedService, IDisposable
    {
        void Init(uint ownerId);

        /// <summary>
        /// Id of character, that invites to the party.
        /// </summary>
        uint InviterId { get; set; }

        /// <summary>
        /// Party or raid, in which player is currently.
        /// </summary>
        IParty Party { get; set; }

        /// <summary>
        /// Party Id, where player used to be.
        /// </summary>
        Guid PreviousPartyId { get; set; }

        /// <summary>
        /// Bool indicator, shows if player is in party/raid.
        /// </summary>
        bool HasParty { get; }

        /// <summary>
        /// Bool indicator, shows if player is the party/raid leader.
        /// </summary>
        bool IsPartyLead { get; }

        /// <summary>
        /// Bool indicator, shows if player is the raid subleader.
        /// </summary>
        bool IsPartySubLeader { get; }

        /// <summary>
        /// Event, that is fired, when player enters, leaves party or gets party leader.
        /// </summary>
        event Action<Character> OnPartyChanged;

        /// <summary>
        /// Sends summon request to party members.
        /// </summary>
        void SummonMembers(bool skeepTimer = false, Item summonItem = null);

        /// <summary>
        /// Event, that is fired, when summoning is started.
        /// </summary>
        event Action<uint> OnSummonning;

        /// <summary>
        /// Event, that is fired, when summoning is finished.
        /// </summary>
        event Action<uint> OnSummoned;

        /// <summary>
        /// Summoning is progress?
        /// </summary>
        bool IsSummoning { get; set; }

        /// <summary>
        /// Sets summon answer of this party member;
        /// </summary>
        void SetSummonAnswer(bool isOk);
    }
}
