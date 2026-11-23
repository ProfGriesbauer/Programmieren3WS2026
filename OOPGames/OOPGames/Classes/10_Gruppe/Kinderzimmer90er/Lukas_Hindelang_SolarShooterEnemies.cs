using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Identifies the enemy type for missions and the renderer. The model only says
    // *what* an enemy is; how it looks is decided by the renderer.
    internal enum G10_ShooterEnemyKind
    {
        Driftling,
        Needlewing,
        Bulwark
    }

    // Template Method: the base class owns the shot timer, the aimed shot and the
    // horizontal clamp; subclasses only describe how they move and what they are worth.
    internal abstract class G10_SolarShooterEnemy : G10_ShooterEntity
    {
        protected float Direction;
        protected float Phase;
        float _ShotCooldown;

        protected G10_SolarShooterEnemy(float x, float y, float direction, float firstShot)
        {
            X = x;
            Y = y;
            Direction = direction;
            _ShotCooldown = firstShot;
        }

        public abstract G10_ShooterEnemyKind Kind { get; }
        public abstract int PointValue { get; }
        protected virtual float HalfSize { get { return 3; } }
        protected virtual float FireInterval { get { return 1.9f; } }
        protected virtual float ShotAimSpeed { get { return 16; } }
        protected virtual float ShotSpeed { get { return 60; } }

        public sealed override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - HalfSize, Y - HalfSize, HalfSize * 2, HalfSize * 2); }
        }

        public sealed override void Update(float deltaSeconds)
        {
            Move(deltaSeconds);
            X = Math.Max(G10_ShooterConfig.EnemyMinimumX, Math.Min(G10_ShooterConfig.EnemyMaximumX, X));
        }

        public G10_SolarShooterProjectile UpdateAndTryFire(
            float deltaSeconds,
            G10_SolarShooterPlayer player)
        {
            Update(deltaSeconds);
            _ShotCooldown -= deltaSeconds;
            if (_ShotCooldown > 0) return null;
            _ShotCooldown = FireInterval;
            return CreateShot(player);
        }

        protected abstract void Move(float deltaSeconds);

        protected virtual G10_SolarShooterProjectile CreateShot(G10_SolarShooterPlayer player)
        {
            float aim = Math.Sign(player.X - X) * ShotAimSpeed;
            return new G10_SolarShooterProjectile(X, Y + 4, aim, ShotSpeed);
        }
    }

    internal sealed class G10_Driftling : G10_SolarShooterEnemy
    {
        public G10_Driftling(float x, float y, float direction, float firstShot)
            : base(x, y, direction, firstShot) { }

        public override G10_ShooterEnemyKind Kind { get { return G10_ShooterEnemyKind.Driftling; } }
        public override int PointValue { get { return 75; } }

        protected override void Move(float deltaSeconds)
        {
            Phase += deltaSeconds * 2;
            X += Direction * (18 + (float)Math.Sin(Phase) * 22) * deltaSeconds;
            Y += 22 * deltaSeconds;
        }
    }

    internal sealed class G10_Needlewing : G10_SolarShooterEnemy
    {
        public G10_Needlewing(float x, float y, float direction, float firstShot)
            : base(x, y, direction, firstShot) { }

        public override G10_ShooterEnemyKind Kind { get { return G10_ShooterEnemyKind.Needlewing; } }
        public override int PointValue { get { return 125; } }

        protected override void Move(float deltaSeconds)
        {
            Phase += deltaSeconds * 4;
            X += Direction * 47 * deltaSeconds;
            if (X < 8) { X = 8; Direction = 1; }
            else if (X > 152) { X = 152; Direction = -1; }
            Y += 34 * deltaSeconds;
        }
    }

    internal sealed class G10_Bulwark : G10_SolarShooterEnemy
    {
        public G10_Bulwark(float x, float y, float direction, float firstShot)
            : base(x, y, direction, firstShot) { }

        public override G10_ShooterEnemyKind Kind { get { return G10_ShooterEnemyKind.Bulwark; } }
        public override int PointValue { get { return 175; } }
        protected override float HalfSize { get { return 4; } }
        protected override float FireInterval { get { return 1.35f; } }
        protected override float ShotAimSpeed { get { return 25; } }
        protected override float ShotSpeed { get { return 68; } }

        protected override void Move(float deltaSeconds)
        {
            Phase += deltaSeconds * 2;
            X += (float)Math.Sin(Phase) * 10 * deltaSeconds;
            Y += 14 * deltaSeconds;
        }
    }

    internal sealed class G10_EnemySpawner
    {
        int _SpawnCount;

        public G10_SolarShooterEnemy Spawn(IList<G10_SolarShooterEnemy> enemies)
        {
            if (enemies.Count >= 5) return null;

            float x = 14 + (_SpawnCount * 43 % 132);
            float direction = _SpawnCount % 2 == 0 ? 1 : -1;
            float firstShot = 1.1f + (_SpawnCount % 3) * 0.25f;
            G10_SolarShooterEnemy enemy;
            switch (_SpawnCount % 3)
            {
                case 0: enemy = new G10_Driftling(x, G10_ShooterConfig.PlayfieldTop, direction, firstShot); break;
                case 1: enemy = new G10_Needlewing(x, G10_ShooterConfig.PlayfieldTop, direction, firstShot); break;
                default: enemy = new G10_Bulwark(x, G10_ShooterConfig.PlayfieldTop, direction, firstShot); break;
            }
            _SpawnCount++;
            enemies.Add(enemy);
            return enemy;
        }

        public void Reset() { _SpawnCount = 0; }
    }
}
