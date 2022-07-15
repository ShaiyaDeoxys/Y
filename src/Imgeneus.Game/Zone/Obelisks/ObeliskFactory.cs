using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.Monster;

namespace Imgeneus.World.Game.Zone.Obelisks
{
    public class ObeliskFactory : IObeliskFactory
    {
        private readonly IMobFactory _mobFactory;
        private readonly IBlessManager _blessManager;

        public ObeliskFactory(IMobFactory mobFactory, IBlessManager blessManager)
        {
            _mobFactory = mobFactory;
            _blessManager = blessManager;
        }

        public Obelisk CreateObelisk(ObeliskConfiguration config, Map map)
        {
            return new Obelisk(config, map, _mobFactory, _blessManager);
        }
    }
}
