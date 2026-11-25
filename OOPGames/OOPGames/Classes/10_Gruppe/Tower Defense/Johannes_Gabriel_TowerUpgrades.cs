using System;

namespace OOPGames
{
    public enum G10_TowerUpgradePath
    {
        Damage,
        Range,
        Speed,
        Tier,
        // Endless levels, only available once all other paths are maxed.
        Mastery
    }

    // Upgrade state of one built tower. Damage, range and speed upgrades add a percentage of the
    // tower's base value per level; the tier (I, II, III) raises all base values at once. When everything
    // is bought, the mastery levels (no limit, rising price) keep raising damage and attack speed.
    public sealed class G10_TowerUpgrades
    {
        public const int MaxTier = 3;
        public const int MaxDamage = 8;
        public const int MaxRange = 5;
        public const int MaxSpeed = 6;

        public const double DamagePerLevel = 0.30;
        public const double RangePerLevel = 0.10;
        public const double SpeedPerLevel = 0.15;

        // Per mastery level. Range stays the same so towers never cover the whole map.
        public const double MasteryDamagePerLevel = 0.10;
        public const double MasterySpeedPerLevel = 0.05;
        // Mastery price: build cost x 3 for the first level, x 1.6 for every further level.
        const double MasteryBaseCost = 3.0;
        const double MasteryCostGrowth = 1.6;

        static readonly double[] TierDamage = { 1.0, 1.5, 2.25 };
        static readonly double[] TierSpeed = { 1.0, 1.15, 1.3 };
        static readonly double[] TierRange = { 1.0, 1.1, 1.2 };
        // Tier upgrade price as a multiple of the tower's build cost (to tier II, to tier III).
        static readonly double[] TierCostFactor = { 2, 4 };

        public int Tier { get; set; } = 1;
        public int Damage { get; set; }
        public int Range { get; set; }
        public int Speed { get; set; }
        public int Mastery { get; set; }

        public int TotalLevels { get { return Damage + Range + Speed; } }

        // True when damage, range, speed and tier are all at their maximum.
        public bool IsComplete
        {
            get { return Damage >= MaxDamage && Range >= MaxRange && Speed >= MaxSpeed && Tier >= MaxTier; }
        }

        public int Level(G10_TowerUpgradePath path)
        {
            switch (path)
            {
                case G10_TowerUpgradePath.Damage: return Damage;
                case G10_TowerUpgradePath.Range: return Range;
                case G10_TowerUpgradePath.Speed: return Speed;
                case G10_TowerUpgradePath.Mastery: return Mastery;
                default: return Tier;
            }
        }

        public static int MaxLevel(G10_TowerUpgradePath path)
        {
            switch (path)
            {
                case G10_TowerUpgradePath.Damage: return MaxDamage;
                case G10_TowerUpgradePath.Range: return MaxRange;
                case G10_TowerUpgradePath.Speed: return MaxSpeed;
                case G10_TowerUpgradePath.Mastery: return int.MaxValue;
                default: return MaxTier;
            }
        }

        public bool IsMaxed(G10_TowerUpgradePath path)
        {
            return Level(path) >= MaxLevel(path);
        }

        // Whether the next level of this path can be bought at all (gold aside).
        public bool CanBuy(G10_TowerUpgradePath path)
        {
            return path == G10_TowerUpgradePath.Mastery ? IsComplete : !IsMaxed(path);
        }

        public int Cost(G10_Tower tower, G10_TowerUpgradePath path)
        {
            return CostAt(tower, path, Level(path));
        }

        // Price to go from `level` to `level + 1` on the given path.
        static int CostAt(G10_Tower tower, G10_TowerUpgradePath path, int level)
        {
            switch (path)
            {
                case G10_TowerUpgradePath.Damage: return (int)Math.Round(tower.Cost * 0.40 * (level + 1));
                case G10_TowerUpgradePath.Range: return (int)Math.Round(tower.Cost * 0.35 * (level + 1));
                case G10_TowerUpgradePath.Speed: return (int)Math.Round(tower.Cost * 0.40 * (level + 1));
                case G10_TowerUpgradePath.Mastery: return (int)Math.Round(tower.Cost * MasteryBaseCost * Math.Pow(MasteryCostGrowth, level));
                default: return (int)Math.Round(tower.Cost * TierCostFactor[level - 1]);
            }
        }

        public void Apply(G10_TowerUpgradePath path)
        {
            if (!CanBuy(path)) throw new InvalidOperationException("This upgrade cannot be bought.");
            switch (path)
            {
                case G10_TowerUpgradePath.Damage: Damage++; break;
                case G10_TowerUpgradePath.Range: Range++; break;
                case G10_TowerUpgradePath.Speed: Speed++; break;
                case G10_TowerUpgradePath.Mastery: Mastery++; break;
                default: Tier++; break;
            }
        }

        // Everything spent on this tower including the build cost.
        public int Invested(G10_Tower tower)
        {
            int total = tower.Cost;
            for (int l = 0; l < Damage; l++) total += CostAt(tower, G10_TowerUpgradePath.Damage, l);
            for (int l = 0; l < Range; l++) total += CostAt(tower, G10_TowerUpgradePath.Range, l);
            for (int l = 0; l < Speed; l++) total += CostAt(tower, G10_TowerUpgradePath.Speed, l);
            for (int t = 1; t < Tier; t++) total += CostAt(tower, G10_TowerUpgradePath.Tier, t);
            for (int m = 0; m < Mastery; m++) total += CostAt(tower, G10_TowerUpgradePath.Mastery, m);
            return total;
        }

        public double DamageMultiplier
        {
            get { return TierDamage[Tier - 1] * (1 + DamagePerLevel * Damage) * (1 + MasteryDamagePerLevel * Mastery); }
        }

        public double SpeedMultiplier
        {
            get { return TierSpeed[Tier - 1] * (1 + SpeedPerLevel * Speed) * (1 + MasterySpeedPerLevel * Mastery); }
        }

        public double RangeMultiplier { get { return TierRange[Tier - 1] * (1 + RangePerLevel * Range); } }

        // Values after one more level on the given path (for the upgrade preview).
        public G10_TowerUpgrades Preview(G10_TowerUpgradePath path)
        {
            G10_TowerUpgrades next = Clone();
            if (next.CanBuy(path)) next.Apply(path);
            return next;
        }

        public G10_TowerUpgrades Clone()
        {
            return new G10_TowerUpgrades { Tier = Tier, Damage = Damage, Range = Range, Speed = Speed, Mastery = Mastery };
        }
    }
}
