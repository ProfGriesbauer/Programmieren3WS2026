using System;
using System.Collections.Generic;

namespace OOPGames
{
    public static class ArenaWeaponWheel
    {
        private static readonly IReadOnlyList<Weapon> _weapons =
            Array.AsReadOnly(new[]
            {
                WeaponCatalog.Sword,
                WeaponCatalog.MagicWand,
                WeaponCatalog.Bow
            });

        public static IReadOnlyList<Weapon> Weapons { get { return _weapons; } }

        public static int GetSelectionIndex(double directionX, double directionY)
        {
            double lengthSquared = directionX * directionX + directionY * directionY;
            if (lengthSquared == 0)
            {
                return 0;
            }

            double segmentAngle = Math.PI * 2 / _weapons.Count;
            double angle = Math.Atan2(directionY, directionX)
                + Math.PI / 2 + segmentAngle / 2;
            angle %= Math.PI * 2;
            if (angle < 0)
            {
                angle += Math.PI * 2;
            }

            return Math.Min(
                _weapons.Count - 1, (int)(angle / segmentAngle));
        }
    }
}
