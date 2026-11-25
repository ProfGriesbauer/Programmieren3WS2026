using System;

namespace OOPGames
{
    public class G10_Goblin : G10_Enemy
    {
        public G10_Goblin(int wave = 1)
            : base("Goblin", 1.4, G10_FantasyEnemyScaling.Scale(45, wave), G10_FantasyEnemyScaling.Scale(1, wave), G10_FantasyEnemyScaling.Scale(8, wave), new[] { G10_DamageType.Ice }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(5, wave))
        {
        }
    }

    public class G10_GoblinArcher : G10_Enemy
    {
        public G10_GoblinArcher(int wave = 1)
            : base("Goblin Archer", 1.7, G10_FantasyEnemyScaling.Scale(35, wave), G10_FantasyEnemyScaling.Scale(0, wave), G10_FantasyEnemyScaling.Scale(10, wave), new[] { G10_DamageType.Physical }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(7, wave))
        {
        }
    }

    public class G10_GoblinShaman : G10_Enemy
    {
        public G10_GoblinShaman(int wave = 1)
            : base("Goblin Shaman", 1.0, G10_FantasyEnemyScaling.Scale(60, wave), G10_FantasyEnemyScaling.Scale(2, wave), G10_FantasyEnemyScaling.Scale(14, wave), new[] { G10_DamageType.Poison }, new[] { G10_DamageType.Physical }, G10_FantasyEnemyScaling.Scale(10, wave))
        {
        }
    }

    public class G10_Orc : G10_Enemy
    {
        public G10_Orc(int wave = 1)
            : base("Orc", 0.9, G10_FantasyEnemyScaling.Scale(110, wave), G10_FantasyEnemyScaling.Scale(8, wave), G10_FantasyEnemyScaling.Scale(18, wave), new[] { G10_DamageType.Physical }, new[] { G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(15, wave))
        {
        }
    }

    public class G10_OrcBerserker : G10_Enemy
    {
        public G10_OrcBerserker(int wave = 1)
            : base("Orc Berserker", 1.2, G10_FantasyEnemyScaling.Scale(135, wave), G10_FantasyEnemyScaling.Scale(5, wave), G10_FantasyEnemyScaling.Scale(24, wave), new[] { G10_DamageType.Physical }, new[] { G10_DamageType.Ice }, G10_FantasyEnemyScaling.Scale(20, wave))
        {
        }
    }

    public class G10_OrcWarlord : G10_Enemy
    {
        public G10_OrcWarlord(int wave = 1)
            : base("Orc Warlord", 0.75, G10_FantasyEnemyScaling.Scale(220, wave), G10_FantasyEnemyScaling.Scale(14, wave), G10_FantasyEnemyScaling.Scale(32, wave), new[] { G10_DamageType.Physical, G10_DamageType.Poison }, new[] { G10_DamageType.Ice }, G10_FantasyEnemyScaling.Scale(30, wave))
        {
        }
    }

    public class G10_Skeleton : G10_Enemy
    {
        public G10_Skeleton(int wave = 1)
            : base("Skeleton", 1.1, G10_FantasyEnemyScaling.Scale(70, wave), G10_FantasyEnemyScaling.Scale(3, wave), G10_FantasyEnemyScaling.Scale(12, wave), new[] { G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(9, wave))
        {
        }
    }

    public class G10_SkeletonKnight : G10_Enemy
    {
        public G10_SkeletonKnight(int wave = 1)
            : base("Skeleton Knight", 0.8, G10_FantasyEnemyScaling.Scale(180, wave), G10_FantasyEnemyScaling.Scale(12, wave), G10_FantasyEnemyScaling.Scale(25, wave), new[] { G10_DamageType.Poison, G10_DamageType.Physical }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(24, wave))
        {
        }
    }

    public class G10_Troll : G10_Enemy
    {
        public G10_Troll(int wave = 1)
            : base("Troll", 0.65, G10_FantasyEnemyScaling.Scale(300, wave), G10_FantasyEnemyScaling.Scale(18, wave), G10_FantasyEnemyScaling.Scale(38, wave), new[] { G10_DamageType.Physical }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(40, wave))
        {
        }
    }

    public class G10_StoneGolem : G10_Enemy
    {
        public G10_StoneGolem(int wave = 1)
            : base("Stone Golem", 0.4, G10_FantasyEnemyScaling.Scale(500, wave), G10_FantasyEnemyScaling.Scale(35, wave), G10_FantasyEnemyScaling.Scale(55, wave), new[] { G10_DamageType.Physical, G10_DamageType.Ice }, new[] { G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(60, wave))
        {
        }
    }

    public class G10_FireElemental : G10_Enemy
    {
        public G10_FireElemental(int wave = 1)
            : base("Fire Elemental", 0.9, G10_FantasyEnemyScaling.Scale(260, wave), G10_FantasyEnemyScaling.Scale(10, wave), G10_FantasyEnemyScaling.Scale(35, wave), new[] { G10_DamageType.Fire }, new[] { G10_DamageType.Ice }, G10_FantasyEnemyScaling.Scale(45, wave))
        {
        }
    }

