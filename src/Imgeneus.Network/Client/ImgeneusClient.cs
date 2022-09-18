using Imgeneus.Network.PacketProcessor;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using LiteNetwork.Protocol.Abstractions;
using LiteNetwork.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.Network.Client
{
    /// <summary>
    /// TPC client, that is used for both login and game server.
    /// </summary>
    public abstract class ImgeneusClient : LiteServerUser
    {
        protected readonly ILogger<ImgeneusClient> _logger;
        protected IServiceScope _scope;

        private readonly ICryptoManager _cryptoManager;
        public ICryptoManager CryptoManager { get => _cryptoManager; }

        public bool IsDisposed { get; private set; }

        public ImgeneusClient(ILogger<ImgeneusClient> logger, ICryptoManager cryptoManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _cryptoManager = cryptoManager;
            _scope = serviceProvider.CreateScope();
        }

        public async override Task HandleMessageAsync(ILitePacketStream packet)
        {
            PacketType packetType = 0;
            ILitePacketStream decryptedPacket = packet;

            if (Socket == null)
            {
                _logger.LogTrace("Skip to handle login packet. Reason: client is not connected.");
                return;
            }

            try
            {
                packetType = (PacketType)packet.Read<ushort>();

                if (ExcludedPackets.Any(x => x == packetType) && CryptoManager.Key is null)
                {
                    decryptedPacket = packet;
                }
                else
                {
                    byte[] decrypted = CryptoManager.Decrypt(packet.Buffer);
                    decryptedPacket = new ImgeneusPacket(decrypted);
                    packetType = (PacketType)decryptedPacket.Read<ushort>();
                }

                if (packetType != PacketType.CHARACTER_MOVE)
                    _logger.LogTrace("Received {0} (0x{1}) packet from {2}.",
                                        packetType,
                                        ((ushort)packetType).ToString("X2"),
                                        Socket.RemoteEndPoint);

                if (!IsDisposed)
                    await InvokePacketAsync(packetType, decryptedPacket).ConfigureAwait(false);
            }
            catch (ArgumentException)
            {
                if (Enum.IsDefined(typeof(PacketType), packetType))
                {
                    _logger.LogTrace("Received an unimplemented Login packet {0} (0x{1}) from {2}.",
                        Enum.GetName(typeof(PacketType), packetType),
                        ((ushort)packetType).ToString("X2"),
                        Socket.RemoteEndPoint);
                }
                else
                {
                    _logger.LogTrace("Received an unknown Login packet 0x{0} from {1}.",
                        ((ushort)packetType).ToString("X2"),
                        Socket.RemoteEndPoint);
                }
            }
            catch (ObjectDisposedException)
            {
                // Packet handler is called after connection is closed. Nothing to do
            }
            catch (HandlerActionNotFoundException notFoundException)
            {
                _logger.LogWarning(notFoundException.Message);
            }
            catch (Exception exception)
            {
                _logger.LogError("An error occured while handling packet: {0}", exception.InnerException?.StackTrace);
            }
            finally
            {
                decryptedPacket.Dispose();
            }
        }

        /// <summary>
        /// Packets, that are excluded from enryption.
        /// </summary>
        public abstract PacketType[] ExcludedPackets { get; }

        /// <summary>
        /// Implement invoke strategy here.
        /// </summary>
        public abstract Task InvokePacketAsync(PacketType type, ILitePacketStream packet);

        /// <summary>
        /// Send packet to TCP client.
        /// </summary>
        /// <param name="packet">packet stream</param>
        /// <param name="shouldEncrypt">optional param, set to true if packet should be encrypted</param>
        public void Send(ImgeneusPacket packet, bool shouldEncrypt = true)
        {
            byte[] bytes;

            if (shouldEncrypt)
            {
                bytes = EncryptPacket(packet);
            }
            else
            {
                bytes = packet.Buffer;
            }

            Send(bytes);
        }


        /// <summary>
        /// Perform bytes encryption before each send to client.
        /// </summary>
        /// <param name="packet">Packet stream</param>
        /// <returns>encrypted bytes</returns>
        private byte[] EncryptPacket(ILitePacketStream packet)
        {
            var rawBytes = packet.Buffer;
            byte[] temp = new byte[rawBytes.Length - 2]; // Skip 2 bytes, because it's packet size and we should not encrypt them.
            Array.Copy(rawBytes, 2, temp, 0, rawBytes.Length - 2);

            // Calculated encrypted bytes.
            var encryptedBytes = CryptoManager.Encrypt(temp);

            var resultBytes = new byte[rawBytes.Length];

            // Copy packet length.
            resultBytes[0] = rawBytes[0];
            resultBytes[1] = rawBytes[1];

            // Copy encrypted bytes.
            for (var i = 0; i < encryptedBytes.Length; i++)
            {
                resultBytes[i + 2] = encryptedBytes[i];
            }

            return resultBytes;
        }


        /// <summary>
        /// Gets the client's logged user id.
        /// </summary>
        public int UserId { get; private set; }


        /// <summary>
        /// Sets the client's user id.
        /// </summary>
        /// <param name="userID">The client user id.</param>
        public void SetClientUserID(int userID)
        {
            if (UserId != 0)
                throw new InvalidOperationException("Client user ID already set.");

            UserId = userID;
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            _scope.Dispose();
            _scope = null;
            IsDisposed = true;

#if DEBUG
            // ONLY for debug! Don't uncomment it in prod.
            // It's for checking if every scoped service is finalized, when connection is closed.

            // Wait 0.5 sec after client disconnected.
            Task.Delay(500).ContinueWith((x) =>
            {
                // Collect everything.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.WaitForFullGCComplete();
            });
#endif
        }
    }
}
