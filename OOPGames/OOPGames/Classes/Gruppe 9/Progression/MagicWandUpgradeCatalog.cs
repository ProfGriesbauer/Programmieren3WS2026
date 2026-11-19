using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class WeaponUpgradeDefinition
    {
        internal WeaponUpgradeDefinition(
            string id, string name, string description,
            int maximumRank, int baseCost, string prerequisite)
        {
            Id = id;
            Name = name;
            Description = description;
            MaximumRank = maximumRank;
            BaseCost = baseCost;
            Prerequisite = prerequisite;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaximumRank { get; }
        public int BaseCost { get; }
        public string Prerequisite { get; }
    }

    public static class MagicWandUpgradeCatalog
    {
        public const string Strike = "strike";
        public const string StrikeDamage = "strike-damage";
        public const string ProjectileDamage = "projectile-damage";
        public const string Poison = "element-poison";
        public const string Lightning = "element-lightning";
        public const string Fire = "element-fire";
        public const string Freeze = "element-freeze";
        public const string ElementPower = "element-power";
        public const string Range = "magic-wand-range";
        public const string Knockback = "magic-wand-knockback";
        public const string StrikeAbility = "magic-wand-strike";

        private static readonly IReadOnlyList<WeaponUpgradeDefinition> _upgrades =
            Array.AsReadOnly(new[]
            {
                new WeaponUpgradeDefinition(
                    Strike, "Stabschlag",
                    "Schaltet einen langen, schmalen Hieb in Blickrichtung frei.",
                    1, 2, null),
                new WeaponUpgradeDefinition(
                    StrikeDamage, "Stabschaden",
                    "Erhöht den Schaden des freigeschalteten Nahkampfschlags.",
                    5, 1, Strike),
                new WeaponUpgradeDefinition(
                    ProjectileDamage, "Projektilschaden", "Erhöht den Grundschaden des Projektils.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    Poison, "Gift", "Vergiftet getroffene Gegner und verursacht Schaden über Zeit.",
                    1, 2, ProjectileDamage),
                new WeaponUpgradeDefinition(
                    Lightning, "Blitz", "Betäubt getroffene Gegner kurzzeitig.",
                    1, 2, ProjectileDamage),
                new WeaponUpgradeDefinition(
                    Fire, "Feuer", "Verbrennt Gegner; Feuer kann auf nahe Gegner überspringen.",
                    1, 2, ProjectileDamage),
                new WeaponUpgradeDefinition(
                    Freeze, "Frost", "Friert getroffene Gegner kurzzeitig ein.",
                    1, 2, ProjectileDamage),
                new WeaponUpgradeDefinition(
                    ElementPower, "Elementstärke", "Verbessert Stärke und Dauer des Elements.",
                    5, 2, null),
                new WeaponUpgradeDefinition(
                    Range, "Reichweite",
                    "Erhöht die Reichweite der Stabangriffe.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    Knockback, "Rückstoß",
                    "Erhöht den Rückstoß, den Stabangriffe bei Treffern verursachen.",
                    5, 1, null)
            });

        public static IReadOnlyList<WeaponUpgradeDefinition> Upgrades { get { return _upgrades; } }

        public static WeaponUpgradeDefinition GetDefinition(string id)
        {
            foreach (WeaponUpgradeDefinition definition in _upgrades)
            {
                if (definition.Id == id)
                {
                    return definition;
                }
            }

            throw new ArgumentException("Unknown weapon upgrade: " + id, nameof(id));
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
                    progression.SelectedMagicWandElement) ?? ProjectileDamage;
            }

            return definition.BaseCost
                * (progression.GetWeaponUpgradeRank(id) + 1);
        }

        public static Weapon CreateMagicWand(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            int projectileDamage = 10
                + progression.GetWeaponUpgradeRank(ProjectileDamage) * 3;
            double range = 520 * GetRangeMultiplier(progression);
            return new Weapon(
                WeaponCatalog.MagicWand.Id, WeaponCatalog.MagicWand.Name,
                "Magischer Stab: magisches Projektil mit ausbaubarem Schaden und Elementen.",
                new ProjectileAttackPattern(range, 540, 7),
                CreateWandEffect(progression, projectileDamage),
                0.55, WeaponTargetingMode.Opponents, "magic-projectile",
                knockbackDistance: GetKnockbackDistance(progression));
        }

        public static Weapon CreateStrikeWeapon(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }
            if (progression.GetWeaponUpgradeRank(Strike) == 0)
            {
                throw new InvalidOperationException(
                    "The magic wand strike has not been unlocked.");
            }

            int damage = 8
                + progression.GetWeaponUpgradeRank(StrikeDamage) * 3;
            double range = 150 * GetRangeMultiplier(progression);
            return new Weapon(
                StrikeAbility, "Stabschlag",
                "Langer, schmaler Hieb in Blickrichtung des Spielers.",
                new MeleeConeAttackPattern(range, Math.PI / 36, 0.26),
                CreateWandEffect(progression, damage),
                0.8, WeaponTargetingMode.Opponents, "magic-wand-strike",
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

        private static ICombatEffect CreateWandEffect(
            ArenaProgression progression, int damage)
        {
            List<ICombatEffect> effects = new List<ICombatEffect>
            {
                new DamageEffect(damage)
            };
            int elementRank = progression.GetWeaponUpgradeRank(ElementPower);
            switch (progression.SelectedMagicWandElement)
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
