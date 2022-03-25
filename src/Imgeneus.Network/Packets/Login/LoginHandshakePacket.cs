using Imgeneus.Network.PacketProcessor;
using System;
using System.Numerics;

namespace Imgeneus.Network.Packets.Login
{
    public record LoginHandshakePacket : IPacketDeserializer
    {
        public BigInteger EncyptedNumber { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            var length = packetStream.Read<byte>();
            // NB! 129 is one byte more, than client sends. The reason for this:
            // By creating a byte array either dynamically or statically without necessarily calling any of the previous methods, or by modifying an existing byte array.
            // To prevent positive values from being misinterpreted as negative values, you can add a zero-byte value to the end of the array.
            // You can read more here: https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger.-ctor
            // So, the last byte is always zero-byte.
            var encryptedBytes = new byte[length + 1];
            Array.Copy(packetStream.Buffer, 3, encryptedBytes, 0, length);

            EncyptedNumber = new BigInteger(encryptedBytes);
        }
    }
}
