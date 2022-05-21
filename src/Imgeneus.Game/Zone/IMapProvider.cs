namespace Imgeneus.World.Game.Zone
{
    public interface IMapProvider
    {
        /// <summary>
        /// Current map.
        /// </summary>
        Map Map { get; set; }

        /// <summary>
        /// Next map id.
        /// </summary>
        public ushort NextMapId { get; set; }

        /// <summary>
        /// Current cell id.
        /// </summary>
        int CellId { get; set; }

        /// <summary>
        /// Previous cell id.
        /// </summary>
        int OldCellId { get; set; }
    }
}
