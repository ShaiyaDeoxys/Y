using Imgeneus.Database.Constants;
using Imgeneus.GameDefinitions.Constants;
using Parsec.Shaiya.Monster;

namespace Imgeneus.GameDefinitions
{
    public class DbMob
    {
        public DbMob(DBMonsterDataRecord data)
        {
            Id = (ushort)data.Id;
            Level = (ushort)data.Level;
            Exp = (short)data.Exp;
            AI = (MobAI)data.Ai;
            MoneyMin = (short)data.Money1;
            MoneyMax = (short)data.Money2;
            QuestItemId = (int)data.QuestItem;
            HP = (int)data.Hp;
            SP = (short)data.Sp;
            MP = (short)data.Mp;
            Dex = (ushort)data.Dex;
            Wis = (ushort)data.Wis;
            Luc = (ushort)data.Luc;
            Day = (byte)data.Day;
            Element = (Element)data.Attrib;
            Defense = (ushort)data.Defense;
            Magic = (ushort)data.Magic;
            ResistState1 = (byte)data.State1;
            ResistState2 = (byte)data.State2;
            ResistState3 = (byte)data.State3;
            ResistState4 = (byte)data.State4;
            ResistState5 = (byte)data.State5;
            ResistState6 = (byte)data.State6;
            ResistState7 = (byte)data.State7;
            ResistState8 = (byte)data.State8;
            ResistState9 = (byte)data.State9;
            ResistState10 = (byte)data.State10;
            ResistState11 = (byte)data.State11;
            ResistState12 = (byte)data.State12;
            ResistState13 = (byte)data.State13;
            ResistState14 = (byte)data.State14;
            ResistState15 = (byte)data.State15;
            ResistSkill1 = (byte)data.Skill1;
            ResistSkill2 = (byte)data.Skill2;
            ResistSkill3 = (byte)data.Skill3;
            ResistSkill4 = (byte)data.Skill4;
            ResistSkill5 = (byte)data.Skill5;
            ResistSkill6 = (byte)data.Skill6;
            NormalTime = (int)data.NormalTime;
            NormalStep = (byte)data.NormalStep;
            ChaseTime = (int)data.ChaseTime;
            ChaseStep = (byte)data.ChaseStep;
            ChaseRange = (byte)data.ChaseRange;
            AttackType1 = (ushort)data.AttackType1;
            AttackTime1 = (int)data.AttackTime1;
            AttackRange1 = (byte)data.AttackRange1;
            Attack1 = (short)data.Attack1;
            AttackPlus1 = (ushort)data.AttackPlus1;
            AttackAttrib1 = (Element)data.AttackAttrib1;
            AttackSpecial1 = (byte)data.AttackSpecial1;
            AttackOk1 = (byte)data.AttackOk1;

            AttackType2 = (ushort)data.AttackType2;
            AttackTime2 = (int)data.AttackTime2;
            AttackRange2 = (byte)data.AttackRange2;
            Attack2 = (short)data.Attack2;
            AttackPlus2 = (ushort)data.AttackPlus2;
            AttackAttrib1 = (Element)data.AttackAttrib2;
            AttackSpecial2 = (byte)data.AttackSpecial2;
            AttackOk2 = (byte)data.AttackOk2;

            AttackType3 = (ushort)data.AttackType3;
            AttackTime3 = (int)data.AttackTime3;
            AttackRange3 = (byte)data.AttackRange3;
            Attack3 = (short)data.Attack3;
            AttackPlus3 = (ushort)data.AttackPlus3;
            Fraction = (MobFraction)data.AttackAttrib3;
            AttackSpecial3 = (MobRespawnTime)data.AttackSpecial3;
            AttackOk3 = (byte)data.AttackOk3;
        }

        public DbMob()
        {
            // For tests.
        }

        /// <summary>
        /// Unique id.
        /// </summary>
        public ushort Id { get; set; }

        /// <summary>
        /// Mob level.
        /// </summary>
        public ushort Level { get; set; }

        /// <summary>
        /// Experience, that character gets, when kills the mob.
        /// </summary>
        public short Exp { get; set; }

        /// <summary>
        /// Ai type.
        /// </summary>
        public MobAI AI { get; set; }

        /// <summary>
        /// Min amount of money, that character can get from the mob.
        /// </summary>
        public short MoneyMin { get; set; }

        /// <summary>
        /// Max amount of money, that character can get from the mob.
        /// During GRB it's number of guild points.
        /// </summary>
        public short MoneyMax { get; set; }

        /// <summary>
        /// Drops secret quest item.
        /// </summary>
        public int QuestItemId { get; set; }

        /// <summary>
        /// Health points.
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// Stamina points.
        /// </summary>
        public short SP { get; set; }

        /// <summary>
        /// Mana points.
        /// </summary>
        public short MP { get; set; }

        /// <summary>
        /// Mob's dexterity.
        /// </summary>
        public ushort Dex { get; set; }

        /// <summary>
        /// Mob's wisdom.
        /// </summary>
        public ushort Wis { get; set; }

