using Imgeneus.Login.Packets;
using Imgeneus.Network.Client;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using LiteNetwork.Protocol.Abstractions;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker;
using System;
using System.Threading.Tasks;

namespace Imgeneus.Login
{
    public sealed class LoginClient : ImgeneusClient
    {
        private readonly IHandlerInvoker _handlerInvoker;
        private readonly ILoginPacketFactory _loginPacketFactory;

        /// <summary>
        /// Creates a new <see cref="LoginClient"/> instance.
        /// </summary>
        public LoginClient(ILogger<ImgeneusClient> logger, ICryptoManager cryptoManager, IServiceProvider serviceProvider, IHandlerInvoker handlerInvoker, ILoginPacketFactory loginPacketFactory):
            base(logger, cryptoManager, serviceProvider)
        {
            _handlerInvoker = handlerInvoker;
            _loginPacketFactory = loginPacketFactory;
        }

        private readonly PacketType[] _excludedPackets = new PacketType[] { PacketType.LOGIN_HANDSHAKE };
        public override PacketType[] ExcludedPackets { get => _excludedPackets; }

        public override Task InvokePacketAsync(PacketType type, ILitePacketStream packet)
        {
            return _handlerInvoker.InvokeAsync(_scope, type, this, packet);
        }

        protected override void OnConnected()
        {
            _logger.LogTrace("Got connection from {ip}. Sending handshake...", Socket.RemoteEndPoint);
            _loginPacketFactory.SendLoginHandshake(this);
        }
    }
}
