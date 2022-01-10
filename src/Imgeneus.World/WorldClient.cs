using Imgeneus.Network.Client;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using Imgeneus.World.Game.Session;
using LiteNetwork.Protocol.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World
{
    public sealed class WorldClient : ImgeneusClient, IWorldClient
    {
        private readonly IHandlerInvoker _handlerInvoker;

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

        protected override void OnDisconnected()
        {
            var gameSession = _scope.ServiceProvider.GetService<IGameSession>();
            gameSession.StartLogOff();

            base.OnDisconnected();
        }
    }
}
