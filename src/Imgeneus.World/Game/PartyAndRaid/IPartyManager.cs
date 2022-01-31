using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.PartyAndRaid
{
    public interface IPartyManager : ISessionedService
    {
        void Init(int ownerId);

        /// <summary>
        /// Id of character, that invites to the party.
        /// </summary>
        int InviterId { get; set; }

        /// <summary>
        /// Party or raid, in which player is currently.
        /// </summary>
        IParty Party { get; set; }

        /// <summary>
        /// Party Id, where player used to be.
        /// </summary>
        Guid PreviousPartyId { get; }

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
    }
}
