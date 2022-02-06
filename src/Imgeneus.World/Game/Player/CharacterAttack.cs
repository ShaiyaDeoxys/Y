using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Monster;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character : IKillable, IKiller
    {
        #region Damage calculation

        /// <summary>
        /// Uses skill or auto attack.
        /// </summary>
        private void Attack(byte skillNumber, IKillable target = null)
        {
            if (StealthManager.IsStealth && !StealthManager.IsAdminStealth)
            {
                var stealthBuff = BuffsManager.ActiveBuffs.FirstOrDefault(b => _databasePreloader.Skills[(b.SkillId, b.SkillLevel)].TypeDetail == TypeDetail.Stealth);
                stealthBuff.CancelBuff();
            }

            if (skillNumber == IAttackManager.AUTO_ATTACK_NUMBER)
            {
                AttackManager.AutoAttack();
            }
            else
            {
                if (!SkillsManager.Skills.TryGetValue(skillNumber, out var skill))
                {
                    _logger.LogWarning($"Character {Id} tries to use nonexistent skill.");
                    return;
                }

                if (skill.CastTime == 0)
                {
                    SkillsManager.UseSkill(skill, this, target);
                }
                else
                {
                    SkillsManager.StartCasting(skill, target);
                }
            }
        }

        #endregion
    }
}
