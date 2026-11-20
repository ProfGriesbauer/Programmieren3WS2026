using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class PlayerAbilityDefinition
    {
        internal PlayerAbilityDefinition(
            string id, string name, string description,
            int unlockCost, int upgradeCost, int maximumRank,
            string durationDescription = null)
        {
            Id = id;
            Name = name;
            Description = description;
            UnlockCost = unlockCost;
            UpgradeCost = upgradeCost;
            MaximumRank = maximumRank;
            DurationDescription = durationDescription;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int UnlockCost { get; }
        public int UpgradeCost { get; }
        public int MaximumRank { get; }
        // Only abilities with a lasting effect can extend their duration.
        public bool HasDuration { get { return DurationDescription != null; } }
        public string DurationDescription { get; }
        public string CooldownUpgradeId { get { return Id + "-cooldown"; } }
        public string DurationUpgradeId { get { return HasDuration ? Id + "-duration" : null; } }

        public int GetNextRankCost(int currentRank)
        {
            return currentRank == 0 ? UnlockCost : UpgradeCost * currentRank;
        }
    }

    public static class PlayerAbilityCatalog
    {
        public const string PowerBlast = "power-blast";
        public const string HealingPulse = "healing-pulse";
        public const string SlowMotion = "slow-motion";

        // Every ability has its own cooldown and, where sensible, duration upgrade line.
        public const int ModifierMaximumRank = 5;
        public const double CooldownReductionPerRank = 0.12;

        // Slow motion lowers the game tick speed for everything in the arena.
        public const double SlowMotionBaseTimeScale = 0.5;
        public const double SlowMotionTimeScalePerRank = 0.05;
        public const double SlowMotionBaseDurationSeconds = 2.5;
        public const double SlowMotionDurationPerRank = 0.75;
        public const double SlowMotionBaseCooldownSeconds = 18;

        // Bullet Time: higher, expensive stage of the slow motion. The arena stays slowed,
        // but the player moves, attacks and recovers at normal speed.
        public const string BulletTime = "slow-motion-bullet-time";
        public const int BulletTimeCost = 8;
        public const int BulletTimeRequiredSlowMotionRank = 3;

        private static readonly IReadOnlyList<PlayerAbilityDefinition> _definitions =
            Array.AsReadOnly(new[]
            {
                new PlayerAbilityDefinition(
                    PowerBlast, "Power Blast",
                    "Verursacht Flächenschaden rund um den Spieler.",
                    3, 2, 5),
                new PlayerAbilityDefinition(
                    HealingPulse, "Heilimpuls",
                    "Heilt den Spieler direkt.",
                    2, 2, 5),
                new PlayerAbilityDefinition(
                    SlowMotion, "Zeitlupe",
                    "Senkt die Tick-Geschwindigkeit der Arena: Gegner, Spieler, Geschosse, "
                        + "Effekte und Abklingzeiten laufen verlangsamt. "
                        + "Höhere Ränge verlangsamen stärker.",
                    3, 2, 5,
                    "Verlängert die Zeitlupe um "
                        + SlowMotionDurationPerRank.ToString("0.##") + " s je Rang.")
            });

        public static IReadOnlyList<PlayerAbilityDefinition> Definitions
        {
            get { return _definitions; }
        }

        public static PlayerAbilityDefinition GetDefinition(string id)
        {
            foreach (PlayerAbilityDefinition definition in _definitions)
            {
                if (definition.Id == id)
                {
                    return definition;
                }
            }

            throw new ArgumentException("Unknown player ability: " + id, nameof(id));
        }

        // Finds the ability a cooldown or duration upgrade belongs to; null for other ids.
        public static PlayerAbilityDefinition FindModifierOwner(string modifierId)
        {
            foreach (PlayerAbilityDefinition definition in _definitions)
            {
                if (definition.CooldownUpgradeId == modifierId
                    || definition.DurationUpgradeId == modifierId)
                {
                    return definition;
                }
            }
            return null;
        }

        public static int GetModifierCost(int currentRank)
        {
            return currentRank + 1;
        }

        public static double GetCooldownMultiplier(int cooldownRank)
        {
            if (cooldownRank < 0 || cooldownRank > ModifierMaximumRank)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownRank));
            }
            return 1 - cooldownRank * CooldownReductionPerRank;
        }

        public static double GetSlowMotionTimeScale(int rank)
        {
            ValidateRank(SlowMotion, rank);
            return SlowMotionBaseTimeScale - (rank - 1) * SlowMotionTimeScalePerRank;
        }

        public static double GetSlowMotionDurationSeconds(int durationRank)
        {
            if (durationRank < 0 || durationRank > ModifierMaximumRank)
            {
                throw new ArgumentOutOfRangeException(nameof(durationRank));
            }
            return SlowMotionBaseDurationSeconds + durationRank * SlowMotionDurationPerRank;
        }

        public static Weapon CreateWeapon(string id, int rank)
        {
            return CreateWeapon(id, rank, 0, 0);
        }

        public static Weapon CreateWeapon(
            string id, int rank, int cooldownRank, int durationRank,
            bool hasBulletTime = false)
        {
            PlayerAbilityDefinition definition = GetDefinition(id);
            ValidateRank(id, rank);
            double cooldownMultiplier = GetCooldownMultiplier(cooldownRank);

            if (id == PowerBlast)
            {
                return new Weapon(
                    id, definition.Name, definition.Description,
                    new RadialAttackPattern(100 + (rank - 1) * 12, 0.25),
                    new DamageEffect(20 + (rank - 1) * 10),
                    3.5 * cooldownMultiplier,
                    WeaponTargetingMode.Opponents, "power-blast");
            }
            if (id == SlowMotion)
            {
                return new Weapon(
                    id, definition.Name, definition.Description,
                    new SelfPulseAttackPattern(70, 0.3),
                    new TimeSlowEffect(
                        GetSlowMotionTimeScale(rank),
                        GetSlowMotionDurationSeconds(durationRank),
                        hasBulletTime),
                    SlowMotionBaseCooldownSeconds * cooldownMultiplier,
                    WeaponTargetingMode.Self, "slow-motion");
            }

            return new Weapon(
                id, definition.Name, definition.Description,
                new SelfPulseAttackPattern(52, 0.28),
                new HealingEffect(20 + (rank - 1) * 10),
                5 * cooldownMultiplier,
                WeaponTargetingMode.Self, "healing-burst");
        }

        private static void ValidateRank(string id, int rank)
        {
            if (rank <= 0 || rank > GetDefinition(id).MaximumRank)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }
        }
    }
}
