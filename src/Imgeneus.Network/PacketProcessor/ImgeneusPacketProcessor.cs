using LiteNetwork.Protocol;
using LiteNetwork.Protocol.Abstractions;
using System;
using System.Linq;

namespace Imgeneus.Network.PacketProcessor
{
    public class ImgeneusPacketProcessor : LitePacketProcessor
    {
        public override int HeaderSize => 2;

        public override ILitePacketStream CreatePacket(byte[] buffer) => new ImgeneusPacket(buffer);

        public override int GetMessageLength(byte[] buffer)
        {
            if (buffer.Length < sizeof(int))
                Array.Resize(ref buffer, sizeof(int));

            return BitConverter.ToInt32(BitConverter.IsLittleEndian
               ? buffer.Take(sizeof(int)).ToArray()
               : buffer.Take(sizeof(int)).Reverse().ToArray(), 0) - HeaderSize;
        }
    }
}
