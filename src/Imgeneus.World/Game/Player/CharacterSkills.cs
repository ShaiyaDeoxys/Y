using Imgeneus.Database.Constants;
using Imgeneus.DatabaseBackgroundService.Handlers;
using System;
using System.Linq;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Attack;

namespace Imgeneus.World.Game.Player
{
    public partial class Character : IKillable
    {
        /// <summary>
        /// Initialize passive skills.
        /// </summary>
        public void InitPassiveSkills()
        {
            foreach (var skill in SkillsManager.Skills.Values.Where(s => s.IsPassive && s.Type != TypeDetail.Stealth))
            {
                SkillsManager.UseSkill(skill, this);
            }
        }

        /// <summary>
        /// Clears skills and adds skill points.
        /// </summary>
        public void ResetSkills()
        {
            ushort skillFactor = _characterConfig.GetLevelStatSkillPoints(LevelingManager.Grow).SkillPoint;

            SkillsManager.TrySetSkillPoints((ushort)(skillFactor * (LevelProvider.Level - 1)));

            _taskQueue.Enqueue(ActionType.REMOVE_ALL_SKILLS, Id);
            //_taskQueue.Enqueue(ActionType.SAVE_CHARACTER_SKILLPOINT, Id, SkillPoint);

            SendResetSkills();

            foreach (var passive in BuffsManager.PassiveBuffs.ToList())
                passive.CancelBuff();

            SkillsManager.Skills.Clear();
        }
    }
}
