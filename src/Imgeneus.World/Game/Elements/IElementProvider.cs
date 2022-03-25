using Imgeneus.Database.Constants;

namespace Imgeneus.World.Game.Elements
{
    public interface IElementProvider
    {
        #region Attack

        /// <summary>
        /// Element set by skill.
        /// </summary>
        Element AttackSkillElement { get; set; }

        /// <summary>
        /// Const element for mobs from db, for players from weapon.
        /// </summary>
        Element ConstAttackElement { get; set; }

        /// <summary>
        /// Element used in weapon.
        /// </summary>
        Element AttackElement { get; }

        #endregion

        #region Defence

        /// <summary>
        /// Mages have skill, that removes element.
        /// </summary>
        bool IsRemoveElement { get; set; }

        /// <summary>
        /// Element set by skill.
        /// </summary>
        Element DefenceSkillElement { get; set; }

        /// <summary>
        /// Const element for mobs from db, for players from armor.
        /// </summary>
        Element ConstDefenceElement { get; set; }

        /// <summary>
        /// Element used in armor.
        /// </summary>
        Element DefenceElement { get; }

        #endregion
    }
}
