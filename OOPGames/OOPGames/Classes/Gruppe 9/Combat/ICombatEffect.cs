using System;
using System.Collections.Generic;

namespace OOPGames
{
    public enum CombatTeam
    {
        Neutral,
        Player,
        Enemies
    }

    public enum CombatEffectKind
    {
        Damage,
        Healing,
        Slow,
        Freeze,
        Mixed
    }

    public interface ICombatEffect
    {
        string Name { get; }
        string Description { get; }
        CombatEffectKind Kind { get; }
        void Apply(Entity source, Entity target);
    }

    public sealed class DamageEffect : ICombatEffect
    {
        public DamageEffect(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Amount = amount;
        }

        public int Amount { get; }
        public string Name { get { return "Damage"; } }
        public string Description { get { return Amount + " damage"; } }
        public CombatEffectKind Kind { get { return CombatEffectKind.Damage; } }

        public void Apply(Entity source, Entity target)
        {
            target.TakeDamage(Amount);
        }
    }

    public sealed class HealingEffect : ICombatEffect
    {
        public HealingEffect(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Amount = amount;
        }

        public int Amount { get; }
        public string Name { get { return "Healing"; } }
        public string Description { get { return "Restores " + Amount + " HP"; } }
        public CombatEffectKind Kind { get { return CombatEffectKind.Healing; } }

        public void Apply(Entity source, Entity target)
        {
            target.Heal(Amount);
        }
    }

    public sealed class SlowEffect : ICombatEffect
    {
        public SlowEffect(double speedMultiplier, double durationSeconds)
        {
            if (speedMultiplier < 0 || speedMultiplier > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(speedMultiplier));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            SpeedMultiplier = speedMultiplier;
            DurationSeconds = durationSeconds;
        }

        public double SpeedMultiplier { get; }
        public double DurationSeconds { get; }
        public string Name { get { return "Slow"; } }
        public string Description
        {
            get
            {
                return Math.Round((1 - SpeedMultiplier) * 100)
                    + "% slow for " + DurationSeconds.ToString("0.0") + " s";
            }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Slow; } }

        public void Apply(Entity source, Entity target)
        {
            target.ApplyMovementStatus("slow", Name, SpeedMultiplier, DurationSeconds);
        }
    }

    public sealed class FreezeEffect : ICombatEffect
    {
        public FreezeEffect(double durationSeconds)
        {
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            DurationSeconds = durationSeconds;
        }

        public double DurationSeconds { get; }
        public string Name { get { return "Freeze"; } }
        public string Description
        {
            get { return "Stops movement for " + DurationSeconds.ToString("0.0") + " s"; }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Freeze; } }

        public void Apply(Entity source, Entity target)
        {
            target.ApplyMovementStatus("freeze", Name, 0, DurationSeconds);
        }
    }

    public sealed class CompositeCombatEffect : ICombatEffect
    {
        private readonly IReadOnlyList<ICombatEffect> _effects;

        public CompositeCombatEffect(params ICombatEffect[] effects)
        {
            if (effects == null || effects.Length == 0)
            {
                throw new ArgumentException("At least one effect is required.", nameof(effects));
            }

            _effects = Array.AsReadOnly((ICombatEffect[])effects.Clone());
        }

        public string Name { get { return "Combined effect"; } }
        public string Description
        {
            get
            {
                List<string> descriptions = new List<string>();
                foreach (ICombatEffect effect in _effects)
                {
                    descriptions.Add(effect.Description);
                }
                return string.Join(" + ", descriptions);
            }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Mixed; } }

        public void Apply(Entity source, Entity target)
        {
            foreach (ICombatEffect effect in _effects)
            {
                effect.Apply(source, target);
                if (!target.IsAlive)
                {
                    break;
                }
            }
        }
    }

    public sealed class ActiveStatusEffect
    {
        internal ActiveStatusEffect(
            string id, string name, double speedMultiplier, double remainingSeconds)
        {
            Id = id;
            Name = name;
            SpeedMultiplier = speedMultiplier;
            RemainingSeconds = remainingSeconds;
        }

        public string Id { get; }
        public string Name { get; }
        public double SpeedMultiplier { get; }
        public double RemainingSeconds { get; internal set; }
    }
}
