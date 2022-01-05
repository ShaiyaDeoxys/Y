#if EP8_V1
using Imgeneus.World.Serialization.EP_8_V1;
#elif EP8_V2
using Imgeneus.World.Serialization.EP_8_V2;
#else
using Imgeneus.World.Serialization.SHAIYA_US;
#endif

using Imgeneus.Database.Entities;
using Imgeneus.Network.PacketProcessor;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using Imgeneus.World.SelectionScreen;
using System.Collections.Generic;
using System.Linq;
using System;
using Imgeneus.World.Game.Player;
using Imgeneus.Network.Serialization;

namespace Imgeneus.World.Packets
{
    public class GamePacketFactory : IGamePacketFactory
    {
        public void SendGameHandshake(WorldClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.GAME_HANDSHAKE);
            packet.WriteByte(0); // 0 means there was no error.
            packet.WriteByte(2); // no idea what is it, it just works.
            packet.Write(CryptoManager.XorKey);
            client.Send(packet);
        }

        public void SendCheckName(WorldClient client, bool isAvailable)
        {
            using var packet = new ImgeneusPacket(PacketType.CHECK_CHARACTER_AVAILABLE_NAME);
            packet.Write(isAvailable);
            client.Send(packet);
        }

        public void SendCreatedCharacter(WorldClient client, bool isCreated)
        {
            using var packet = new ImgeneusPacket(PacketType.CREATE_CHARACTER);
            packet.Write(isCreated ? 0 : 1); // 0 means character was created.
            client.Send(packet);
        }

        public void SendFaction(WorldClient client, Fraction faction, Mode maxMode)
        {
            using var packet = new ImgeneusPacket(PacketType.ACCOUNT_FACTION);
            packet.Write((byte)faction);
            packet.Write((byte)maxMode);
            client.Send(packet);
        }

        public void SendCharacterList(WorldClient client, IEnumerable<DbCharacter> characters)
        {
            var nonExistingCharacters = new List<ImgeneusPacket>();
            var existingCharacters = new List<ImgeneusPacket>();

            for (byte i = 0; i < SelectionScreenManager.MaxCharacterNumber; i++)
            {
                var packet = new ImgeneusPacket(PacketType.CHARACTER_LIST);
                packet.Write(i);
                var character = characters.FirstOrDefault(c => c.Slot == i && (!c.IsDelete || c.IsDelete && c.DeleteTime != null && DateTime.UtcNow.Subtract((DateTime)c.DeleteTime) < TimeSpan.FromHours(2)));
                if (character is null)
                {
                    // No char at this slot.
                    packet.Write(0);
                    nonExistingCharacters.Add(packet);
                }
                else
                {
                    packet.Write(new CharacterSelectionScreen(character).Serialize());
                    existingCharacters.Add(packet);
                }
            }

            foreach (var p in nonExistingCharacters)
                client.Send(p);

            foreach (var p in existingCharacters)
                client.Send(p);
        }

        public void SendCharacterSelected(WorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.SELECT_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendDeletedCharacter(WorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.DELETE_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendRestoredCharacter(WorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.RESTORE_CHARACTER);
            packet.Write((byte)(ok ? 0 : 1));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendRenamedCharacter(WorldClient client, bool ok, int id)
        {
            using var packet = new ImgeneusPacket(PacketType.RENAME_CHARACTER);
            packet.Write((byte)(ok ? 1 : 0));
            packet.Write(id);
            client.Send(packet);
        }

        public void SendDetails(WorldClient client, Character character)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_DETAILS);
            packet.Write(new CharacterDetails(character).Serialize());
            client.Send(packet);
        }

        public void SendSkillBar(WorldClient client, IEnumerable<DbQuickSkillBarItem> quickItems)
        {
            using var packet = new ImgeneusPacket(PacketType.CHARACTER_SKILL_BAR);
            packet.Write((byte)quickItems.Count());
            packet.Write(0); // Unknown int.

            foreach (var item in quickItems)
            {
                packet.Write(item.Bar);
                packet.Write(item.Slot);
                packet.Write(item.Bag);
                packet.Write(item.Number);
                packet.Write(0); // Unknown int.
            }

            client.Send(packet);
        }
    }
}
