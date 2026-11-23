using System;
using System.Collections.Generic;

namespace OOPGames
{
    public static class BowUpgradeCatalog
    {
        public const string AutoAim = "bow-auto-aim";
        public const string AttackCooldown = "bow-attack-cooldown";
        public const double BaseCooldownSeconds = 0.7;
        public const string ArrowDamage = "bow-arrow-damage";
        public const string Poison = "bow-element-poison";
        public const string Lightning = "bow-element-lightning";
        public const string Fire = "bow-element-fire";
        public const string Freeze = "bow-element-freeze";
        public const string ElementPower = "bow-element-power";
        public const string Range = "bow-range";
        public const string Knockback = "bow-knockback";

        private static readonly IReadOnlyList<WeaponUpgradeDefinition> _upgrades =
            Array.AsReadOnly(new[]
            {
                new WeaponUpgradeDefinition(
                    AutoAim, "Autoaim",
                    "Erweitert den Zielkegel und verbessert die Pfeilsteuerung; Rang 5 zielt rundum.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    AttackCooldown, "Schusstempo",
                    "Senkt die Abklingzeit zwischen zwei Pfeilen um 12 % je Rang.",
                    PlayerAbilityCatalog.ModifierMaximumRank, 1, null),
                new WeaponUpgradeDefinition(
                    ArrowDamage, "Pfeilschaden",
                    "Erhöht den Grundschaden jedes Pfeils.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    Poison, "Giftpfeile",
                    "Vergiftet getroffene Gegner und verursacht Schaden über Zeit.",
                    1, 2, ArrowDamage),
                new WeaponUpgradeDefinition(
                    Lightning, "Blitzpfeile",
                    "Betäubt getroffene Gegner kurzzeitig.",
                    1, 2, ArrowDamage),
                new WeaponUpgradeDefinition(
                    Fire, "Brandpfeile",
                    "Verbrennt Gegner; Feuer kann auf nahe Gegner überspringen.",
                    1, 2, ArrowDamage),
                new WeaponUpgradeDefinition(
                    Freeze, "Frostpfeile",
                    "Friert getroffene Gegner kurzzeitig ein.",
                    1, 2, ArrowDamage),
                new WeaponUpgradeDefinition(
                    ElementPower, "Elementstärke",
                    "Verbessert Stärke und Dauer des gewählten Pfeileffekts.",
                    5, 2, ArrowDamage),
                new WeaponUpgradeDefinition(
                    Range, "Reichweite",
                    "Erhöht die Flugreichweite der Pfeile.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    Knockback, "Rückstoß",
                    "Erhöht den Rückstoß, den Pfeile bei Treffern verursachen.",
                    5, 1, null)
            });

        public static IReadOnlyList<WeaponUpgradeDefinition> Upgrades
        {
            get { return _upgrades; }
        }

        public static WeaponUpgradeDefinition GetDefinition(string id)
        {
            foreach (WeaponUpgradeDefinition definition in _upgrades)
            {
                if (definition.Id == id)
                {
                    return definition;
                }
            }

            throw new ArgumentException("Unknown bow upgrade: " + id, nameof(id));
        }

        public static bool IsElement(string id)
        {
            return id == Poison || id == Lightning || id == Fire || id == Freeze;
        }

        public static double GetAutoAimHalfAngleRadians(int rank)
        {
            switch (rank)
            {
                case 1: return Math.PI / 18;
                case 2: return Math.PI / 9;
                case 3: return Math.PI / 4;
                case 4: return Math.PI / 2;
                default: return rank >= 5 ? Math.PI : 0;
            }
        }

        public static double GetHomingTurnRateRadiansPerSecond(int rank)
        {
            return Math.Max(0, Math.Min(5, rank)) * 2.0;
        }

        public static int GetNextRankCost(
            ArenaProgression progression, string id,
            out int maximumRank, out string prerequisite)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            WeaponUpgradeDefinition definition = GetDefinition(id);
            maximumRank = definition.MaximumRank;
            prerequisite = definition.Prerequisite;
            if (id == ElementPower)
            {
                prerequisite = GetSelectedElementUpgrade(
                    progression.SelectedBowElement) ?? ArrowDamage;
            }

            return definition.BaseCost
                * (progression.GetWeaponUpgradeRank(id) + 1);
        }

        public static Weapon CreateBow(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            int autoAimRank = progression.GetWeaponUpgradeRank(AutoAim);
            double range = 650 * GetRangeMultiplier(progression);
            int damage = 12
                + progression.GetWeaponUpgradeRank(ArrowDamage) * 3;
            return new Weapon(
                WeaponCatalog.Bow.Id, WeaponCatalog.Bow.Name,
                "Bogen: schnelle Projektile mit ausbaubarem Autoaim und Elementen.",
                new ProjectileAttackPattern(
                    range, 600, 6,
                    GetAutoAimHalfAngleRadians(autoAimRank),
                    GetHomingTurnRateRadiansPerSecond(autoAimRank)),
                CreateBowEffect(progression, damage),
                BaseCooldownSeconds * PlayerAbilityCatalog.GetCooldownMultiplier(
                    progression.GetWeaponUpgradeRank(AttackCooldown)),
                WeaponTargetingMode.Opponents, "bow-arrow",
                knockbackDistance: GetKnockbackDistance(progression));
        }

        private static double GetRangeMultiplier(ArenaProgression progression)
        {
            return 1 + progression.GetWeaponUpgradeRank(Range) * 0.1;
        }

        private static double GetKnockbackDistance(ArenaProgression progression)
        {
            return 8 + progression.GetWeaponUpgradeRank(Knockback) * 5;
        }

        private static string GetSelectedElementUpgrade(WeaponElement element)
        {
            switch (element)
            {
                case WeaponElement.Poison: return Poison;
                case WeaponElement.Lightning: return Lightning;
                case WeaponElement.Fire: return Fire;
                case WeaponElement.Freeze: return Freeze;
                default: return null;
            }
        }

        private static ICombatEffect CreateBowEffect(
            ArenaProgression progression, int damage)
        {
            List<ICombatEffect> effects = new List<ICombatEffect>
            {
                new DamageEffect(damage)
            };
            int elementRank = progression.GetWeaponUpgradeRank(ElementPower);
            switch (progression.SelectedBowElement)
            {
                case WeaponElement.Poison:
                    effects.Add(WeaponElementEffectFactory.Create(
                        WeaponElement.Poison, elementRank, 70 + elementRank * 10));
                    break;
                case WeaponElement.Lightning:
                    effects.Add(WeaponElementEffectFactory.Create(
                        WeaponElement.Lightning, elementRank, 0));
                    break;
                case WeaponElement.Fire:
                    effects.Add(WeaponElementEffectFactory.Create(
                        WeaponElement.Fire, elementRank, 70 + elementRank * 10));
                    break;
                case WeaponElement.Freeze:
                    effects.Add(WeaponElementEffectFactory.Create(
                        WeaponElement.Freeze, elementRank, 0));
                    break;
            }

            return effects.Count == 1
                ? effects[0]
                : new CompositeCombatEffect(effects.ToArray());
        }
    }
}
