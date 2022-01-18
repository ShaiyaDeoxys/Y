using Imgeneus.World.Game.Shape;
using System;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        /// <summary>
        /// Event, that is fired, when character changes shape.
        /// </summary>
        //public event Action<Character> OnShapeChange;

        /*public CharacterShape Shape
        {
            get
            {
                if (StealthManager.IsStealth)
                    return CharacterShape.Stealth;

                if (VehicleManager.IsOnVehicle)
                {
                    var value1 = (byte)InventoryManager.Mount.Grow >= 2 ? 15 : 14;
                    var value2 = InventoryManager.Mount.Range < 2 ? InventoryManager.Mount.Range * 2 : InventoryManager.Mount.Range + 7;
                    var mountType = value1 + value2;
                    return (CharacterShape)mountType;
                }

                return CharacterShape.None;
            }
        }*/
    }
}
