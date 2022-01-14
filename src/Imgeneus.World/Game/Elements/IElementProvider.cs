using Imgeneus.Database.Constants;

namespace Imgeneus.World.Game.Elements
{
    public interface IElementProvider
    {
        /// <summary>
        /// Element used in weapon.
        /// </summary>
        public Element AttackElement { get; set; }

        /// <summary>
        /// Element used in armor.
        /// </summary>
        public Element DefenceElement { get; set; }
    }
}
