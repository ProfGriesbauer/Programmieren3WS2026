using System;
using System.Collections.Generic;

namespace OOPGames
{
    public static class SwordUpgradeCatalog
    {
        public const string StrikeDamage = "sword-strike-damage";
        public const string Spin = "sword-spin";
        public const string SpinDamage = "sword-spin-damage";
        public const string Poison = "sword-element-poison";
        public const string Lightning = "sword-element-lightning";
        public const string Fire = "sword-element-fire";
        public const string Freeze = "sword-element-freeze";
        public const string ElementPower = "sword-element-power";
        public const string Range = "sword-range";
        public const string Knockback = "sword-knockback";
        public const string SpinAbility = "sword-spin-ability";

        private static readonly IReadOnlyList<WeaponUpgradeDefinition> _upgrades =
            Array.AsReadOnly(new[]
            {
                new WeaponUpgradeDefinition(
                    StrikeDamage, "Schlagkraft",
                    "Erhöht den Schaden des normalen Schwerthiebs.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    Spin, "Drehwirbel",
                    "Schaltet drei rotierende Schwerthiebe um den Spieler frei.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    SpinDamage, "Wirbelschaden",
                    "Erhöht den Schaden jedes Treffers des Drehwirbels.",
                    5, 1, Spin),
                new WeaponUpgradeDefinition(
                    Poison, "Giftklinge",
                    "Vergiftet Gegner, die von der Klinge getroffen werden.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    Lightning, "Blitzklinge",
                    "Betäubt getroffene Gegner kurzzeitig.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    Fire, "Flammenklinge",
                    "Verbrennt Gegner; Feuer kann auf nahe Gegner überspringen.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    Freeze, "Frostklinge",
                    "Friert getroffene Gegner kurzzeitig ein.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    ElementPower, "Elementstärke",
                    "Verbessert Stärke und Dauer des gewählten Klingeneffekts.",
                    5, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    Range, "Reichweite",
                    "Erhöht die Reichweite der Schwerthiebe.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    Knockback, "Rückstoß",
                    "Erhöht den Rückstoß, den Schwerthiebe bei Treffern verursachen.",
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

            throw new ArgumentException("Unknown sword upgrade: " + id, nameof(id));
        }

        public static bool IsElement(string id)
        {
            return id == Poison || id == Lightning || id == Fire || id == Freeze;
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
                    progression.SelectedSwordElement) ?? StrikeDamage;
            }

            return definition.BaseCost
                * (progression.GetWeaponUpgradeRank(id) + 1);
        }

        public static Weapon CreateSword(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            int damage = 14
                + progression.GetWeaponUpgradeRank(StrikeDamage) * 3;
            double range = 100 * GetRangeMultiplier(progression);
            return new Weapon(
                WeaponCatalog.Sword.Id, WeaponCatalog.Sword.Name,
                "Schwert: weiter, schwenkender Hieb mit wählbarem Element.",
                new SweepingArcAttackPattern(
                    range, Math.PI * 0.72, Math.PI / 10, 0.58),
                CreateSwordEffect(progression, damage, 75),
                0.65, WeaponTargetingMode.Opponents, "sweeping-slash",
                knockbackDistance: GetKnockbackDistance(progression));
        }

        public static Weapon CreateSpinWeapon(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }
            if (progression.GetWeaponUpgradeRank(Spin) == 0)
            {
                throw new InvalidOperationException(
                    "The sword spin ability has not been unlocked.");
            }

            int damage = 8
                + progression.GetWeaponUpgradeRank(SpinDamage) * 3;
            double range = 115 * GetRangeMultiplier(progression);
            return new Weapon(
                SpinAbility, "Drehwirbel",
                "Der Spieler dreht sich dreimal und trifft Gegner bei jedem Umlauf.",
                new SpinningAttackPattern(range, 3, Math.PI / 8, 0.9),
                CreateSwordEffect(progression, damage, 75),
                4.5, WeaponTargetingMode.Opponents, "sword-spin", 3,
                GetKnockbackDistance(progression));
        }

        private static double GetRangeMultiplier(ArenaProgression progression)
        {
            return 1 + progression.GetWeaponUpgradeRank(Range) * 0.1;
        }

        private static double GetKnockbackDistance(ArenaProgression progression)
        {
            return 8 + progression.GetWeaponUpgradeRank(Knockback) * 5;
        }

        private static ICombatEffect CreateSwordEffect(
            ArenaProgression progression, int damage, double fireSpreadRadius)
        {
            List<ICombatEffect> effects = new List<ICombatEffect>
            {
                new DamageEffect(damage)
            };
            int elementRank = progression.GetWeaponUpgradeRank(ElementPower);
            if (progression.SelectedSwordElement != WeaponElement.None)
            {
                effects.Add(WeaponElementEffectFactory.Create(
                    progression.SelectedSwordElement,
                    elementRank, fireSpreadRadius + elementRank * 10));
            }

            return effects.Count == 1
                ? effects[0]
                : new CompositeCombatEffect(effects.ToArray());
        }
    }
}
