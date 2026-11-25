using System;

namespace OOPGames
{
    public enum WeaponTargetingMode
    {
        Opponents,
        Allies,
        Self,
        // Every entity but the attacker, of any team (e.g. a frog eating everything).
        Everyone
    }

    public sealed class Weapon
    {
        public Weapon(
            string id, string name, string description, IAttackPattern attackPattern,
            ICombatEffect effect, double cooldownSeconds,
            WeaponTargetingMode targetingMode, string animationPlaceholder,
            int maximumHitsPerTarget = 1, double knockbackDistance = 8,
            WeaponAppearance appearance = null, double equipReadySeconds = 0,
            ICombatEffect trailEffect = null, string trailVisual = null,
            ChainLightning chainLightning = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A weapon id is required.", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A weapon name is required.", nameof(name));
            }
            if (attackPattern == null)
            {
                throw new ArgumentNullException(nameof(attackPattern));
            }
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }
            if (cooldownSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            }
            if (maximumHitsPerTarget <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumHitsPerTarget));
            }
            if (knockbackDistance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(knockbackDistance));
            }
            if (equipReadySeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(equipReadySeconds));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            AttackPattern = attackPattern;
            Effect = effect;
            CooldownSeconds = cooldownSeconds;
            TargetingMode = targetingMode;
            AnimationPlaceholder = animationPlaceholder ?? string.Empty;
            MaximumHitsPerTarget = maximumHitsPerTarget;
            KnockbackDistance = knockbackDistance;
            Appearance = appearance;
            EquipReadySeconds = equipReadySeconds;
            TrailEffect = trailEffect;
            TrailVisual = trailVisual;
            ChainLightning = chainLightning;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public IAttackPattern AttackPattern { get; }
        public ICombatEffect Effect { get; }
        public double CooldownSeconds { get; }
        public WeaponTargetingMode TargetingMode { get; }
        public string AnimationPlaceholder { get; }
        public int MaximumHitsPerTarget { get; }
        public double KnockbackDistance { get; }
        // Sprite clips of the weapon; null for weapons without a sprite, e.g. enemy claws.
        public WeaponAppearance Appearance { get; }
        // Game time after equipping before the weapon can attack, e.g. drawing the bow.
        public double EquipReadySeconds { get; }
        // Traces a line attack leaves along its path while it runs (the staff strike): each
        // trace applies TrailEffect to opponents touching it; TrailVisual names its particles.
        public ICombatEffect TrailEffect { get; }
        public string TrailVisual { get; }
        public bool HasTrail { get { return !string.IsNullOrEmpty(TrailVisual); } }
        // Lightning that jumps from each hit target to further opponents; null without.
        public ChainLightning ChainLightning { get; }
    }
}
