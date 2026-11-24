using System;

namespace OOPGames
{
    public enum WeaponTargetingMode
    {
        Opponents,
        Allies,
        Self
    }

    public sealed class Weapon
    {
        public Weapon(
            string id, string name, string description, IAttackPattern attackPattern,
            ICombatEffect effect, double cooldownSeconds,
            WeaponTargetingMode targetingMode, string animationPlaceholder,
            int maximumHitsPerTarget = 1, double knockbackDistance = 8,
            WeaponAppearance appearance = null, double equipReadySeconds = 0)
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
    }
}