    public class G10_IceElemental : G10_Enemy
    {
        public G10_IceElemental(int wave = 1)
            : base("Ice Elemental", 0.7, G10_FantasyEnemyScaling.Scale(280, wave), G10_FantasyEnemyScaling.Scale(12, wave), G10_FantasyEnemyScaling.Scale(40, wave), new[] { G10_DamageType.Ice }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(48, wave))
        {
        }
    }

    public class G10_StormElemental : G10_Enemy
    {
        public G10_StormElemental(int wave = 1)
            : base("Storm Elemental", 1.0, G10_FantasyEnemyScaling.Scale(320, wave), G10_FantasyEnemyScaling.Scale(8, wave), G10_FantasyEnemyScaling.Scale(45, wave), new[] { G10_DamageType.Storm }, new[] { G10_DamageType.Physical }, G10_FantasyEnemyScaling.Scale(55, wave))
        {
        }
    }

    public class G10_EarthElemental : G10_Enemy
    {
        public G10_EarthElemental(int wave = 1)
            : base("Earth Elemental", 0.5, G10_FantasyEnemyScaling.Scale(450, wave), G10_FantasyEnemyScaling.Scale(28, wave), G10_FantasyEnemyScaling.Scale(50, wave), new[] { G10_DamageType.Physical, G10_DamageType.Storm }, new[] { G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(65, wave))
        {
        }
    }

    public class G10_DarkElf : G10_Enemy
    {
        public G10_DarkElf(int wave = 1)
            : base("Dark Elf", 1.3, G10_FantasyEnemyScaling.Scale(240, wave), G10_FantasyEnemyScaling.Scale(9, wave), G10_FantasyEnemyScaling.Scale(42, wave), new[] { G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(50, wave))
        {
        }
    }

    public class G10_Vampire : G10_Enemy
    {
        public G10_Vampire(int wave = 1)
            : base("Vampire", 1.1, G10_FantasyEnemyScaling.Scale(380, wave), G10_FantasyEnemyScaling.Scale(15, wave), G10_FantasyEnemyScaling.Scale(60, wave), new[] { G10_DamageType.Physical, G10_DamageType.Poison }, new[] { G10_DamageType.Fire }, G10_FantasyEnemyScaling.Scale(75, wave))
        {
        }
    }

    public class G10_Wyvern : G10_Enemy
    {
        public G10_Wyvern(int wave = 1)
            : base("Wyvern", 1.5, G10_FantasyEnemyScaling.Scale(420, wave), G10_FantasyEnemyScaling.Scale(17, wave), G10_FantasyEnemyScaling.Scale(65, wave), new[] { G10_DamageType.Ice }, new[] { G10_DamageType.Storm }, G10_FantasyEnemyScaling.Scale(85, wave))
        {
        }
    }

    public class G10_FireGolem : G10_Enemy
    {
        public G10_FireGolem(int wave = 1)
            : base("Fire Golem", 0.35, G10_FantasyEnemyScaling.Scale(700, wave), G10_FantasyEnemyScaling.Scale(45, wave), G10_FantasyEnemyScaling.Scale(80, wave), new[] { G10_DamageType.Fire, G10_DamageType.Physical }, new[] { G10_DamageType.Ice }, G10_FantasyEnemyScaling.Scale(100, wave))
        {
        }
    }

    public class G10_Dragon : G10_Enemy
    {
        public G10_Dragon(int wave = 1)
            : base("Dragon", 0.8, G10_FantasyEnemyScaling.Scale(1200, wave), G10_FantasyEnemyScaling.Scale(60, wave), G10_FantasyEnemyScaling.Scale(130, wave), new[] { G10_DamageType.Fire, G10_DamageType.Ice }, new[] { G10_DamageType.Storm }, G10_FantasyEnemyScaling.Scale(180, wave))
        {
        }
    }

    public class G10_ArcaneTitan : G10_Enemy
    {
        public G10_ArcaneTitan(int wave = 1)
            : base("Arcane Titan", 0.25, G10_FantasyEnemyScaling.Scale(2000, wave), G10_FantasyEnemyScaling.Scale(75, wave), G10_FantasyEnemyScaling.Scale(200, wave), new[] { G10_DamageType.Physical, G10_DamageType.Fire, G10_DamageType.Ice }, new[] { G10_DamageType.Poison }, G10_FantasyEnemyScaling.Scale(300, wave))
        {
        }
    }

    internal static class G10_FantasyEnemyScaling
    {
        public static int Scale(int value, int wave)
        {
            if (wave < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(wave), "Wave must be at least one.");
            }

            return value + (wave - 1) * Math.Max(1, value / 5);
        }
    }
}
