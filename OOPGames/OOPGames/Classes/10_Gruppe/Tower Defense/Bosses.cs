using System.Collections.Generic;

namespace OOPGames
{
    public class MiniBoss : Enemy
    {
        public MiniBoss(
            string name,
            double moveSpeed,
            int health,
            int armor,
            int damageOnMainBuilding,
            IEnumerable<DamageType> resistantDamageTypes,
            IEnumerable<DamageType> weakAgainst,
            int reward)
            : base(
                name,
                moveSpeed,
                health,
                armor,
                damageOnMainBuilding,
                resistantDamageTypes,
                weakAgainst,
                reward)
        {
        }
    }

    public class Boss : Enemy
    {
        public Boss(
            string name,
            double moveSpeed,
            int health,
            int armor,
            int damageOnMainBuilding,
            IEnumerable<DamageType> resistantDamageTypes,
            IEnumerable<DamageType> weakAgainst,
            int reward)
            : base(
                name,
                moveSpeed,
                health,
                armor,
                damageOnMainBuilding,
                resistantDamageTypes,
                weakAgainst,
                reward)
        {
        }
    }

    public class SkeletonKing : Boss
    {
        public SkeletonKing(int wave = 1)
            : base("Skeleton King", 0.65, FantasyEnemyScaling.Scale(900, wave), FantasyEnemyScaling.Scale(45, wave), FantasyEnemyScaling.Scale(100, wave), new[] { DamageType.Physical, DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(160, wave))
        {
        }
    }

    public class OrcLord : Boss
    {
        public OrcLord(int wave = 1)
            : base("Orc Lord", 0.55, FantasyEnemyScaling.Scale(1100, wave), FantasyEnemyScaling.Scale(55, wave), FantasyEnemyScaling.Scale(120, wave), new[] { DamageType.Physical }, new[] { DamageType.Ice, DamageType.Poison }, FantasyEnemyScaling.Scale(180, wave))
        {
        }
    }

    public class ElementalGod : Boss
    {
        public ElementalGod(int wave = 1)
            : base("Elemental God", 0.7, FantasyEnemyScaling.Scale(1400, wave), FantasyEnemyScaling.Scale(65, wave), FantasyEnemyScaling.Scale(150, wave), new[] { DamageType.Fire, DamageType.Ice, DamageType.Earth, DamageType.Storm }, new[] { DamageType.Poison }, FantasyEnemyScaling.Scale(220, wave))
        {
        }
    }

    public class VampireLord : Boss
    {
        public VampireLord(int wave = 1)
            : base("Vampire Lord", 0.9, FantasyEnemyScaling.Scale(1250, wave), FantasyEnemyScaling.Scale(50, wave), FantasyEnemyScaling.Scale(140, wave), new[] { DamageType.Physical, DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(200, wave))
        {
        }
    }

    public class ElderDragon : Boss
    {
        public ElderDragon(int wave = 1)
            : base("Elder Dragon", 0.6, FantasyEnemyScaling.Scale(1800, wave), FantasyEnemyScaling.Scale(75, wave), FantasyEnemyScaling.Scale(180, wave), new[] { DamageType.Fire, DamageType.Ice }, new[] { DamageType.Storm }, FantasyEnemyScaling.Scale(260, wave))
        {
        }
    }

    public class FireDragon : MiniBoss
    {
        public FireDragon(int wave = 1)
            : base("Fire Dragon", 0.85, FantasyEnemyScaling.Scale(650, wave), FantasyEnemyScaling.Scale(30, wave), FantasyEnemyScaling.Scale(75, wave), new[] { DamageType.Fire }, new[] { DamageType.Ice, DamageType.Storm }, FantasyEnemyScaling.Scale(100, wave))
        {
        }
    }

    public class SkeletonMage : MiniBoss
    {
        public SkeletonMage(int wave = 1)
            : base("Skeleton Mage", 0.75, FantasyEnemyScaling.Scale(500, wave), FantasyEnemyScaling.Scale(20, wave), FantasyEnemyScaling.Scale(65, wave), new[] { DamageType.Ice, DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(85, wave))
        {
        }
    }

    public class DarkElfKing : MiniBoss
    {
        public DarkElfKing(int wave = 1)
            : base("Dark Elf King", 1.0, FantasyEnemyScaling.Scale(600, wave), FantasyEnemyScaling.Scale(25, wave), FantasyEnemyScaling.Scale(80, wave), new[] { DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(95, wave))
        {
        }
    }

    public class AllElementsGolem : MiniBoss
    {
        public AllElementsGolem(int wave = 1)
            : base("All Elements Golem", 0.35, FantasyEnemyScaling.Scale(1000, wave), FantasyEnemyScaling.Scale(60, wave), FantasyEnemyScaling.Scale(110, wave), new[] { DamageType.Fire, DamageType.Ice, DamageType.Earth, DamageType.Storm }, new[] { DamageType.Poison }, FantasyEnemyScaling.Scale(140, wave))
        {
        }
    }

    public class GoblinLord : MiniBoss
    {
        public GoblinLord(int wave = 1)
            : base("Goblin Lord", 1.1, FantasyEnemyScaling.Scale(550, wave), FantasyEnemyScaling.Scale(18, wave), FantasyEnemyScaling.Scale(70, wave), new[] { DamageType.Ice }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(90, wave))
        {
        }
    }
}
