using Imgeneus.Network.Client;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Trade;
using LiteNetwork.Protocol.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World
{
    public sealed class WorldClient : ImgeneusClient, IWorldClient
    {
        private readonly IHandlerInvoker _handlerInvoker;

        public WorldClient(ILogger<ImgeneusClient> logger, ICryptoManager cryptoManager, IServiceProvider serviceProvider, IHandlerInvoker handlerInvoker) :
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
            if (IsDisposed)
                return;

            var gameSession = _scope.ServiceProvider.GetService<IGameSession>();
            gameSession.StartLogOff(true);
        }

        public async Task ClearSession(bool quitGame = false)
        {
            var x = _scope.ServiceProvider;

            var tasks = new List<Task>();

            tasks.Add(x.GetService<IInventoryManager>().Clear());
            tasks.Add(x.GetService<ISkillsManager>().Clear());
            tasks.Add(x.GetService<IBuffsManager>().Clear());
            tasks.Add(x.GetService<IKillsManager>().Clear());
            tasks.Add(x.GetService<ITeleportationManager>().Clear());
            tasks.Add(x.GetService<IHealthManager>().Clear());
            tasks.Add(x.GetService<IPartyManager>().Clear());
            tasks.Add(x.GetService<ITradeManager>().Clear());
            tasks.Add(x.GetService<IDuelManager>().Clear());

            await Task.WhenAll(tasks).ConfigureAwait(false);

            if (quitGame)
                base.OnDisconnected();
        }
    }
}
