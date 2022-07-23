namespace Imgeneus.Game.Crafting
{
    public interface ICraftingManager
    {
        /// <summary>
        /// Chaotic square dialog box currently open.
        /// </summary>
        (byte Type, byte TypeId) ChaoticSquare { get; set; }
    }
}
