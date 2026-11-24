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
        public const string BlackSteel = "sword-element-black-steel";
        public const string Leach = "sword-black-steel-leach";
        public const string ElementPower = "sword-element-power";
        public const string Range = "sword-range";
        public const string Knockback = "sword-knockback";
        public const string AttackCooldown = "sword-attack-cooldown";
        public const double BaseCooldownSeconds = 0.65;
        // A slash lasts exactly as long as the cooldown: as soon as it is finished the next
        // one can start, and a lower cooldown makes the swing faster.
        public const double BaseSwingSeconds = BaseCooldownSeconds;
        public const double BaseSlashRange = 100;
        public const string SpinCooldown = "sword-spin-cooldown";
        public const string SpinDuration = "sword-spin-duration";
        public const string SpinAbility = "sword-spin-ability";
        // Revolutions of the spin without duration upgrades; each duration rank adds one.
        public const int SpinRotations = 3;
        public const double SpinSecondsPerRotation = 0.3;
        public const double SpinBaseCooldownSeconds = 4.5;

        private static readonly IReadOnlyList<WeaponUpgradeDefinition> _upgrades =
            Array.AsReadOnly(new[]
            {
                new WeaponUpgradeDefinition(
                    StrikeDamage, "Schlagkraft",
                    "Erhöht den Schaden des normalen Schwerthiebs.",
                    5, 1, null),
                new WeaponUpgradeDefinition(
                    AttackCooldown, "Hiebtempo",
                    "Senkt die Abklingzeit des Schwerthiebs um 12 % je Rang; der Schwung wird entsprechend schneller.",
                    PlayerAbilityCatalog.ModifierMaximumRank, 1, null),
                new WeaponUpgradeDefinition(
                    Spin, "Drehwirbel",
                    "Schaltet drei rotierende Schwerthiebe um den Spieler frei.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    SpinDamage, "Wirbelschaden",
                    "Erhöht den Schaden jedes Treffers des Drehwirbels.",
                    5, 1, Spin),
                new WeaponUpgradeDefinition(
                    SpinCooldown, "Wirbel-Abklingzeit",
                    "Senkt die Abklingzeit des Drehwirbels um 12 % je Rang.",
                    PlayerAbilityCatalog.ModifierMaximumRank, 1, Spin),
                new WeaponUpgradeDefinition(
                    SpinDuration, "Wirbeldauer",
                    "Der Drehwirbel dauert länger: eine zusätzliche Umdrehung je Rang.",
                    3, 2, Spin),
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
                    BlackSteel, "Schwarzstahlklinge",
                    "Schwächt getroffene Gegner: Sie verursachen eine Zeit lang weniger Schaden.",
                    1, 2, StrikeDamage),
                new WeaponUpgradeDefinition(
                    Leach, "Blutsauger",
                    "Die Schwarzstahlklinge stiehlt Lebensenergie: Ein Teil des Schadens heilt den Spieler.",
                    1, 3, BlackSteel),
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
            return id == Poison || id == Lightning || id == Fire || id == Freeze
                || id == BlackSteel;
        }

        private static string GetSelectedElementUpgrade(WeaponElement element)
        {
            switch (element)
            {
                case WeaponElement.Poison: return Poison;
                case WeaponElement.Lightning: return Lightning;
                case WeaponElement.Fire: return Fire;
                case WeaponElement.Freeze: return Freeze;
                case WeaponElement.BlackSteel: return BlackSteel;
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
            double range = GetSlashRange(progression);
            // Swing and cooldown shrink together, so the next slash can start right when the
            // previous one is finished.
            double tempo = PlayerAbilityCatalog.GetCooldownMultiplier(
                progression.GetWeaponUpgradeRank(AttackCooldown));
            double swingSeconds = BaseSwingSeconds * tempo;
            return new Weapon(
                WeaponCatalog.Sword.Id, WeaponCatalog.Sword.Name,
                "Schwert: weiter, schwenkender Hieb mit wählbarem Element.",
                new SweepingArcAttackPattern(
                    range, Math.PI * 0.72, Math.PI / 10, swingSeconds),
                CreateSwordEffect(progression, damage, 75),
                swingSeconds,
                WeaponTargetingMode.Opponents, "sweeping-slash",
                knockbackDistance: GetKnockbackDistance(progression),
                appearance: GetAppearance(progression));
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
            // The spin hits with the same blade as the slash.
            double range = GetSlashRange(progression);
            int rotations = GetSpinRotations(progression);
            return new Weapon(
                SpinAbility, "Drehwirbel",
                "Der Spieler dreht sich " + rotations
                    + "-mal und trifft Gegner bei jedem Umlauf.",
                new SpinningAttackPattern(
                    range, rotations, Math.PI / 8, rotations * SpinSecondsPerRotation),
                CreateSwordEffect(progression, damage, 75),
                SpinBaseCooldownSeconds * PlayerAbilityCatalog.GetCooldownMultiplier(
                    progression.GetWeaponUpgradeRank(SpinCooldown)),
                WeaponTargetingMode.Opponents, "sword-spin", rotations,
                GetKnockbackDistance(progression), GetAppearance(progression));
        }

        // Blade sprite of the chosen element; Lightning and Freeze have no own sprite yet.
        public static WeaponAppearance GetAppearance(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            // The blade is the damage area, so it is as long as the slash reaches; range
            // upgrades lengthen the sword.
            double reach = GetSlashRange(progression);
            switch (progression.SelectedSwordElement)
            {
                case WeaponElement.Poison:
                    return new WeaponAppearance(WeaponSpriteKind.Blade, reach, "SwordPoisen");
                case WeaponElement.Fire:
                    return new WeaponAppearance(
                        WeaponSpriteKind.Blade, reach, "SwordFire", "SwordFireAttack");
                case WeaponElement.BlackSteel:
                    return new WeaponAppearance(
                        WeaponSpriteKind.Blade, reach,
                        progression.GetWeaponUpgradeRank(Leach) > 0
                            ? "SwordBlackSteelLeach"
                            : "SwordBlackSteel");
                default:
                    return new WeaponAppearance(WeaponSpriteKind.Blade, reach, "Sword");
            }
        }

        public static int GetSpinRotations(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }
            return SpinRotations + progression.GetWeaponUpgradeRank(SpinDuration);
        }

        public static double GetSlashRange(ArenaProgression progression)
        {
            if (progression == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }
            return BaseSlashRange * GetRangeMultiplier(progression);
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

            ICombatEffect effect = effects.Count == 1
                ? effects[0]
                : new CompositeCombatEffect(effects.ToArray());
            return progression.GetWeaponUpgradeRank(Leach) > 0
                ? new LifeStealEffect(effect, GetLeachFraction(elementRank))
                : effect;
        }

        // Share of the dealt damage that Leach returns as healing: 25 %, +5 % per element rank.
        public static double GetLeachFraction(int elementRank)
        {
            return 0.25 + Math.Max(0, elementRank) * 0.05;
        }
    }
}
