using Imgeneus.Database.Entities;
using Imgeneus.Game.Skills;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Tests;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

namespace Imgeneus.Game.Tests.CharacterTests
{
    public class SkillCastingTest : BaseTest
    {
        [Fact]
        [Description("Move should stop casting skill.")]
        public void MoveStopsCastTest()
        {
            var character1 = CreateCharacter();
            var character2 = CreateCharacter(country: Fraction.Dark);
            var skillWasCasted = false;

            character1.SkillsManager.OnUsedSkill += (uint senderId, IKillable target, Skill skill, AttackResult res) => skillWasCasted = true;

            character1.SkillCastingManager.StartCasting(new Skill(MagicRoots_Lvl1, 0, 0), character2);
            Assert.NotNull(character1.SkillCastingManager.SkillInCast);

            character1.MovementManager.PosX += 1;
            character1.MovementManager.RaisePositionChanged();

            Assert.Null(character1.SkillCastingManager.SkillInCast);
            Assert.False(skillWasCasted);
        }

        [Fact]
        [Description("Damage should stop casting skill.")]
        public void DamageStopsCastTest()
        {
            var character1 = CreateCharacter();
            character1.HealthManager.FullRecover();

            var character2 = CreateCharacter(country: Fraction.Dark);
            character2.HealthManager.FullRecover();
            character2.StatsManager.WeaponMinAttack = 1;
            character2.StatsManager.WeaponMaxAttack = 1;

            var skillWasCasted = false;

            character1.SkillsManager.OnUsedSkill += (uint senderId, IKillable target, Skill skill, AttackResult res) => skillWasCasted = true;

            character1.SkillCastingManager.StartCasting(new Skill(MagicRoots_Lvl1, 0, 0), character2);
            Assert.NotNull(character1.SkillCastingManager.SkillInCast);

            character2.AttackManager.Target = character1;
            character2.AttackManager.AutoAttack(character2);

            Assert.Null(character1.SkillCastingManager.SkillInCast);
            Assert.False(skillWasCasted);
        }

        [Fact]
        [Description("Cast skill should be possible.")]
        public async Task CastSuccessTest()
        {
            var character1 = CreateCharacter();
            character1.HealthManager.FullRecover();
            character1.StatsManager.WeaponMinAttack = 1;
            character1.StatsManager.WeaponMaxAttack = 1;

            var character2 = CreateCharacter(country: Fraction.Dark);
            character2.HealthManager.FullRecover();

            var skillWasCasted = false;
            character1.SkillsManager.OnUsedSkill += (uint senderId, IKillable target, Skill skill, AttackResult res) => skillWasCasted = true;

            character1.SkillCastingManager.StartCasting(new Skill(MagicRoots_Lvl1, 0, 0), character2);

            await Task.Delay(1500);

            Assert.True(skillWasCasted);
            Assert.True(character2.HealthManager.CurrentHP < character2.HealthManager.MaxHP);
        }

        [Fact]
        [Description("Persist Barrier should protect casting.")]
        public async void PersistBarrierProtectsCastTest()
        {
            var map = testMap;

            var character1 = CreateCharacter(map);
            character1.HealthManager.FullRecover();

            var character2 = CreateCharacter(map, country: Fraction.Dark);
            character2.HealthManager.FullRecover();
            character2.StatsManager.WeaponMinAttack = 1;
            character2.StatsManager.WeaponMaxAttack = 1;

            var priest = CreateCharacter(map);
            priest.HealthManager.FullRecover();

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            priest.PartyManager.Party = party;

            Assert.False(character1.CastProtectionManager.IsCastProtected);
            priest.SkillsManager.UseSkill(new Skill(PersistBarrier, 0, 0), priest);
            Assert.True(character1.CastProtectionManager.IsCastProtected);

            var skillWasCasted = false;
            character1.SkillsManager.OnUsedSkill += (uint senderId, IKillable target, Skill skill, AttackResult res) => skillWasCasted = true;
            character1.SkillCastingManager.StartCasting(new Skill(MagicRoots_Lvl1, 0, 0), character2);

            character2.AttackManager.Target = character1;
            character2.AttackManager.AutoAttack(character2);

            await Task.Delay(1500);
            Assert.True(skillWasCasted);
        }
    }
}
