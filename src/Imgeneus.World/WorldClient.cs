using Imgeneus.Network.Client;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Server.Crypto;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Shop;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Warehouse;
using LiteNetwork.Protocol.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        public IGameSession GameSession { get => _scope.ServiceProvider.GetService<IGameSession>(); }

        protected override void OnDisconnected()
        {
            if (IsDisposed)
                return;

            // When user closes game window, he is still in game for 10 secs.
            GameSession.StartLogOff(true);
        }

        public async Task ClearSession(bool quitGame = false)
        {
            var x = _scope.ServiceProvider;

            // Pay attention! Health should be saved before inventory.
            await x.GetService<IHealthManager>().Clear().ConfigureAwait(false);
            await x.GetService<IStatsManager>().Clear().ConfigureAwait(false);
            await x.GetService<IInventoryManager>().Clear().ConfigureAwait(false);
            await x.GetService<ISkillsManager>().Clear().ConfigureAwait(false);
            await x.GetService<IBuffsManager>().Clear().ConfigureAwait(false);
            await x.GetService<IKillsManager>().Clear().ConfigureAwait(false);
            await x.GetService<ITeleportationManager>().Clear().ConfigureAwait(false);
            await x.GetService<IPartyManager>().Clear().ConfigureAwait(false);
            await x.GetService<ITradeManager>().Clear().ConfigureAwait(false);
            await x.GetService<IDuelManager>().Clear().ConfigureAwait(false);
            await x.GetService<ILevelingManager>().Clear().ConfigureAwait(false);
            await x.GetService<IGuildManager>().Clear().ConfigureAwait(false);
            await x.GetService<IQuestsManager>().Clear().ConfigureAwait(false);
            await x.GetService<IAdditionalInfoManager>().Clear().ConfigureAwait(false);
            await x.GetService<IBankManager>().Clear().ConfigureAwait(false);
            await x.GetService<IWarehouseManager>().Clear().ConfigureAwait(false);
            await x.GetService<IShopManager>().Clear().ConfigureAwait(false);
            await x.GetService<IMovementManager>().Clear().ConfigureAwait(false);

            if (quitGame)
                base.OnDisconnected();
        }
    }
}
