using System;
using System.Collections.Generic;

namespace OOPGames
{
    // How a weapon sprite is held and swung; decides grip point and pose in the renderer.
    public enum WeaponSpriteKind
    {
        Blade,
        Staff,
        Bow
    }

    // Names the clips of Assets/Weapons/Weapons.json that show a weapon. The upgrade catalogs
    // choose them from the run progression; ArenaWeaponRenderer draws them.
    public sealed class WeaponAppearance
    {
        // Distance in front of the bow grip where the tip of the nocked arrow lies
        // (5 weapon pixels at 2.6 world units per pixel). Arrows are launched there.
        public const double BowArrowTipDistance = 13;

        public WeaponAppearance(
            WeaponSpriteKind kind, double reach, string heldAnimation,
            string attackAnimation = null,
            params string[] projectileAnimations)
        {
            if (reach < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reach));
            }
            if (string.IsNullOrWhiteSpace(heldAnimation))
            {
                throw new ArgumentException(
                    "A held animation is required.", nameof(heldAnimation));
            }

            Kind = kind;
            Reach = reach;
            HeldAnimation = heldAnimation;
            AttackAnimation = string.IsNullOrWhiteSpace(attackAnimation)
                ? heldAnimation
                : attackAnimation;
            ProjectileAnimations = Array.AsReadOnly(
                (string[])(projectileAnimations ?? Array.Empty<string>()).Clone());
        }

        public WeaponSpriteKind Kind { get; }
        // Distance from the body center that blades and staffs reach when attacking, equal to
        // the range of their damage area; the renderer scales the sprite to it. Bows keep a
        // fixed size and use 0.
        public double Reach { get; }
        // Clip shown while the weapon is carried.
        public string HeldAnimation { get; }
        // Clip shown while attacking; equals HeldAnimation when there is no extra attack frame.
        public string AttackAnimation { get; }
        // Clips a flying projectile cycles through; empty for weapons without projectile sprites.
        public IReadOnlyList<string> ProjectileAnimations { get; }
        public bool HasProjectileSprite { get { return ProjectileAnimations.Count > 0; } }
        // Distance in front of the hand where projectiles of this weapon start; null keeps
        // the default (body radius plus projectile radius). Bows shoot from the arrow tip.
        public double? ProjectileLaunchDistance
        {
            get { return Kind == WeaponSpriteKind.Bow ? BowArrowTipDistance : (double?)null; }
        }
    }
}
