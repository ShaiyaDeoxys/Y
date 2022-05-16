using Imgeneus.Database.Constants;
using System;

namespace Imgeneus.World.Game.AI
{
    public interface IAIManager : IDisposable
    {
        void Init(int ownerId,
                  MobAI aiType, 
                  MoveArea moveArea,
                  int idleTime = 4000,
                  byte chaseRange = 10,
                  byte chaseSpeed = 5,
                  int chaseTime = 1,
                  byte idleSpeed = 1,
                  bool isAttack1Enabled = false,
                  bool isAttack2Enabled = false,
                  bool isAttack3Enabled = false,
                  byte attack1Range = 1,
                  byte attack2Range = 1,
                  byte attack3Range = 1,
                  ushort attackType1 = 0,
                  ushort attackType2 = 0,
                  ushort attackType3 = 0,
                  Element attackAttrib1 = Element.None,
                  Element attackAttrib2 = Element.None,
                  Element attackAttrib3 = Element.None,
                  ushort attack1 = 0,
                  ushort attack2 = 0,
                  ushort attack3 = 0,
                  ushort attackPlus1 = 0,
                  ushort attackPlus2 = 0,
                  ushort attackPlus3 = 0,
                  int attackTime1 = 0,
                  int attackTime2 = 0,
                  int attackTime3 = 0);

        /// <summary>
        /// Current ai state.
        /// </summary>
        AIState State { get; }

        /// <summary>
        /// Event, that is fired when AI state changes.
        /// </summary>
        event Action<AIState> OnStateChanged;

        /// <summary>
        /// Mob's ai type.
        /// </summary>
        MobAI AI { get; }

        /// <summary>
        /// Turns on ai of mob, based on its' type.
        /// </summary>
        void SelectActionBasedOnAI();
    }
}
