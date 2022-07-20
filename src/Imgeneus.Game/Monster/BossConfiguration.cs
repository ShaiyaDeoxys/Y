using System.Collections.Generic;

namespace Imgeneus.Game.Monster
{
    public class MapBossConfigurations
    {
        public IEnumerable<MapBossConfiguration> Maps { get; set; }
    }

    public class MapBossConfiguration
    {
        public ushort MapId { get; set; }

        public IEnumerable<BossConfiguration> MobBosses { get; set; }
    }


    public class BossConfiguration
    {
        public ushort MobId { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public ushort Portal { get; set; }

        public uint RespawnTimeInSeconds { get; set; }
    }
}
