using Imgeneus.Network.PacketProcessor;
using System;

namespace Imgeneus.Network.Packets.Game
{
    public record InventorySortPacket : IPacketDeserializer
    {
        public byte Count { get; private set; }

        public SortInventoryItem[] Items { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Count = packetStream.Read<byte>();
            Items = new SortInventoryItem[Count];

            for (var i = 0; i < Count; i++)
            {
                Items[i] = new SortInventoryItem(packetStream.Read<byte>(), packetStream.Read<byte>(), packetStream.Read<byte>(), packetStream.Read<byte>());
            }
        }
    }

    public struct SortInventoryItem
    {
        public byte DestinationBag { get; }
        public byte DestinationSlot { get; }

        public byte SourceBag { get; }
        public byte SourceSlot { get; }

        public SortInventoryItem(byte destBag, byte destSlot, byte sourceBag, byte sourceSlot)
        {
            DestinationBag = destBag;
            DestinationSlot = destSlot;
            SourceBag = sourceBag;
            SourceSlot = sourceSlot;
        }
    }
}
