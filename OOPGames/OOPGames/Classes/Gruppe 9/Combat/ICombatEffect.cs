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
        Mixed,
        Poison,
        Lightning,
        Fire
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

    // Carries the slow-motion parameters of the Zeitlupe ability. Hitting the caster changes
    // nothing by itself; ArenaField reads the values and lowers the tick speed of the arena.
    public sealed class TimeSlowEffect : ICombatEffect
    {
        public TimeSlowEffect(
            double timeScale, double durationSeconds, bool keepsCasterAtNormalSpeed = false)
        {
            if (timeScale <= 0 || timeScale >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeScale));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            TimeScale = timeScale;
            DurationSeconds = durationSeconds;
            KeepsCasterAtNormalSpeed = keepsCasterAtNormalSpeed;
        }

        public double TimeScale { get; }
        // Bullet Time: only the arena is slowed, the caster keeps the normal tick speed.
        public bool KeepsCasterAtNormalSpeed { get; }
        // Measured in unscaled ticks, so the slow motion does not stretch itself.
        public double DurationSeconds { get; }
        public string Name { get { return KeepsCasterAtNormalSpeed ? "Bullet Time" : "Zeitlupe"; } }
        public string Description
        {
            get
            {
                return "Spieltempo " + (TimeScale * 100).ToString("0") + " % für "
                    + DurationSeconds.ToString("0.0") + " s";
            }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Slow; } }

        public void Apply(Entity source, Entity target)
        {
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
        public CombatEffectKind Kind { get { return CombatEffectKind.Lightning; } }

        public void Apply(Entity source, Entity target)
        {
            target.ApplyMovementStatus("freeze", Name, 0, DurationSeconds);
        }
    }

    public sealed class StunEffect : ICombatEffect
    {
        public StunEffect(double durationSeconds)
        {
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            DurationSeconds = durationSeconds;
        }

        public double DurationSeconds { get; }
        public string Name { get { return "Stun"; } }
        public string Description
        {
            get { return "Stuns for " + DurationSeconds.ToString("0.0") + " s"; }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Freeze; } }

        public void Apply(Entity source, Entity target)
        {
            target.ApplyMovementStatus("stun", Name, 0, DurationSeconds);
        }
    }

    public sealed class PoisonEffect : ICombatEffect
    {
        public PoisonEffect(double damagePerSecond, double durationSeconds)
        {
            if (damagePerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerSecond));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            DamagePerSecond = damagePerSecond;
            DurationSeconds = durationSeconds;
        }

        public double DamagePerSecond { get; }
        public double DurationSeconds { get; }
        public string Name { get { return "Poison"; } }
        public string Description
        {
            get
            {
                return DamagePerSecond.ToString("0.#") + " damage/s for "
                    + DurationSeconds.ToString("0.0") + " s";
            }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Poison; } }

        public void Apply(Entity source, Entity target)
        {
            target.ApplyDamageOverTime(
                "poison", Name, DamagePerSecond, DurationSeconds, 0, source);
        }
    }

    public sealed class BurningEffect : ICombatEffect
    {
        public BurningEffect(
            double damagePerSecond, double durationSeconds, double spreadRadius)
        {
            if (damagePerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerSecond));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }
            if (spreadRadius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spreadRadius));
            }

            DamagePerSecond = damagePerSecond;
            DurationSeconds = durationSeconds;
            SpreadRadius = spreadRadius;
        }

        public double DamagePerSecond { get; }
        public double DurationSeconds { get; }
        public double SpreadRadius { get; }
        public string Name { get { return "Burning"; } }
        public string Description
        {
            get
            {
                return DamagePerSecond.ToString("0.#") + " fire damage/s for "
                    + DurationSeconds.ToString("0.0") + " s";
            }
        }
        public CombatEffectKind Kind { get { return CombatEffectKind.Fire; } }

        public void Apply(Entity source, Entity target)
        {
            target.ApplyDamageOverTime(
                "burning", Name, DamagePerSecond, DurationSeconds,
                SpreadRadius, source);
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

    public sealed class ActiveDamageOverTimeEffect
    {
        private double _damageRemainder;
        private double _spreadRemainingSeconds;

        internal ActiveDamageOverTimeEffect(
            string id, string name, double damagePerSecond,
            double remainingSeconds, double spreadRadius,
            Entity source, Weapon sourceWeapon)
        {
            Id = id;
            Name = name;
            DamagePerSecond = damagePerSecond;
            RemainingSeconds = remainingSeconds;
            SpreadRadius = spreadRadius;
            Source = source;
            SourceWeapon = sourceWeapon;
            _spreadRemainingSeconds = 0;
        }

        public string Id { get; }
        public string Name { get; }
        public double DamagePerSecond { get; internal set; }
        public double RemainingSeconds { get; internal set; }
        public double SpreadRadius { get; internal set; }
        public Entity Source { get; internal set; }
        public Weapon SourceWeapon { get; internal set; }
        public bool IsExpired { get { return RemainingSeconds <= 0; } }

        internal int AdvanceDamage(double deltaSeconds)
        {
            double elapsed = Math.Min(deltaSeconds, RemainingSeconds);
            RemainingSeconds -= deltaSeconds;
            _damageRemainder += DamagePerSecond * elapsed;
            int damage = (int)_damageRemainder;
            _damageRemainder -= damage;
            return damage;
        }

        internal bool AdvanceSpreadClock(double deltaSeconds)
        {
            if (SpreadRadius <= 0 || IsExpired)
            {
                return false;
            }

            _spreadRemainingSeconds -= deltaSeconds;
            if (_spreadRemainingSeconds > 0)
            {
                return false;
            }

            _spreadRemainingSeconds += 0.8;
            return true;
        }
    }
}
