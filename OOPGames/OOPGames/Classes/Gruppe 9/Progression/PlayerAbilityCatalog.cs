using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class PlayerAbilityDefinition
    {
        internal PlayerAbilityDefinition(
            string id, string name, string description,
            int unlockCost, int upgradeCost, int maximumRank)
        {
            Id = id;
            Name = name;
            Description = description;
            UnlockCost = unlockCost;
            UpgradeCost = upgradeCost;
            MaximumRank = maximumRank;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int UnlockCost { get; }
        public int UpgradeCost { get; }
        public int MaximumRank { get; }

        public int GetNextRankCost(int currentRank)
        {
            return currentRank == 0 ? UnlockCost : UpgradeCost * currentRank;
        }
    }

    public static class PlayerAbilityCatalog
    {
        public const string PowerBlast = "power-blast";
        public const string HealingPulse = "healing-pulse";

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
                    2, 2, 5)
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

        public static Weapon CreateWeapon(string id, int rank)
        {
            if (rank <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            PlayerAbilityDefinition definition = GetDefinition(id);
            if (rank > definition.MaximumRank)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            if (id == PowerBlast)
            {
                return new Weapon(
                    id, definition.Name, definition.Description,
                    new RadialAttackPattern(100 + (rank - 1) * 12, 0.25),
                    new DamageEffect(20 + (rank - 1) * 10),
                    Math.Max(1.4, 3.5 - (rank - 1) * 0.25),
                    WeaponTargetingMode.Opponents, "power-blast");
            }

            return new Weapon(
                id, definition.Name, definition.Description,
                new SelfPulseAttackPattern(52, 0.28),
                new HealingEffect(20 + (rank - 1) * 10),
                Math.Max(2, 5 - (rank - 1) * 0.5),
                WeaponTargetingMode.Self, "healing-burst");
        }
    }
}
