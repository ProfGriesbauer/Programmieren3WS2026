using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OOPGames
{
    internal enum G10_ShooterBackdrop
    {
        ShardField,
        SporeGrove,
        Ironworks
    }

    internal enum G10_ShooterPhase
    {
        Playing,
        BossWarning,
        StageTransition,
        GameOver,
        Victory
    }

    internal interface IG10_Collidable
    {
        G10_SolarShooterBounds Bounds { get; }
    }

    internal interface IG10_ShooterCollisionContext
    {
        IG10_Collidable PlayerCollider { get; }
        IG10_Collidable BossCollider { get; }
        IReadOnlyList<G10_SolarShooterEnemy> Enemies { get; }
        IReadOnlyList<G10_SolarShooterProjectile> PlayerProjectiles { get; }
        IReadOnlyList<G10_SolarShooterProjectile> EnemyProjectiles { get; }
        IReadOnlyList<G10_SolarShooterPowerCell> PowerCells { get; }
        void RemovePlayerProjectile(G10_SolarShooterProjectile projectile);
        void DestroyEnemy(G10_SolarShooterEnemy enemy);
        void RemoveEnemy(G10_SolarShooterEnemy enemy);
        void RemoveEnemyProjectile(G10_SolarShooterProjectile projectile);
        void HitPlayer();
        void HitBoss();
        void CollectPowerCell(G10_SolarShooterPowerCell powerCell);
    }

    internal interface IG10_ShooterView
    {
        G10_SolarShooterPlayer Player { get; }
        IReadOnlyList<G10_SolarShooterEnemy> Enemies { get; }
        IReadOnlyList<G10_SolarShooterProjectile> PlayerProjectiles { get; }
        IReadOnlyList<G10_SolarShooterProjectile> EnemyProjectiles { get; }
        IReadOnlyList<G10_SolarShooterPowerCell> PowerCells { get; }
        G10_SolarShooterBoss Boss { get; }
        G10_SolarShooterStage Stage { get; }
        int StageIndex { get; }
        int Score { get; }
        int TickNumber { get; }
        bool IsGameOver { get; }
        bool IsVictory { get; }
        bool IsBossWarning { get; }
        bool IsStageTransition { get; }
        event EventHandler<G10_EntityDestroyedEventArgs> EnemyDestroyed;
        event EventHandler<G10_EntityDestroyedEventArgs> PlayerHit;
        event EventHandler<G10_BossHitEventArgs> BossHit;
        event EventHandler<G10_EntityDestroyedEventArgs> BossDefeated;
    }

    internal abstract class G10_ShooterEntity : IG10_Collidable
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

    internal sealed class G10_ShooterEnemyAppearance
    {
        public G10_ShooterEnemyAppearance(
            int shapeIndex,
            double glowWidth,
            double glowHeight,
            bool useBulwarkHull,
            bool hasArmorLines,
            bool hasTrail)
        {
            ShapeIndex = shapeIndex;
            GlowWidth = glowWidth;
            GlowHeight = glowHeight;
            UseBulwarkHull = useBulwarkHull;
            HasArmorLines = hasArmorLines;
            HasTrail = hasTrail;
        }

        public int ShapeIndex { get; private set; }
        public double GlowWidth { get; private set; }
        public double GlowHeight { get; private set; }
        public bool UseBulwarkHull { get; private set; }
        public bool HasArmorLines { get; private set; }
        public bool HasTrail { get; private set; }
    }

    internal sealed class G10_CollisionResolver
    {
        static bool Intersects(IG10_Collidable first, IG10_Collidable second)
        {
            return first.Bounds.Intersects(second.Bounds);
        }

        public void ResolvePlayerShots(IG10_ShooterCollisionContext world)
        {
            for (int projectileIndex = world.PlayerProjectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                G10_SolarShooterProjectile projectile = world.PlayerProjectiles[projectileIndex];
                if (world.BossCollider != null && Intersects(world.BossCollider, projectile))
                {
                    world.RemovePlayerProjectile(projectile);
                    world.HitBoss();
                    continue;
                }

                for (int enemyIndex = world.Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
                {
                    G10_SolarShooterEnemy enemy = world.Enemies[enemyIndex];
                    if (!Intersects(projectile, enemy)) continue;

                    world.RemovePlayerProjectile(projectile);
                    world.DestroyEnemy(enemy);
                    break;
                }
            }
        }

        public void ResolveEnemyHits(IG10_ShooterCollisionContext world)
        {
            for (int i = world.EnemyProjectiles.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterProjectile projectile = world.EnemyProjectiles[i];
                if (Intersects(projectile, world.PlayerCollider))
                {
                    world.RemoveEnemyProjectile(projectile);
                    world.HitPlayer();
                }
            }

            for (int i = world.Enemies.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterEnemy enemy = world.Enemies[i];
                if (Intersects(world.PlayerCollider, enemy))
                {
                    world.RemoveEnemy(enemy);
                    world.HitPlayer();
                }
            }
        }

        public void ResolvePowerCells(IG10_ShooterCollisionContext world)
        {
            for (int i = world.PowerCells.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterPowerCell cell = world.PowerCells[i];
                if (!Intersects(world.PlayerCollider, cell)) continue;

                world.CollectPowerCell(cell);
            }
        }
    }

    internal interface IG10_Weapon
    {
        float Cooldown { get; }
        void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles);
    }

    internal sealed class G10_SingleShotWeapon : IG10_Weapon
    {
        public float Cooldown { get { return 0.24f; } }
        public void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(Create(player, 0));
        }
        public static G10_SolarShooterProjectile Create(G10_SolarShooterPlayer player, float horizontalOffset)
        {
            return new G10_SolarShooterProjectile(
                player.X + horizontalOffset, player.Y - 7, horizontalOffset * 1.5f, -150);
        }
    }

    internal sealed class G10_DoubleShotWeapon : IG10_Weapon
    {
        public float Cooldown { get { return 0.24f; } }
        public void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(G10_SingleShotWeapon.Create(player, -3));
            projectiles.Add(G10_SingleShotWeapon.Create(player, 3));
        }
    }

    internal sealed class G10_TripleShotWeapon : IG10_Weapon
    {
        public float Cooldown { get { return 0.3f; } }
        public void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(G10_SingleShotWeapon.Create(player, -5));
            projectiles.Add(G10_SingleShotWeapon.Create(player, 0));
            projectiles.Add(G10_SingleShotWeapon.Create(player, 5));
        }
    }

    internal sealed class G10_SolarShooterStage
    {
        public G10_SolarShooterStage(
            string name,
            G10_ShooterBackdrop backdrop,
            int enemiesToDefeat,
            int bossHealth,
            string bossName,
            float bossTempo,
            float bossAmplitude,
            float bossShotInterval,
            int bossScore,
            float spawnInterval,
            IG10_BossAttack bossAttack)
        {
            Name = name;
            Backdrop = backdrop;
            EnemiesToDefeat = enemiesToDefeat;
            BossHealth = bossHealth;
            BossName = bossName;
            BossTempo = bossTempo;
            BossAmplitude = bossAmplitude;
            BossShotInterval = bossShotInterval;
            BossScore = bossScore;
            SpawnInterval = spawnInterval;
            BossAttack = bossAttack;
        }

        public string Name { get; private set; }
        public G10_ShooterBackdrop Backdrop { get; private set; }
        public int EnemiesToDefeat { get; private set; }
        public int BossHealth { get; private set; }
        public string BossName { get; private set; }
        public float BossTempo { get; private set; }
        public float BossAmplitude { get; private set; }
        public float BossShotInterval { get; private set; }
        public int BossScore { get; private set; }
        public float SpawnInterval { get; private set; }
        public IG10_BossAttack BossAttack { get; private set; }

        public static IReadOnlyList<G10_SolarShooterStage> All { get; } =
            Array.AsReadOnly(new[]
            {
                new G10_SolarShooterStage("RIFT", G10_ShooterBackdrop.ShardField, 10, 15, "THE DRIFTER",
                    0.8f, 38, 1.05f, 1500, 1.05f, new G10_AimedBossAttack()),
                new G10_SolarShooterStage("CANOPY", G10_ShooterBackdrop.SporeGrove, 12, 19, "ROOT ENGINE",
                    0.98f, 35, 0.89f, 2000, 0.93f, new G10_SpreadBossAttack(false)),
                new G10_SolarShooterStage("IRONWORKS", G10_ShooterBackdrop.Ironworks, 14, 24, "NULL FOUNDRY",
                    1.16f, 32, 0.73f, 2500, 0.81f, new G10_SpreadBossAttack(true))
            });
    }

    internal sealed class G10_ShooterProgress
    {
        public int Score { get; private set; }
        public int EnemiesDefeated { get; private set; }

        public bool RegisterEnemyDefeated(int pointValue)
        {
            Score += pointValue;
            EnemiesDefeated++;
            return EnemiesDefeated % 4 == 0;
        }

        public void RegisterBossDefeated(int pointValue)
        {
            Score += pointValue;
        }

        public void RegisterPowerCellOverflow(int pointValue)
        {
            Score += pointValue;
        }

        public void ResetStage()
        {
            EnemiesDefeated = 0;
        }
    }

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

        public abstract int PointValue { get; }
        public abstract G10_ShooterEnemyAppearance Appearance { get; }
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

        protected abstract float FireInterval { get; }
        protected abstract G10_SolarShooterProjectile CreateShot(G10_SolarShooterPlayer player);
    }

    internal sealed class G10_Driftling : G10_SolarShooterEnemy
    {
        static readonly G10_ShooterEnemyAppearance _Appearance =
            new G10_ShooterEnemyAppearance(0, 7, 6, false, false, false);

        public G10_Driftling(float x, float y, float direction, float firstShot)
            : base(x, y, direction, firstShot) { }

        public override int PointValue { get { return 75; } }
        public override G10_ShooterEnemyAppearance Appearance { get { return _Appearance; } }
        protected override float FireInterval { get { return 1.9f; } }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 3, Y - 3, 6, 6); }
        }
        public override void Update(float deltaSeconds)
        {
            Phase += deltaSeconds * 2;
            X += Direction * (18 + (float)Math.Sin(Phase) * 22) * deltaSeconds;
            Y += 22 * deltaSeconds;
            X = Math.Max(7, Math.Min(153, X));
        }

        protected override G10_SolarShooterProjectile CreateShot(G10_SolarShooterPlayer player)
        {
            float aim = Math.Sign(player.X - X) * 16;
            return new G10_SolarShooterProjectile(X, Y + 4, aim, 60);
        }
    }

    internal sealed class G10_Needlewing : G10_SolarShooterEnemy
    {
        static readonly G10_ShooterEnemyAppearance _Appearance =
            new G10_ShooterEnemyAppearance(1, 7, 6, false, false, true);

        public G10_Needlewing(float x, float y, float direction, float firstShot)
            : base(x, y, direction, firstShot) { }

        public override int PointValue { get { return 125; } }
        public override G10_ShooterEnemyAppearance Appearance { get { return _Appearance; } }
        protected override float FireInterval { get { return 1.9f; } }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 3, Y - 3, 6, 6); }
        }
        public override void Update(float deltaSeconds)
        {
            Phase += deltaSeconds * 4;
            X += Direction * 47 * deltaSeconds;
            if (X < 8) { X = 8; Direction = 1; }
            else if (X > 152) { X = 152; Direction = -1; }
            Y += 34 * deltaSeconds;
            X = Math.Max(7, Math.Min(153, X));
        }

        protected override G10_SolarShooterProjectile CreateShot(G10_SolarShooterPlayer player)
        {
            float aim = Math.Sign(player.X - X) * 16;
            return new G10_SolarShooterProjectile(X, Y + 4, aim, 60);
        }
    }

    internal sealed class G10_Bulwark : G10_SolarShooterEnemy
    {
        static readonly G10_ShooterEnemyAppearance _Appearance =
            new G10_ShooterEnemyAppearance(2, 10, 8, true, true, false);

        public G10_Bulwark(float x, float y, float direction, float firstShot)
            : base(x, y, direction, firstShot) { }

        public override int PointValue { get { return 175; } }
        public override G10_ShooterEnemyAppearance Appearance { get { return _Appearance; } }
        protected override float FireInterval { get { return 1.35f; } }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 4, Y - 4, 8, 8); }
        }
        public override void Update(float deltaSeconds)
        {
            Phase += deltaSeconds * 2;
            X += (float)Math.Sin(Phase) * 10 * deltaSeconds;
            Y += 14 * deltaSeconds;
            X = Math.Max(7, Math.Min(153, X));
        }

        protected override G10_SolarShooterProjectile CreateShot(G10_SolarShooterPlayer player)
        {
            float aim = Math.Sign(player.X - X) * 25;
            return new G10_SolarShooterProjectile(X, Y + 4, aim, 68);
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

    internal interface IG10_BossAttack
    {
        void Fire(G10_SolarShooterBoss boss, IList<G10_SolarShooterProjectile> projectiles);
    }

    internal sealed class G10_AimedBossAttack : IG10_BossAttack
    {
        public void Fire(G10_SolarShooterBoss boss, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(new G10_SolarShooterProjectile(
                boss.X, boss.Y + 8, boss.AimDirection * 22, 72));
        }
    }

    internal sealed class G10_SpreadBossAttack : IG10_BossAttack
    {
        readonly bool _ExtraDownShot;

        public G10_SpreadBossAttack(bool extraDownShot)
        {
            _ExtraDownShot = extraDownShot;
        }

        public void Fire(G10_SolarShooterBoss boss, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(new G10_SolarShooterProjectile(boss.X - 10, boss.Y + 7, -32, 68));
            projectiles.Add(new G10_SolarShooterProjectile(boss.X, boss.Y + 9, 0, 74));
            projectiles.Add(new G10_SolarShooterProjectile(boss.X + 10, boss.Y + 7, 32, 68));
            if (_ExtraDownShot && boss.AttackNumber % 2 == 0)
                projectiles.Add(new G10_SolarShooterProjectile(boss.X, boss.Y + 8, 0, 76));
        }
    }

    internal sealed class G10_SolarShooterBoss : G10_ShooterEntity
    {
        readonly G10_SolarShooterStage _Stage;
        readonly G10_SolarShooterPlayer _Player;
        float _Phase;
        float _ShotCooldown = 1.1f;

        public G10_SolarShooterBoss(G10_SolarShooterStage stage, G10_SolarShooterPlayer player)
        {
            _Stage = stage;
            _Player = player;
            Health = stage.BossHealth;
            MaxHealth = stage.BossHealth;
            X = G10_ShooterConfig.BossStartX;
            Y = G10_ShooterConfig.BossStartY;
        }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int AttackNumber { get; private set; }
        public float AimDirection { get { return Math.Sign(_Player.X - X); } }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 21, Y - 9, 42, 18); }
        }

        public override void Update(float deltaSeconds)
        {
            _Phase += deltaSeconds * _Stage.BossTempo;
            X = 80 + (float)Math.Sin(_Phase) * _Stage.BossAmplitude;
            Y = 31 + (float)Math.Sin(_Phase * 1.7f) * 4;
        }

        public bool TryFire(float deltaSeconds)
        {
            _ShotCooldown -= deltaSeconds;
            if (_ShotCooldown > 0) return false;
            _ShotCooldown = _Stage.BossShotInterval;
            AttackNumber++;
            return true;
        }

        public void TakeHit() { Health = Math.Max(0, Health - 1); }
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
        public G10_SolarShooterProjectile(float x, float y, float velocityX, float velocityY)
        {
            X = x;
            Y = y;
            VelocityX = velocityX;
            VelocityY = velocityY;
        }

        public float VelocityX { get; private set; }
        public float VelocityY { get; private set; }
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

    internal sealed class G10_SolarShooterPlayer : G10_ShooterEntity
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
            Lives = 3;
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

        public void Fire(bool firePressed, IList<G10_SolarShooterProjectile> projectiles)
        {
            if (!firePressed || _ShotCooldown > 0) return;
            IG10_Weapon weapon = _Weapons[WeaponLevel - 1];
            weapon.Fire(this, projectiles);
            _ShotCooldown = weapon.Cooldown;
        }

        public void UpgradeWeapon() { WeaponLevel = Math.Min(3, WeaponLevel + 1); }
        public void DowngradeWeapon() { WeaponLevel = Math.Max(1, WeaponLevel - 1); }

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

    internal sealed class G10_EntityDestroyedEventArgs : EventArgs
    {
        public G10_EntityDestroyedEventArgs(float x, float y, bool large)
        {
            X = x;
            Y = y;
            Large = large;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public bool Large { get; private set; }
    }

    internal sealed class G10_BossHitEventArgs : EventArgs
    {
        public G10_BossHitEventArgs(float x, float y) { X = x; Y = y; }
        public float X { get; private set; }
        public float Y { get; private set; }
    }

    internal sealed class G10_SolarShooterSession : IG10_ShooterView, IG10_ShooterCollisionContext
    {
        readonly IReadOnlyList<G10_SolarShooterStage> _Stages;
        readonly G10_EnemySpawner _Spawner = new G10_EnemySpawner();
        readonly G10_CollisionResolver _CollisionResolver = new G10_CollisionResolver();
        readonly G10_ShooterProgress _Progress = new G10_ShooterProgress();
        readonly List<G10_SolarShooterEnemy> _Enemies = new List<G10_SolarShooterEnemy>();
        readonly List<G10_SolarShooterProjectile> _PlayerProjectiles = new List<G10_SolarShooterProjectile>();
        readonly List<G10_SolarShooterProjectile> _EnemyProjectiles = new List<G10_SolarShooterProjectile>();
        readonly List<G10_SolarShooterPowerCell> _PowerCells = new List<G10_SolarShooterPowerCell>();
        readonly ReadOnlyCollection<G10_SolarShooterEnemy> _EnemyView;
        readonly ReadOnlyCollection<G10_SolarShooterProjectile> _PlayerProjectileView;
        readonly ReadOnlyCollection<G10_SolarShooterProjectile> _EnemyProjectileView;
        readonly ReadOnlyCollection<G10_SolarShooterPowerCell> _PowerCellView;
        G10_ShooterPhase _Phase = G10_ShooterPhase.Playing;
        float _SpawnCooldown = 0.8f;
        float _BossWarningTimer;
        float _StageTransitionTimer;

        public G10_SolarShooterSession()
            : this(G10_SolarShooterStage.All) { }

        public G10_SolarShooterSession(IReadOnlyList<G10_SolarShooterStage> stages)
        {
            if (stages == null || stages.Count == 0)
                throw new ArgumentException("At least one shooter stage is required.", "stages");
            _Stages = stages;
            _EnemyView = _Enemies.AsReadOnly();
            _PlayerProjectileView = _PlayerProjectiles.AsReadOnly();
            _EnemyProjectileView = _EnemyProjectiles.AsReadOnly();
            _PowerCellView = _PowerCells.AsReadOnly();
            Player = new G10_SolarShooterPlayer(
                G10_ShooterConfig.PlayerStartX,
                G10_ShooterConfig.PlayerStartY);
        }

        public G10_SolarShooterPlayer Player { get; private set; }
        public IReadOnlyList<G10_SolarShooterEnemy> Enemies { get { return _EnemyView; } }
        public IReadOnlyList<G10_SolarShooterProjectile> PlayerProjectiles { get { return _PlayerProjectileView; } }
        public IReadOnlyList<G10_SolarShooterProjectile> EnemyProjectiles { get { return _EnemyProjectileView; } }
        public IReadOnlyList<G10_SolarShooterPowerCell> PowerCells { get { return _PowerCellView; } }
        IG10_Collidable IG10_ShooterCollisionContext.PlayerCollider { get { return Player; } }
        IG10_Collidable IG10_ShooterCollisionContext.BossCollider { get { return Boss; } }
        public G10_SolarShooterBoss Boss { get; private set; }
        public G10_SolarShooterStage Stage { get { return _Stages[StageIndex]; } }
        public int StageIndex { get; private set; }
        public int StageNumber { get { return StageIndex + 1; } }
        public int Score { get { return _Progress.Score; } }
        public int TickNumber { get; private set; }
        public int DefeatedEnemies { get { return _Progress.EnemiesDefeated; } }
        public G10_ShooterPhase Phase { get { return _Phase; } }
        public bool IsGameOver { get { return _Phase == G10_ShooterPhase.GameOver; } }
        public bool IsVictory { get { return _Phase == G10_ShooterPhase.Victory; } }
        public bool IsBossWarning { get { return _Phase == G10_ShooterPhase.BossWarning; } }
        public bool IsStageTransition { get { return _Phase == G10_ShooterPhase.StageTransition; } }

        public event EventHandler<G10_EntityDestroyedEventArgs> EnemyDestroyed;
        public event EventHandler<G10_EntityDestroyedEventArgs> PlayerHit;
        public event EventHandler<G10_BossHitEventArgs> BossHit;
        public event EventHandler<G10_EntityDestroyedEventArgs> BossDefeated;

        void IG10_ShooterCollisionContext.RemovePlayerProjectile(G10_SolarShooterProjectile projectile)
        {
            _PlayerProjectiles.Remove(projectile);
        }

        void IG10_ShooterCollisionContext.DestroyEnemy(G10_SolarShooterEnemy enemy)
        {
            if (_Enemies.Remove(enemy)) RegisterEnemyDestroyed(enemy);
        }

        void IG10_ShooterCollisionContext.RemoveEnemy(G10_SolarShooterEnemy enemy)
        {
            _Enemies.Remove(enemy);
        }

        void IG10_ShooterCollisionContext.RemoveEnemyProjectile(G10_SolarShooterProjectile projectile)
        {
            _EnemyProjectiles.Remove(projectile);
        }

        void IG10_ShooterCollisionContext.HitPlayer()
        {
            if (Player.TakeHit()) OnPlayerHit();
        }

        void IG10_ShooterCollisionContext.HitBoss() { HitBoss(); }

        void IG10_ShooterCollisionContext.CollectPowerCell(G10_SolarShooterPowerCell powerCell)
        {
            if (!_PowerCells.Remove(powerCell)) return;
            if (Player.WeaponLevel < 3) Player.UpgradeWeapon();
            else _Progress.RegisterPowerCellOverflow(250);
        }

        public void Update(float deltaSeconds, G10_SolarShooterInput input)
        {
            if (IsGameOver || IsVictory) return;
            TickNumber++;
            Player.Update(deltaSeconds);
            Player.Move(input.Horizontal, input.Vertical, deltaSeconds);

            if (IsStageTransition)
            {
                _StageTransitionTimer -= deltaSeconds;
                if (_StageTransitionTimer <= 0) AdvanceStage();
                return;
            }

            if (IsBossWarning)
            {
                _BossWarningTimer -= deltaSeconds;
                if (_BossWarningTimer <= 0) _Phase = G10_ShooterPhase.Playing;
                return;
            }

            Player.Fire(input.FirePressed, _PlayerProjectiles);
            if (Boss == null) UpdateEnemies(deltaSeconds);
            else UpdateBoss(deltaSeconds);
            UpdateProjectiles(_PlayerProjectiles, deltaSeconds);
            UpdateProjectiles(_EnemyProjectiles, deltaSeconds);
            UpdatePowerCells(deltaSeconds);
            _CollisionResolver.ResolvePlayerShots(this);
            _CollisionResolver.ResolveEnemyHits(this);
            _CollisionResolver.ResolvePowerCells(this);
            if (Player.Lives <= 0) _Phase = G10_ShooterPhase.GameOver;
        }

        void UpdateEnemies(float deltaSeconds)
        {
            if (_Progress.EnemiesDefeated < Stage.EnemiesToDefeat)
            {
                _SpawnCooldown -= deltaSeconds;
                if (_SpawnCooldown <= 0)
                {
                    _Spawner.Spawn(_Enemies);
                    _SpawnCooldown = Stage.SpawnInterval;
                }
            }

            for (int i = _Enemies.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterEnemy enemy = _Enemies[i];
                G10_SolarShooterProjectile shot = enemy.UpdateAndTryFire(deltaSeconds, Player);
                if (shot != null) _EnemyProjectiles.Add(shot);
                if (enemy.Y > G10_ShooterConfig.WorldHeight + 8)
                    _Enemies.RemoveAt(i);
            }

            if (_Progress.EnemiesDefeated >= Stage.EnemiesToDefeat && _Enemies.Count == 0)
                BeginBossEncounter();
        }

        void BeginBossEncounter()
        {
            Boss = new G10_SolarShooterBoss(Stage, Player);
            _BossWarningTimer = 1.5f;
            _Phase = G10_ShooterPhase.BossWarning;
            _PlayerProjectiles.Clear();
            _EnemyProjectiles.Clear();
        }

        void UpdateBoss(float deltaSeconds)
        {
            Boss.Update(deltaSeconds);
            if (Boss.TryFire(deltaSeconds))
                Stage.BossAttack.Fire(Boss, _EnemyProjectiles);
        }

        static void UpdateProjectiles(IList<G10_SolarShooterProjectile> projectiles, float deltaSeconds)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterProjectile projectile = projectiles[i];
                projectile.Update(deltaSeconds);
                if (projectile.Y < G10_ShooterConfig.PlayfieldTop - 8 ||
                    projectile.Y > G10_ShooterConfig.WorldHeight + 8 ||
                    projectile.X < -8 ||
                    projectile.X > G10_ShooterConfig.WorldWidth + 8)
                    projectiles.RemoveAt(i);
            }
        }

        void UpdatePowerCells(float deltaSeconds)
        {
            for (int i = _PowerCells.Count - 1; i >= 0; i--)
            {
                _PowerCells[i].Update(deltaSeconds);
                if (_PowerCells[i].Y > G10_ShooterConfig.WorldHeight + 6)
                    _PowerCells.RemoveAt(i);
            }
        }

        void RegisterEnemyDestroyed(G10_SolarShooterEnemy enemy)
        {
            if (_Progress.RegisterEnemyDefeated(enemy.PointValue))
                _PowerCells.Add(new G10_SolarShooterPowerCell(enemy.X, enemy.Y));
            EventHandler<G10_EntityDestroyedEventArgs> handler = EnemyDestroyed;
            if (handler != null) handler(this, new G10_EntityDestroyedEventArgs(enemy.X, enemy.Y, false));
        }

        internal void HitBoss()
        {
            Boss.TakeHit();
            EventHandler<G10_BossHitEventArgs> hitHandler = BossHit;
            if (hitHandler != null) hitHandler(this, new G10_BossHitEventArgs(Boss.X, Boss.Y));
            if (Boss.Health <= 0) DefeatBoss();
        }

        internal void OnPlayerHit()
        {
            EventHandler<G10_EntityDestroyedEventArgs> handler = PlayerHit;
            if (handler != null) handler(this, new G10_EntityDestroyedEventArgs(Player.X, Player.Y, true));
        }

        void DefeatBoss()
        {
            _Progress.RegisterBossDefeated(Stage.BossScore);
            float bossX = Boss.X;
            float bossY = Boss.Y;
            Boss = null;
            _EnemyProjectiles.Clear();
            _StageTransitionTimer = 1.8f;
            _Phase = G10_ShooterPhase.StageTransition;
            EventHandler<G10_EntityDestroyedEventArgs> handler = BossDefeated;
            if (handler != null) handler(this, new G10_EntityDestroyedEventArgs(bossX, bossY, true));
        }

        void AdvanceStage()
        {
            if (StageIndex == _Stages.Count - 1)
            {
                _Phase = G10_ShooterPhase.Victory;
                return;
            }

            StageIndex++;
            _Progress.ResetStage();
            _SpawnCooldown = 1;
            _Spawner.Reset();
            _Enemies.Clear();
            _PlayerProjectiles.Clear();
            _EnemyProjectiles.Clear();
            _PowerCells.Clear();
            Player.ResetPosition();
            _Phase = G10_ShooterPhase.Playing;
        }
    }
}
