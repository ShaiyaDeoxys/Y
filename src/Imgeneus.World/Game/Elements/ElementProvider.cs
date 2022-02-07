using Imgeneus.Database.Constants;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Elements
{
    public class ElementProvider : IElementProvider
    {
        private readonly ILogger<ElementProvider> _logger;

        public ElementProvider(ILogger<ElementProvider> logger)
        {
            _logger = logger;

#if DEBUG
            _logger.LogDebug("ElementProvider {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~ElementProvider()
        {
            _logger.LogDebug("ElementProvider {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Attack

        public Element AttackSkillElement { get; set; }

        public Element ConstAttackElement { get; set; }

        public Element AttackElement
        {
            get
            {
                if (AttackSkillElement != Element.None)
                    return AttackSkillElement;

                return ConstAttackElement;
            }
        }

        #endregion

        #region Defence

        public bool IsRemoveElement { get; set; }

        public Element DefenceSkillElement { get; set; }

        public Element ConstDefenceElement { get; set; }

        public Element DefenceElement
        {
            get
            {
                if (IsRemoveElement)
                    return Element.None;

                if (DefenceSkillElement != Element.None)
                    return DefenceSkillElement;

                return ConstDefenceElement;
            }
        }

        #endregion
    }
}
