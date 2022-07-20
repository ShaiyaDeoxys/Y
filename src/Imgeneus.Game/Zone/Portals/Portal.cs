using Imgeneus.World.Game.Country;
using SPortal = Parsec.Shaiya.Svmap.Portal;

namespace Imgeneus.World.Game.Zone.Portals
{
    public class Portal
    {
        private readonly SPortal _config;
        private readonly float X1;
        private readonly float X2;
        private readonly float Y1;
        private readonly float Y2;
        private readonly float Z1;
        private readonly float Z2;

        public Portal(SPortal config)
        {
            _config = config;
            X1 = _config.Position.X - 5;
            X2 = _config.Position.X + 5;
            Y1 = _config.Position.Y - 5;
            Y2 = _config.Position.Y + 5;
            Z1 = _config.Position.Z - 5;
            Z2 = _config.Position.Z + 5;

            // > 2 is boss death portal
            IsOpen = config.FactionOrPortalId > 2 ? false : true;
        }

        /// <summary>
        /// Checks if character of some fraction can use this portal.
        /// </summary>
        public bool IsSameFaction(CountryType faction)
        {
            if (_config.FactionOrPortalId == 0)
                return true;

            if (faction == CountryType.Light && (int)_config.FactionOrPortalId == 1)
                return true;

            if (faction == CountryType.Dark && (int)_config.FactionOrPortalId == 2)
                return true;

            if ((int)_config.FactionOrPortalId > 2) // TODO: portal activated with boss death.
                return true;

            return false;
        }

        /// <summary>
        /// Checks if character can use this portal by level.
        /// </summary>
        public bool IsRightLevel(ushort level)
        {
            return level >= _config.MinLevel && level <= _config.MaxLevel;
        }

        /// <summary>
        /// Checks if character is in portal zone.
        /// </summary>
        /// <param name="x">player x coordinate</param>
        /// <param name="y">player y coordinate</param>
        /// <param name="z">player z coordinate</param>
        public bool IsInPortalZone(float x, float y, float z)
        {
            return x >= X1 && x <= X2 &&
                   y >= Y1 && y <= Y2 &&
                   z >= Z1 && z <= Z2;
        }

        public ushort MapId => (ushort)_config.DestinationMapId;

        public float Destination_X => _config.DestinationPosition.X;

        public float Destination_Y => _config.DestinationPosition.Y;

        public float Destination_Z => _config.DestinationPosition.Z;

        /// <summary>
        /// Opened by boss death.
        /// </summary>
        public bool IsOpen { get; set; }

        public int PortalId { get => _config.FactionOrPortalId; }
    }

}
