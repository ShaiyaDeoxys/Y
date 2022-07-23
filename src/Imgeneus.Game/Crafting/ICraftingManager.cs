namespace Imgeneus.Game.Crafting
{
    public interface ICraftingManager
    {
        /// <summary>
        /// Chaotic square dialog box currently open.
        /// </summary>
        (byte Type, byte TypeId) ChaoticSquare { get; set; }

        /// <summary>
        /// Tries to craft item.
        /// </summary>
        /// <param name="bag">Chaotic square bag</param>
        /// <param name="slot">Chaotic square slot</param>
        /// <param name="index">craft item index</param>
        /// <param name="hammerBag">Hammer bag</param>
        /// <param name="hammerSlot">Hammer slot</param>
        /// <returns>true if success</returns>
        bool TryCraft(byte bag, byte slot, int index, byte hammerBag, byte hammerSlot);
    }
}
