using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player;
using System.Collections.Generic;

namespace Imgeneus.World.Packets
{
    public interface IGamePacketFactory
    {
        #region Handshake
        void SendGameHandshake(WorldClient worldClient);
        #endregion

        #region Selection screen
        void SendCreatedCharacter(WorldClient client, bool isCreated);
        void SendCheckName(WorldClient client, bool isAvailable);
        void SendFaction(WorldClient client, Fraction faction, Mode maxMode);
        void SendCharacterList(WorldClient client, IEnumerable<DbCharacter> characters);
        void SendCharacterSelected(WorldClient client, bool ok, int id);
        void SendDeletedCharacter(WorldClient client, bool ok, int id);
        void SendRestoredCharacter(WorldClient client, bool ok, int id);
        void SendRenamedCharacter(WorldClient client, bool ok, int id);
        #endregion

        #region Character
        void SendDetails(WorldClient client, Character character);
        void SendSkillBar(WorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems);
        #endregion
    }
}
