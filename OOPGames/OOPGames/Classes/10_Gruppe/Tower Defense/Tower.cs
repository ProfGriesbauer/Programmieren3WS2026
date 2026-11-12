using System;

namespace OOPGames
{
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Earth,
        Storm,
        Poison,
    }

    public enum TowerEffect
    {
        None,
        Burn,
        PoisonDamageOverTime,
        Slow,
        ArmorBreak,
        ChainLightning
    }

    public enum AttackMode
    {
        SingleTarget,
        AreaDamage
    }

    public class Tower
    {
        public Tower(
            string name,
            int damage,
            double attackSpeed,
            DamageType damageType,
            double attackRange,
            AttackMode attackMode,
            int cost,
            TowerEffect effect = TowerEffect.None,
            int effectDamagePerSecond = 0,
            double effectDuration = 0,
            double slowFactor = 1,
            int maxChainTargets = 1,
            int effectStrength = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException("A tower needs a name.", nameof(name));
            }

            if (damage < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
            }

            if (attackSpeed <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(attackSpeed), "Attack speed must be greater than zero.");
            }

            if (attackRange <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(attackRange), "Attack range must be greater than zero.");
            }

            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), "Cost cannot be negative.");
            }

            if (effectDamagePerSecond < 0 || effectDuration < 0 || effectStrength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(effectDamagePerSecond), "Effect values cannot be negative.");
            }

            if (slowFactor <= 0 || slowFactor > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(slowFactor), "Slow factor must be greater than zero and at most one.");
            }

            if (maxChainTargets < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxChainTargets), "A tower must have at least one target.");
            }

            Name = name;
            Damage = damage;
            AttackSpeed = attackSpeed;
            DamageType = damageType;
            AttackRange = attackRange;
            AttackMode = attackMode;
            Cost = cost;
            UpgradeLevel = 1;
            MaxTargets = attackMode == AttackMode.AreaDamage ? 3 : 1;
            SplashRadius = attackMode == AttackMode.AreaDamage ? 1 : 0;
            Effect = effect;
            EffectDamagePerSecond = effectDamagePerSecond;
            EffectDuration = effectDuration;
            SlowFactor = slowFactor;
            MaxChainTargets = maxChainTargets;
            EffectStrength = effectStrength;
        }

        public string Name { get; private set; }
        public int Damage { get; private set; }

        // Number of attacks per second.
        public double AttackSpeed { get; private set; }

        public DamageType DamageType { get; private set; }
        public double AttackRange { get; private set; }
        public AttackMode AttackMode { get; private set; }
        public int Cost { get; private set; }
        public int UpgradeLevel { get; private set; }
        public int MaxTargets { get; private set; }
        public double SplashRadius { get; private set; }
        public TowerEffect Effect { get; private set; }
        public int EffectDamagePerSecond { get; private set; }
        public double EffectDuration { get; private set; }
        public double SlowFactor { get; private set; }
        public int MaxChainTargets { get; private set; }
        public int EffectStrength { get; private set; }
    }
}
