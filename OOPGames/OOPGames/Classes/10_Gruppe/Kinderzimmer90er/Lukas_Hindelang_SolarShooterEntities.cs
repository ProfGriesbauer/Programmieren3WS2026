using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal interface IG10_Collidable
    {
        G10_SolarShooterBounds Bounds { get; }
    }

    // Common base of everything that moves in the playfield: position, hitbox, update.
    internal abstract class G10_ShooterEntity : IG10_Collidable, IG10_ShooterEntityView
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }
        public abstract G10_SolarShooterBounds Bounds { get; }
        public abstract void Update(float deltaSeconds);
    }

    internal sealed class G10_SolarShooterBounds
    {
        readonly float _X;
        readonly float _Y;
        readonly float _Width;
        readonly float _Height;

        public G10_SolarShooterBounds(float x, float y, float width, float height)
        {
            _X = x;
            _Y = y;
            _Width = width;
            _Height = height;
        }

        public bool Intersects(G10_SolarShooterBounds other)
        {
            return _X < other._X + other._Width &&
                   _X + _Width > other._X &&
                   _Y < other._Y + other._Height &&
                   _Y + _Height > other._Y;
        }
    }

    internal sealed class G10_SolarShooterPowerCell : G10_ShooterEntity
    {
        public G10_SolarShooterPowerCell(float x, float y) { X = x; Y = y; }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 3, Y - 3, 6, 6); }
        }
        public override void Update(float deltaSeconds) { Y += 20 * deltaSeconds; }
    }

    internal sealed class G10_SolarShooterProjectile : G10_ShooterEntity
    {
        public G10_SolarShooterProjectile(
            float x,
            float y,
            float velocityX,
            float velocityY,
            int weaponLevel = 1)
        {
            X = x;
            Y = y;
            VelocityX = velocityX;
            VelocityY = velocityY;
            WeaponLevel = weaponLevel;
        }

        public float VelocityX { get; private set; }
        public float VelocityY { get; private set; }
        public int WeaponLevel { get; private set; }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 1, Y - 2, 2, 4); }
        }
        public override void Update(float deltaSeconds)
        {
            X += VelocityX * deltaSeconds;
            Y += VelocityY * deltaSeconds;
        }
    }

    internal sealed class G10_SolarShooterPlayer : G10_ShooterEntity, IG10_ShooterPlayerView
    {
        readonly IG10_Weapon[] _Weapons =
        {
            new G10_SingleShotWeapon(),
            new G10_DoubleShotWeapon(),
            new G10_TripleShotWeapon()
        };
        float _InvulnerabilitySeconds;
        float _ShotCooldown;

        public G10_SolarShooterPlayer(float x, float y)
        {
            X = x;
            Y = y;
            Lives = G10_ShooterConfig.MaximumLives;
            WeaponLevel = 1;
        }

        public int Lives { get; private set; }
        public int WeaponLevel { get; private set; }
        public bool IsInvulnerable { get { return _InvulnerabilitySeconds > 0; } }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 4, Y - 4, 8, 8); }
        }

        public override void Update(float deltaSeconds)
        {
            _InvulnerabilitySeconds = Math.Max(0, _InvulnerabilitySeconds - deltaSeconds);
            _ShotCooldown = Math.Max(0, _ShotCooldown - deltaSeconds);
        }

        public void Move(float horizontal, float vertical, float deltaSeconds)
        {
            float length = (float)Math.Sqrt(horizontal * horizontal + vertical * vertical);
            if (length > 1) { horizontal /= length; vertical /= length; }
            X = Clamp(X + horizontal * G10_ShooterConfig.PlayerSpeed * deltaSeconds,
                G10_ShooterConfig.PlayerMinimumX, G10_ShooterConfig.PlayerMaximumX);
            Y = Clamp(Y + vertical * G10_ShooterConfig.PlayerSpeed * deltaSeconds,
                G10_ShooterConfig.PlayerMinimumY, G10_ShooterConfig.PlayerMaximumY);
        }

        // Polymorphic weapon: the current weapon level picks the IG10_Weapon strategy.
        public bool Fire(bool firePressed, IList<G10_SolarShooterProjectile> projectiles)
        {
            if (!firePressed || _ShotCooldown > 0) return false;
            IG10_Weapon weapon = _Weapons[WeaponLevel - 1];
            weapon.Fire(this, projectiles);
            _ShotCooldown = weapon.Cooldown;
            return true;
        }

        public void UpgradeWeapon() { WeaponLevel = Math.Min(3, WeaponLevel + 1); }
        public void DowngradeWeapon() { WeaponLevel = Math.Max(1, WeaponLevel - 1); }
        public void AddLife() { Lives = Math.Min(G10_ShooterConfig.MaximumLives, Lives + 1); }
        public void RestoreWeaponLevel(int weaponLevel)
        {
            WeaponLevel = Math.Max(1, Math.Min(3, weaponLevel));
        }

        public bool TakeHit()
        {
            if (IsInvulnerable) return false;
            Lives--;
            DowngradeWeapon();
            ResetPosition();
            _InvulnerabilitySeconds = 1.4f;
            return true;
        }

        public void ResetPosition()
        {
            X = G10_ShooterConfig.PlayerStartX;
            Y = G10_ShooterConfig.PlayerStartY;
        }

        static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }

    internal readonly struct G10_SolarShooterInput
    {
        public G10_SolarShooterInput(float horizontal, float vertical, bool firePressed)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            FirePressed = firePressed;
        }

        public static G10_SolarShooterInput Neutral { get; } =
            new G10_SolarShooterInput(0, 0, false);

        public float Horizontal { get; }
        public float Vertical { get; }
        public bool FirePressed { get; }
    }
}
