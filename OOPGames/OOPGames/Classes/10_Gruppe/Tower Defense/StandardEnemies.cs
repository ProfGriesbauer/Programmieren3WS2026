using System;

namespace OOPGames
{
    public class Goblin : Enemy
    {
        public Goblin(int wave = 1)
            : base("Goblin", 1.4, FantasyEnemyScaling.Scale(45, wave), FantasyEnemyScaling.Scale(1, wave), FantasyEnemyScaling.Scale(8, wave), new[] { DamageType.Ice }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(5, wave))
        {
        }
    }

    public class GoblinArcher : Enemy
    {
        public GoblinArcher(int wave = 1)
            : base("Goblin Archer", 1.7, FantasyEnemyScaling.Scale(35, wave), FantasyEnemyScaling.Scale(0, wave), FantasyEnemyScaling.Scale(10, wave), new[] { DamageType.Physical }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(7, wave))
        {
        }
    }

    public class GoblinShaman : Enemy
    {
        public GoblinShaman(int wave = 1)
            : base("Goblin Shaman", 1.0, FantasyEnemyScaling.Scale(60, wave), FantasyEnemyScaling.Scale(2, wave), FantasyEnemyScaling.Scale(14, wave), new[] { DamageType.Poison }, new[] { DamageType.Physical }, FantasyEnemyScaling.Scale(10, wave))
        {
        }
    }

    public class Orc : Enemy
    {
        public Orc(int wave = 1)
            : base("Orc", 0.9, FantasyEnemyScaling.Scale(110, wave), FantasyEnemyScaling.Scale(8, wave), FantasyEnemyScaling.Scale(18, wave), new[] { DamageType.Physical }, new[] { DamageType.Poison }, FantasyEnemyScaling.Scale(15, wave))
        {
        }
    }

    public class OrcBerserker : Enemy
    {
        public OrcBerserker(int wave = 1)
            : base("Orc Berserker", 1.2, FantasyEnemyScaling.Scale(135, wave), FantasyEnemyScaling.Scale(5, wave), FantasyEnemyScaling.Scale(24, wave), new[] { DamageType.Physical }, new[] { DamageType.Ice }, FantasyEnemyScaling.Scale(20, wave))
        {
        }
    }

    public class OrcWarlord : Enemy
    {
        public OrcWarlord(int wave = 1)
            : base("Orc Warlord", 0.75, FantasyEnemyScaling.Scale(220, wave), FantasyEnemyScaling.Scale(14, wave), FantasyEnemyScaling.Scale(32, wave), new[] { DamageType.Physical, DamageType.Poison }, new[] { DamageType.Ice }, FantasyEnemyScaling.Scale(30, wave))
        {
        }
    }

    public class Skeleton : Enemy
    {
        public Skeleton(int wave = 1)
            : base("Skeleton", 1.1, FantasyEnemyScaling.Scale(70, wave), FantasyEnemyScaling.Scale(3, wave), FantasyEnemyScaling.Scale(12, wave), new[] { DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(9, wave))
        {
        }
    }

    public class SkeletonKnight : Enemy
    {
        public SkeletonKnight(int wave = 1)
            : base("Skeleton Knight", 0.8, FantasyEnemyScaling.Scale(180, wave), FantasyEnemyScaling.Scale(12, wave), FantasyEnemyScaling.Scale(25, wave), new[] { DamageType.Poison, DamageType.Physical }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(24, wave))
        {
        }
    }

    public class Troll : Enemy
    {
        public Troll(int wave = 1)
            : base("Troll", 0.65, FantasyEnemyScaling.Scale(300, wave), FantasyEnemyScaling.Scale(18, wave), FantasyEnemyScaling.Scale(38, wave), new[] { DamageType.Physical }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(40, wave))
        {
        }
    }

    public class StoneGolem : Enemy
    {
        public StoneGolem(int wave = 1)
            : base("Stone Golem", 0.4, FantasyEnemyScaling.Scale(500, wave), FantasyEnemyScaling.Scale(35, wave), FantasyEnemyScaling.Scale(55, wave), new[] { DamageType.Physical, DamageType.Ice }, new[] { DamageType.Poison }, FantasyEnemyScaling.Scale(60, wave))
        {
        }
    }

