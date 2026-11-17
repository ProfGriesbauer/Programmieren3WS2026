using System;
using System.Collections.Generic;

namespace OOPGames
{
    public static class WeaponCatalog
    {
        public static readonly Weapon PracticeSword = new Weapon(
            "practice-sword", "Übungsschwert",
            "Nahkampf: kurzer 60°-Kegel, trifft alle Gegner im Bereich.",
            new MeleeConeAttackPattern(80, Math.PI / 6, 0.2),
            new DamageEffect(10), 0.45,
            WeaponTargetingMode.Opponents, "sword-swing");

        public static readonly Weapon SweepingBlade = new Weapon(
            "sweeping-blade", "Schwingende Klinge",
            "Längerer, breiter Schwerthieb, der den Kegel sichtbar über die Zeit schwenkt.",
            new SweepingArcAttackPattern(92, Math.PI * 0.62, Math.PI / 12, 0.62),
            new DamageEffect(8), 0.8,
            WeaponTargetingMode.Opponents, "sweeping-slash");

        public static readonly Weapon Pistol = new Weapon(
            "pistol", "Testpistole",
            "Schnelles Projektil mit kleiner Trefferfläche.",
            new ProjectileAttackPattern(520, 720, 7),
            new DamageEffect(12), 0.36,
            WeaponTargetingMode.Opponents, "projectile-shot");

        public static readonly Weapon FreezeStaff = new Weapon(
            "freeze-staff", "Froststab",
            "Projektil verursacht leichten Schaden und friert kurz ein.",
            new ProjectileAttackPattern(400, 520, 9),
            new CompositeCombatEffect(new DamageEffect(2), new FreezeEffect(0.9)),
            1.1, WeaponTargetingMode.Opponents, "freeze-impact");

        public static readonly Weapon SlowWand = new Weapon(
            "slow-wand", "Verlangsamungsstab",
            "Projektil verursacht Schaden und verlangsamt für zwei Sekunden.",
            new ProjectileAttackPattern(450, 500, 9),
            new CompositeCombatEffect(
                new DamageEffect(4), new SlowEffect(0.45, 2.0)),
            0.8, WeaponTargetingMode.Opponents, "slow-impact");

        public static readonly Weapon HealingPulse = new Weapon(
            "healing-pulse", "Heilimpuls",
            "Heilt den Spieler um 25 Lebenspunkte.",
            new SelfPulseAttackPattern(52, 0.28),
            new HealingEffect(25), 2.0,
            WeaponTargetingMode.Self, "healing-burst");

        public static readonly Weapon EnemyClaws = new Weapon(
            "enemy-claws", "Gegnerklauen",
            "Gegner-Nahkampfangriff: 10 Schaden, Reichweite 60.",
            new MeleeConeAttackPattern(60, Math.PI / 4, 0.2),
            new DamageEffect(10), 1.0,
            WeaponTargetingMode.Opponents, "enemy-claw-swipe");

        private static readonly IReadOnlyList<Weapon> _testWeapons =
            Array.AsReadOnly(new[]
            {
                PracticeSword,
                SweepingBlade,
                Pistol,
                FreezeStaff,
                SlowWand,
                HealingPulse
            });

        public static IReadOnlyList<Weapon> TestWeapons { get { return _testWeapons; } }
    }
}
