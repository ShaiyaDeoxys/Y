using Imgeneus.Database.Constants;
using Imgeneus.GameDefinitions.Constants;
using System;

namespace Imgeneus.World.Game.AI
{
    public interface IAIManager : IDisposable
    {
        void Init(uint ownerId,
                  MobAI aiType, 
                  MoveArea moveArea,
                  int idleTime = 4000,
                  byte chaseRange = 10,
                  byte chaseSpeed = 5,
                  int chaseTime = 1000,
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

        /// <summary>
        /// Tries to get the nearest player on the map.
        /// </summary>
        bool TryGetEnemy();

        /// <summary>
        /// Uses some attack.
        /// </summary>
        /// <param name="skillId">skill id</param>
        /// <param name="minAttack">min damage</param>
        /// <param name="element">element</param>
        /// <param name="additionalDamage">plus damage</param>
        void Attack(ushort skillId, Element element, ushort minAttack, ushort additionalDamage, int attackRange);

        /// <summary>
        /// Moves AI to the specified position.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="z">z coordinate</param>
        void Move(float x, float z);

        /// <summary>
        /// Starts AI.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops AI.
        /// </summary>
        void Stop();
    }
}
