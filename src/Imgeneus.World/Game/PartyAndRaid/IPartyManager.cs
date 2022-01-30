namespace Imgeneus.World.Game.PartyAndRaid
{
    public interface IPartyManager
    {
        #region Party creation

        /// <summary>
        /// Id of character, that invites to the party.
        /// </summary>
        int InviterId { get; set; }

        #endregion
    }
}
