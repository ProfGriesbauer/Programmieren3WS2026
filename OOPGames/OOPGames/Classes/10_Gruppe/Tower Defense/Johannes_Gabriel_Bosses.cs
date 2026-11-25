using System.Collections.Generic;

namespace OOPGames
{
    public class G10_MiniBoss : G10_Enemy
    {
        public G10_MiniBoss(
            string name,
            double moveSpeed,
            int health,
            int armor,
            int damageOnMainBuilding,
            IEnumerable<G10_DamageType> resistantDamageTypes,
            IEnumerable<G10_DamageType> weakAgainst,
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

    public class G10_Boss : G10_Enemy
    {
        public G10_Boss(
            string name,
            double moveSpeed,
            int health,
            int armor,
            int damageOnMainBuilding,
            IEnumerable<G10_DamageType> resistantDamageTypes,
            IEnumerable<G10_DamageType> weakAgainst,
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

    public class G10_SkeletonKing : G10_Boss
    {
        public G10_SkeletonKing(int wave = 1)
            : base("Skeleton King", 0.65, G10_FantasyEnemyScaling.Scale(900, wave), G10_FantasyEnemyScaling.Scale(45, wave), G10_FantasyEnemyScaling.Scale(100, wave), new[] { G10_DamageType.Physical, G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(160, wave))
        {
        }
    }

    public class G10_OrcLord : G10_Boss
    {
        public G10_OrcLord(int wave = 1)
            : base("Orc Lord", 0.55, G10_FantasyEnemyScaling.Scale(1100, wave), G10_FantasyEnemyScaling.Scale(55, wave), G10_FantasyEnemyScaling.Scale(120, wave), new[] { G10_DamageType.Physical }, new[] { G10_DamageType.Ice, G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(180, wave))
        {
        }
    }

    public class G10_ElementalGod : G10_Boss
    {
        public G10_ElementalGod(int wave = 1)
            : base("Elemental God", 0.7, G10_FantasyEnemyScaling.Scale(1400, wave), G10_FantasyEnemyScaling.Scale(65, wave), G10_FantasyEnemyScaling.Scale(150, wave), new[] { G10_DamageType.Fire, G10_DamageType.Ice, G10_DamageType.Earth, G10_DamageType.Storm }, new[] { G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(220, wave))
        {
        }
    }

    public class G10_VampireLord : G10_Boss
    {
        public G10_VampireLord(int wave = 1)
            : base("Vampire Lord", 0.9, G10_FantasyEnemyScaling.Scale(1250, wave), G10_FantasyEnemyScaling.Scale(50, wave), G10_FantasyEnemyScaling.Scale(140, wave), new[] { G10_DamageType.Physical, G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(200, wave))
        {
        }
    }

    public class G10_ElderDragon : G10_Boss
    {
        public G10_ElderDragon(int wave = 1)
            : base("Elder Dragon", 0.6, G10_FantasyEnemyScaling.Scale(1800, wave), G10_FantasyEnemyScaling.Scale(75, wave), G10_FantasyEnemyScaling.Scale(180, wave), new[] { G10_DamageType.Fire, G10_DamageType.Ice }, new[] { G10_DamageType.Storm }, G10_FantasyEnemyScaling.Scale(260, wave))
        {
        }
    }

    public class G10_FireDragon : G10_MiniBoss
    {
        public G10_FireDragon(int wave = 1)
            : base("Fire Dragon", 0.85, G10_FantasyEnemyScaling.Scale(650, wave), G10_FantasyEnemyScaling.Scale(30, wave), G10_FantasyEnemyScaling.Scale(75, wave), new[] { G10_DamageType.Fire }, new[] { G10_DamageType.Ice, G10_DamageType.Storm }, G10_FantasyEnemyScaling.Scale(100, wave))
        {
        }
    }

    public class G10_SkeletonMage : G10_MiniBoss
    {
        public G10_SkeletonMage(int wave = 1)
            : base("Skeleton Mage", 0.75, G10_FantasyEnemyScaling.Scale(500, wave), G10_FantasyEnemyScaling.Scale(20, wave), G10_FantasyEnemyScaling.Scale(65, wave), new[] { G10_DamageType.Ice, G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(85, wave))
        {
        }
    }

    public class G10_DarkElfKing : G10_MiniBoss
    {
        public G10_DarkElfKing(int wave = 1)
            : base("Dark Elf King", 1.0, G10_FantasyEnemyScaling.Scale(600, wave), G10_FantasyEnemyScaling.Scale(25, wave), G10_FantasyEnemyScaling.Scale(80, wave), new[] { G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(95, wave))
        {
        }
    }

    public class G10_AllElementsGolem : G10_MiniBoss
    {
        public G10_AllElementsGolem(int wave = 1)
            : base("All Elements Golem", 0.35, G10_FantasyEnemyScaling.Scale(1000, wave), G10_FantasyEnemyScaling.Scale(60, wave), G10_FantasyEnemyScaling.Scale(110, wave), new[] { G10_DamageType.Fire, G10_DamageType.Ice, G10_DamageType.Earth, G10_DamageType.Storm }, new[] { G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(140, wave))
        {
        }
    }

    public class G10_GoblinLord : G10_MiniBoss
    {
        public G10_GoblinLord(int wave = 1)
            : base("Goblin Lord", 1.1, G10_FantasyEnemyScaling.Scale(550, wave), G10_FantasyEnemyScaling.Scale(18, wave), G10_FantasyEnemyScaling.Scale(70, wave), new[] { G10_DamageType.Ice }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(90, wave))
        {
        }
    }
}
