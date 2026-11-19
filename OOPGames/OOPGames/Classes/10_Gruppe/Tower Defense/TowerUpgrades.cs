using System;

namespace OOPGames
{
    public enum TowerUpgradePath
    {
        Damage,
        Range,
        Speed,
        Tier
    }

    // Upgrade state of one built tower. Damage, range and speed upgrades add a percentage of the
    // tower's base value per level; the tier (I, II, III) raises all base values at once.
    public sealed class TowerUpgrades
    {
        public const int MaxTier = 3;
        public const int MaxDamage = 8;
        public const int MaxRange = 5;
        public const int MaxSpeed = 6;

        public const double DamagePerLevel = 0.25;
        public const double RangePerLevel = 0.08;
        public const double SpeedPerLevel = 0.12;

        static readonly double[] TierDamage = { 1.0, 1.5, 2.25 };
        static readonly double[] TierSpeed = { 1.0, 1.15, 1.3 };
        static readonly double[] TierRange = { 1.0, 1.1, 1.2 };
        // Tier upgrade price as a multiple of the tower's build cost (to tier II, to tier III).
        static readonly double[] TierCostFactor = { 1.5, 3 };

        public int Tier { get; set; } = 1;
        public int Damage { get; set; }
        public int Range { get; set; }
        public int Speed { get; set; }

        public int TotalLevels { get { return Damage + Range + Speed; } }

        public int Level(TowerUpgradePath path)
        {
            switch (path)
            {
                case TowerUpgradePath.Damage: return Damage;
                case TowerUpgradePath.Range: return Range;
                case TowerUpgradePath.Speed: return Speed;
                default: return Tier;
            }
        }

        public static int MaxLevel(TowerUpgradePath path)
        {
            switch (path)
            {
                case TowerUpgradePath.Damage: return MaxDamage;
                case TowerUpgradePath.Range: return MaxRange;
                case TowerUpgradePath.Speed: return MaxSpeed;
                default: return MaxTier;
            }
        }

        public bool IsMaxed(TowerUpgradePath path)
        {
            return Level(path) >= MaxLevel(path);
        }

        public int Cost(Tower tower, TowerUpgradePath path)
        {
            return CostAt(tower, path, Level(path));
        }

        // Price to go from `level` to `level + 1` on the given path.
        static int CostAt(Tower tower, TowerUpgradePath path, int level)
        {
            switch (path)
            {
                case TowerUpgradePath.Damage: return (int)Math.Round(tower.Cost * 0.30 * (level + 1));
                case TowerUpgradePath.Range: return (int)Math.Round(tower.Cost * 0.25 * (level + 1));
                case TowerUpgradePath.Speed: return (int)Math.Round(tower.Cost * 0.30 * (level + 1));
                default: return (int)Math.Round(tower.Cost * TierCostFactor[level - 1]);
            }
        }

        public void Apply(TowerUpgradePath path)
        {
            if (IsMaxed(path)) throw new InvalidOperationException("This upgrade is already at its maximum.");
            switch (path)
            {
                case TowerUpgradePath.Damage: Damage++; break;
                case TowerUpgradePath.Range: Range++; break;
                case TowerUpgradePath.Speed: Speed++; break;
                default: Tier++; break;
            }
        }

        // Everything spent on this tower including the build cost.
        public int Invested(Tower tower)
        {
            int total = tower.Cost;
            for (int l = 0; l < Damage; l++) total += CostAt(tower, TowerUpgradePath.Damage, l);
            for (int l = 0; l < Range; l++) total += CostAt(tower, TowerUpgradePath.Range, l);
            for (int l = 0; l < Speed; l++) total += CostAt(tower, TowerUpgradePath.Speed, l);
            for (int t = 1; t < Tier; t++) total += CostAt(tower, TowerUpgradePath.Tier, t);
            return total;
        }

        public double DamageMultiplier { get { return TierDamage[Tier - 1] * (1 + DamagePerLevel * Damage); } }
        public double SpeedMultiplier { get { return TierSpeed[Tier - 1] * (1 + SpeedPerLevel * Speed); } }
        public double RangeMultiplier { get { return TierRange[Tier - 1] * (1 + RangePerLevel * Range); } }

        // Values after one more level on the given path (for the upgrade preview).
        public TowerUpgrades Preview(TowerUpgradePath path)
        {
            TowerUpgrades next = Clone();
            if (!next.IsMaxed(path)) next.Apply(path);
            return next;
        }

        public TowerUpgrades Clone()
        {
            return new TowerUpgrades { Tier = Tier, Damage = Damage, Range = Range, Speed = Speed };
        }
    }
}
