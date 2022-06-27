using Imgeneus.World.Game;
using System;

namespace Imgeneus.Game.Skills
{
    public interface ISkillCastingManager : IDisposable
    {
        void Init(uint ownerId);

        /// <summary>
        /// Starts casting.
        /// </summary>
        /// <param name="skill">skill, that we are casting</param>
        /// <param name="target">target for which, that we are casting</param>
        void StartCasting(Skill skill, IKillable target);

        /// <summary>
        /// Skill, that player tries to cast.
        /// </summary>
        Skill SkillInCast { get; }


        /// <summary>
        /// Event, that is fired, when user starts casting.
        /// </summary>
        event Action<uint, IKillable, Skill> OnSkillCastStarted;
    }
}