        /// <summary>
        /// Mob's luck.
        /// </summary>
        public ushort Luc { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public byte Day { get; set; }

        /// <summary>
        /// Mob's element.
        /// </summary>
        public Element Element { get; set; }

        /// <summary>
        /// Mob's defense.
        /// </summary>
        public ushort Defense { get; set; }

        /// <summary>
        /// Mob's magic defense.
        /// </summary>
        public ushort Magic { get; set; }

        /// <summary>
        /// Resist sleep.
        /// </summary>
        public byte ResistState1 { get; set; }

        /// <summary>
        /// Resist stun.
        /// </summary>
        public byte ResistState2 { get; set; }

        /// <summary>
        /// Resist silent.
        /// </summary>
        public byte ResistState3 { get; set; }

        /// <summary>
        /// Resist darkness.
        /// </summary>
        public byte ResistState4 { get; set; }

        /// <summary>
        /// Resist immobilize.
        /// </summary>
        public byte ResistState5 { get; set; }

        /// <summary>
        /// Resist slow.
        /// </summary>
        public byte ResistState6 { get; set; }

        /// <summary>
        /// Resist dying.
        /// </summary>
        public byte ResistState7 { get; set; }

        /// <summary>
        /// Resist death.
        /// </summary>
        public byte ResistState8 { get; set; }

        /// <summary>
        /// Resist poison.
        /// </summary>
        public byte ResistState9 { get; set; }

        /// <summary>
        /// Resist illeness.
        /// </summary>
        public byte ResistState10 { get; set; }

        /// <summary>
        /// Resist delusion.
        /// </summary>
        public byte ResistState11 { get; set; }

        /// <summary>
        /// Resist doom.
        /// </summary>
        public byte ResistState12 { get; set; }

        /// <summary>
        /// Resist fear.
        /// </summary>
        public byte ResistState13 { get; set; }

        /// <summary>
        /// Resist dull.
        /// </summary>
        public byte ResistState14 { get; set; }

        /// <summary>
        /// Resist bad luck.
        /// </summary>
        public byte ResistState15 { get; set; }

        public byte ResistSkill1 { get; set; }
        public byte ResistSkill2 { get; set; }
        public byte ResistSkill3 { get; set; }
        public byte ResistSkill4 { get; set; }
        public byte ResistSkill5 { get; set; }
        public byte ResistSkill6 { get; set; }

        /// <summary>
        /// Delay in idle state.
        /// </summary>
        public int NormalTime { get; set; }

        /// <summary>
        /// Speed of mob in idle state.
        /// </summary>
        public byte NormalStep { get; set; }

        /// <summary>
        /// Delay in chase state.
        /// </summary>
        public int ChaseTime { get; set; }

        /// <summary>
        /// Speed of mob in chase state.
        /// </summary>
        public byte ChaseStep { get; set; }

        /// <summary>
        /// How far mob will chase player. Also vision of mob.
        /// </summary>
        public byte ChaseRange { get; set; }

        #region Attack 1

        /// <summary>
        ///  List of skills (NpcSkills.SData).
        /// </summary>
        public ushort AttackType1 { get; set; }

        /// <summary>
        /// Delay.
        /// </summary>
        public int AttackTime1 { get; set; }

        /// <summary>
        /// Range.
        /// </summary>
        public byte AttackRange1 { get; set; }

        /// <summary>
        /// Damage.
        /// </summary>
        public short Attack1 { get; set; }

        /// <summary>
        /// Additional damage.
        /// </summary>
        public ushort AttackPlus1 { get; set; }

        /// <summary>
        /// Element.
        /// </summary>
        public Element AttackAttrib1 { get; set; }

        /// <summary>
        /// Param.
        /// </summary>
        public byte AttackSpecial1 { get; set; }

        /// <summary>
        /// On/off.
        /// </summary>
        public byte AttackOk1 { get; set; }
        #endregion

        #region Attack 2

        /// <summary>
        ///  List of skills (NpcSkills.SData).
        /// </summary>
        public ushort AttackType2 { get; set; }

        /// <summary>
        /// Delay.
        /// </summary>
        public int AttackTime2 { get; set; }

        /// <summary>
        /// Range.
        /// </summary>
        public byte AttackRange2 { get; set; }

        /// <summary>
        /// Damage.
        /// </summary>
        public short Attack2 { get; set; }

        /// <summary>
        /// Additional damage.
        /// </summary>
        public ushort AttackPlus2 { get; set; }

        /// <summary>
        /// Element.
        /// </summary>
        public Element AttackAttrib2 { get; set; }

        /// <summary>
        /// Param.
        /// </summary>
        public byte AttackSpecial2 { get; set; }

        /// <summary>
        /// On/off.
        /// </summary>
        public byte AttackOk2 { get; set; }
        #endregion

        //  AtkAttack.3 is the respawn delay (if presented?)
        // https://www.elitepvpers.com/forum/shaiya-pserver-development/3532706-how-change-mob-respawns-time.html#post30486297
        // https://www.elitepvpers.com/forum/shaiya-pserver-development/4298648-question-mob-respawn-time.html
        #region Attack 3

        /// <summary>
        ///  List of skills (NpcSkills.SData).
        /// </summary>
        public ushort AttackType3 { get; set; }

        /// <summary>
        /// Delay.
        /// </summary>
        public int AttackTime3 { get; set; }

        /// <summary>
        /// Range.
        /// </summary>
        public byte AttackRange3 { get; set; }

        /// <summary>
        /// Damage.
        /// </summary>
        public short Attack3 { get; set; }

        /// <summary>
        /// Additional damage.
        /// </summary>
        public ushort AttackPlus3 { get; set; }

        /// <summary>
        /// Fraction of mob. AttackAttrib3
        /// </summary>
        public MobFraction Fraction { get; set; }

        /// <summary>
        /// Param, probably respawn time according to forum discussions.
        /// </summary>
        public MobRespawnTime AttackSpecial3 { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public byte AttackOk3 { get; set; }
        #endregion

    }
}
