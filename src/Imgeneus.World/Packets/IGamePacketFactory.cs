using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Player;
using System.Collections.Generic;

namespace Imgeneus.World.Packets
{
    public interface IGamePacketFactory
    {
        #region Handshake
        void SendGameHandshake(IWorldClient IWorldClient);
        void SendLogout(IWorldClient client);
        void SendQuitGame(IWorldClient client);
        #endregion

        #region Selection screen
        void SendCreatedCharacter(IWorldClient client, bool isCreated);
        void SendCheckName(IWorldClient client, bool isAvailable);
        void SendFaction(IWorldClient client, Fraction faction, Mode maxMode);
        void SendCharacterList(IWorldClient client, IEnumerable<DbCharacter> characters);
        void SendCharacterSelected(IWorldClient client, bool ok, int id);
        void SendDeletedCharacter(IWorldClient client, bool ok, int id);
        void SendRestoredCharacter(IWorldClient client, bool ok, int id);
        void SendRenamedCharacter(IWorldClient client, bool ok, int id);
        #endregion

        #region Character
        void SendDetails(IWorldClient client, Character character);
        void SendAdditionalStats(IWorldClient client, Character character);
        void SendSkillBar(IWorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems);
        void SendAttribute(IWorldClient client, CharacterAttributeEnum attribute, uint attributeValue);
        void SendStatsUpdate(IWorldClient client, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc);
        #endregion

        #region Inventory
        void SendInventoryItems(IWorldClient client, ICollection<Item> inventoryItems);
        void SendItemExpiration(IWorldClient client, Item item);
        void SendAddItem(IWorldClient client, Item item);
        void SendMoveItem(IWorldClient client, Item sourceItem, Item destinationItem);
        void SendRemoveItem(IWorldClient client, Item item, bool fullRemove);
        void SendItemDoesNotBelong(IWorldClient client);
        void SendFullInventory(IWorldClient client);
        #endregion

        #region Map
        void SendCharacterMotion(IWorldClient client, int characterId, Motion motion);
        void SendCharacterChangedEquipment(IWorldClient client, int characterId, Item equipmentItem, byte slot);
        void SendCharacterShape(IWorldClient client, Character character);
        void SendShapeUpdate(IWorldClient client, Character character);
        void SendMaxHitpoints(IWorldClient client, int characterId, HitpointType type, int value);
        #endregion

        #region GM
        void SendGmCommandSuccess(IWorldClient client);
        void SendGmCommandError(IWorldClient client, PacketType error);
        #endregion
    }
}
