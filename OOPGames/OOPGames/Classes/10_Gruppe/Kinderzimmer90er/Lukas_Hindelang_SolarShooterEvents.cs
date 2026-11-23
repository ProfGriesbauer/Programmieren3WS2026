using System;

namespace OOPGames
{
    internal sealed class G10_EntityDestroyedEventArgs : EventArgs
    {
        public G10_EntityDestroyedEventArgs(float x, float y, bool large)
            : this(x, y, large, null, 1) { }

        public G10_EntityDestroyedEventArgs(
            float x,
            float y,
            bool large,
            G10_ShooterEnemyKind? enemyKind,
            int weaponLevel,
            int pointValue = 0)
        {
            X = x;
            Y = y;
            Large = large;
            EnemyKind = enemyKind;
            WeaponLevel = weaponLevel;
            PointValue = pointValue;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public bool Large { get; private set; }
        // Null when the destroyed entity is not a regular enemy (player or boss).
        public G10_ShooterEnemyKind? EnemyKind { get; private set; }
        public int WeaponLevel { get; private set; }
        public int PointValue { get; private set; }
    }

    internal sealed class G10_ShotFiredEventArgs : EventArgs
    {
        public G10_ShotFiredEventArgs(int weaponLevel) { WeaponLevel = weaponLevel; }
        public int WeaponLevel { get; private set; }
    }

    internal sealed class G10_BossHitEventArgs : EventArgs
    {
        public G10_BossHitEventArgs(float x, float y) { X = x; Y = y; }
        public float X { get; private set; }
        public float Y { get; private set; }
    }
}
