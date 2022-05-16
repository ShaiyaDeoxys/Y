namespace Imgeneus.World.Game.Monster
{
    public partial class Mob
    {
        /// <summary>
        /// When user hits mob, it automatically turns on ai.
        /// </summary>
        private void OnDecreaseHP(int senderId, IKiller damageMaker)
        {
            if (!HealthManager.IsDead)
            {
                // TODO: calculate not only max damage, but also amount or rec and argo skills.
                if (HealthManager.MaxDamageMaker is IKillable)
                    AttackManager.Target = (HealthManager.MaxDamageMaker as IKillable);

                AIManager.SelectActionBasedOnAI();
            }
        }
    }
}
