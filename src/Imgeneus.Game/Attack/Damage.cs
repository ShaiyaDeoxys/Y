namespace Imgeneus.World.Game.Attack
{
    public struct Damage
    {
        public ushort HP { get; set; }
        public ushort SP { get; set; }
        public ushort MP { get; set; }

        public Damage(ushort hp, ushort sp, ushort mp)
        {
            HP = hp;
            SP = sp;
            MP = mp;
        }
    }
}
