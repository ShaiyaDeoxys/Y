using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Login;
using InterServer.Common;
using InterServer.Server;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.Login.Handlers
{
    [Handler]
    public class HandshakeHandler
    {
        private readonly IInterServer _interServer;

        public HandshakeHandler(IInterServer interServer)
        {
            _interServer = interServer;
        }

        /// <summary>
        /// Handles the Handshake packet.
        /// </summary>
        /// <param name="client">Client.</param>
        /// <param name="pingPacket">Handshake packet.</param>
        [HandlerAction(PacketType.LOGIN_HANDSHAKE)]
        public void Handle(LoginClient sender, LoginHandshakePacket packet)
        {
            var decryptedNumber = sender.CryptoManager.DecryptRSA(packet.EncyptedNumber);
            sender.CryptoManager.GenerateAES(decryptedNumber);

            _interServer.Sessions.TryAdd(sender.Id, new KeyPair(sender.CryptoManager.Key, sender.CryptoManager.IV));
        }
    }
}