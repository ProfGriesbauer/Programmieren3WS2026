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
            WeaponTargetingMode targetingMode, string animationPlaceholder)
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

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            AttackPattern = attackPattern;
            Effect = effect;
            CooldownSeconds = cooldownSeconds;
            TargetingMode = targetingMode;
            AnimationPlaceholder = animationPlaceholder ?? string.Empty;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public IAttackPattern AttackPattern { get; }
        public ICombatEffect Effect { get; }
        public double CooldownSeconds { get; }
        public WeaponTargetingMode TargetingMode { get; }
        public string AnimationPlaceholder { get; }
    }
}
