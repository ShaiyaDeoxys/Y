using Imgeneus.Network.Packets;
using LiteNetwork.Protocol;
using System;
using System.IO;
using System.Text;

namespace Imgeneus.Network.PacketProcessor
{
    public class ImgeneusPacket : LitePacketStream
    {
        public ImgeneusPacket()
        {
            WriteInt16(0); // Write header bytes.
        }

        public ImgeneusPacket(PacketType packetType): this()
        {
            Write((ushort)packetType);
        }

        public ImgeneusPacket(byte[] buffer)
            : base(buffer)
        {
        }

        public const int HeaderSize = sizeof(ushort);

        public override byte[] Buffer
        {
            get
            {
                if (Mode == LitePacketMode.Write)
                {
                    long oldPosition = Position;

                    Seek(0, SeekOrigin.Begin);
                    Write((ushort)Length);
                    Seek((int)oldPosition, SeekOrigin.Begin);
                }

                return base.Buffer;
            }
        }

        #region Read helpers

        /// <summary>
        /// Reads a string with a specific length from the current packet stream.
        /// </summary>
        /// <param name="size">length in bytes</param>
        /// <param name="encoding">optional param Encoding</param>
        public string ReadString(int size, Encoding? encoding = null)
        {
            if (Mode != LitePacketMode.Read)
                throw new InvalidOperationException("Packet is in write-only mode.");

            if (size == 0)
                return string.Empty;

            if (encoding is null)
                encoding = Encoding.Default;

            if (encoding == Encoding.Unicode)
                size *= 2; // unicode is 2-byte per character encoding

            var value = encoding.GetString(ReadBytes(size));

            if (value.IndexOf("\0", StringComparison.Ordinal) < 0)
                return value;

            return value.Substring(0, value.IndexOf("\0", StringComparison.Ordinal));
        }

        #endregion

        #region Write helpers

        /// <summary>
        /// Writes a string with specified length to the current packet stream.
        /// </summary>
        public void WriteString(string value, int count, Encoding? encoding = null)
        {
            if (Mode != LitePacketMode.Write)
                throw new InvalidOperationException("Packet is in read-only mode.");

            if (value == null)
                throw new ArgumentNullException("The string value can't be null.");

            if (value.Length > count)
                throw new InvalidOperationException("The string is too big.");

            if (encoding is null)
                encoding = Encoding.UTF8;

            var length = value.Length;
            if (encoding == Encoding.Unicode)
            {
                count *= 2; // unicode is 2-byte per character encoding
                length *= 2;
            }

            byte[] buffer = new byte[count];

            byte[] stuff = encoding.GetBytes(value);

            System.Buffer.BlockCopy(stuff, 0, buffer, 0, length);

            Write(buffer);
        }

        /// <summary>
        /// Writes padded bytes (with 0) to packet stream.
        /// </summary>
        /// <param name="bytes">bytes that we want to write to stream</param>
        /// <param name="count">size of padded 0 bytes</param>
        public void WritePaddedBytes(byte[] bytes, int count)
        {
            if (Mode != LitePacketMode.Write)
                throw new InvalidOperationException($"The current packet stream is in read-only mode.");

            if (bytes.Length > count)
                throw new InvalidOperationException("The byte array is too big.");

            var buffer = new byte[count];
            System.Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);

            foreach (var x in buffer)
                Write(x);
        }

        /// <summary>
        /// Writes bytes array to stream.
        /// </summary>
        public void Write(byte[] bytes) => WriteBytes(bytes);

        #endregion
    }
}