    public class FireElemental : Enemy
    {
        public FireElemental(int wave = 1)
            : base("Fire Elemental", 0.9, FantasyEnemyScaling.Scale(260, wave), FantasyEnemyScaling.Scale(10, wave), FantasyEnemyScaling.Scale(35, wave), new[] { DamageType.Fire }, new[] { DamageType.Ice }, FantasyEnemyScaling.Scale(45, wave))
        {
        }
    }

    public class IceElemental : Enemy
    {
        public IceElemental(int wave = 1)
            : base("Ice Elemental", 0.7, FantasyEnemyScaling.Scale(280, wave), FantasyEnemyScaling.Scale(12, wave), FantasyEnemyScaling.Scale(40, wave), new[] { DamageType.Ice }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(48, wave))
        {
        }
    }

    public class StormElemental : Enemy
    {
        public StormElemental(int wave = 1)
            : base("Storm Elemental", 1.0, FantasyEnemyScaling.Scale(320, wave), FantasyEnemyScaling.Scale(8, wave), FantasyEnemyScaling.Scale(45, wave), new[] { DamageType.Storm }, new[] { DamageType.Physical }, FantasyEnemyScaling.Scale(55, wave))
        {
        }
    }

    public class EarthElemental : Enemy
    {
        public EarthElemental(int wave = 1)
            : base("Earth Elemental", 0.5, FantasyEnemyScaling.Scale(450, wave), FantasyEnemyScaling.Scale(28, wave), FantasyEnemyScaling.Scale(50, wave), new[] { DamageType.Physical, DamageType.Storm }, new[] { DamageType.Poison }, FantasyEnemyScaling.Scale(65, wave))
        {
        }
    }

    public class DarkElf : Enemy
    {
        public DarkElf(int wave = 1)
            : base("Dark Elf", 1.3, FantasyEnemyScaling.Scale(240, wave), FantasyEnemyScaling.Scale(9, wave), FantasyEnemyScaling.Scale(42, wave), new[] { DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(50, wave))
        {
        }
    }

    public class Vampire : Enemy
    {
        public Vampire(int wave = 1)
            : base("Vampire", 1.1, FantasyEnemyScaling.Scale(380, wave), FantasyEnemyScaling.Scale(15, wave), FantasyEnemyScaling.Scale(60, wave), new[] { DamageType.Physical, DamageType.Poison }, new[] { DamageType.Fire }, FantasyEnemyScaling.Scale(75, wave))
        {
        }
    }

    public class Wyvern : Enemy
    {
        public Wyvern(int wave = 1)
            : base("Wyvern", 1.5, FantasyEnemyScaling.Scale(420, wave), FantasyEnemyScaling.Scale(17, wave), FantasyEnemyScaling.Scale(65, wave), new[] { DamageType.Ice }, new[] { DamageType.Storm }, FantasyEnemyScaling.Scale(85, wave))
        {
        }
    }

    public class FireGolem : Enemy
    {
        public FireGolem(int wave = 1)
            : base("Fire Golem", 0.35, FantasyEnemyScaling.Scale(700, wave), FantasyEnemyScaling.Scale(45, wave), FantasyEnemyScaling.Scale(80, wave), new[] { DamageType.Fire, DamageType.Physical }, new[] { DamageType.Ice }, FantasyEnemyScaling.Scale(100, wave))
        {
        }
    }

    public class Dragon : Enemy
    {
        public Dragon(int wave = 1)
            : base("Dragon", 0.8, FantasyEnemyScaling.Scale(1200, wave), FantasyEnemyScaling.Scale(60, wave), FantasyEnemyScaling.Scale(130, wave), new[] { DamageType.Fire, DamageType.Ice }, new[] { DamageType.Storm }, FantasyEnemyScaling.Scale(180, wave))
        {
        }
    }

    public class ArcaneTitan : Enemy
    {
        public ArcaneTitan(int wave = 1)
            : base("Arcane Titan", 0.25, FantasyEnemyScaling.Scale(2000, wave), FantasyEnemyScaling.Scale(90, wave), FantasyEnemyScaling.Scale(200, wave), new[] { DamageType.Physical, DamageType.Fire, DamageType.Ice }, new[] { DamageType.Poison }, FantasyEnemyScaling.Scale(300, wave))
        {
        }
    }

    internal static class FantasyEnemyScaling
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
