using Imgeneus.Network.Client;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using LiteNetwork.Protocol.Abstractions;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World
{
    public sealed class WorldClient : ImgeneusClient
    {
        private readonly IHandlerInvoker _handlerInvoker;

        /// <summary>
        /// Gets the client's logged char id.
        /// </summary>
        public int CharID { get; set; }

        /// <summary>
        /// Is character about to log off?
        /// </summary>
        public bool IsLoggingOff;

        public WorldClient(ILogger<ImgeneusClient> logger, ICryptoManager cryptoManager, IServiceProvider serviceProvider, IHandlerInvoker handlerInvoker):
            base(logger, cryptoManager, serviceProvider)
        {
            _handlerInvoker = handlerInvoker;
        }

        private readonly PacketType[] _excludedPackets = new PacketType[] { PacketType.GAME_HANDSHAKE };
        public override PacketType[] ExcludedPackets { get => _excludedPackets; }

        public override Task InvokePacketAsync(PacketType type, ILitePacketStream packet)
        {
            // TODO: create mixed strategy, where some packets are called sync and some async.
            return _handlerInvoker.InvokeAsync(_scope, type, this, packet);
        }
    }
}
