using System;
namespace OOPGames
{
    public static class WeaponCatalog
    {
        public static readonly Weapon MagicWand = new Weapon(
            "magic-wand", "Magischer Stab",
            "Magische Projektile als Grundangriff; der Stabschlag wird freischaltbar.",
            new ProjectileAttackPattern(520, 540, 7),
            new DamageEffect(10), 0.55,
            WeaponTargetingMode.Opponents, "magic-projectile");

        public static readonly Weapon Bow = new Weapon(
            "bow", "Bogen",
            "Verschießt Pfeile; Waffen-Upgrades verbessern Schaden, Autoaim und Effekte.",
            new ProjectileAttackPattern(650, 600, 6),
            new DamageEffect(12), 0.7,
            WeaponTargetingMode.Opponents, "bow-arrow");

        public static readonly Weapon Sword = new Weapon(
            "sword", "Schwert",
            "Schwerer, über die Zeit schwenkender Schwerthieb mit hohem Grundschaden.",
            new SweepingArcAttackPattern(100, Math.PI * 0.72, Math.PI / 10, 0.58),
            new DamageEffect(14), 0.65,
            WeaponTargetingMode.Opponents, "sweeping-slash");

        public static readonly Weapon EnemyClaws = new Weapon(
            "enemy-claws", "Gegnerklauen",
            "Gegner-Nahkampfangriff: 10 Schaden, Reichweite 60.",
            new MeleeConeAttackPattern(60, Math.PI / 4, 0.2),
            new DamageEffect(10), 1.0,
            WeaponTargetingMode.Opponents, "enemy-claw-swipe");
    }
}
