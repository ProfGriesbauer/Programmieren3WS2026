using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal enum G10_ShooterBackdrop
    {
        ShardField,
        SporeGrove,
        Ironworks
    }

    internal sealed class G10_ShooterStage
    {
        public static readonly G10_ShooterStage[] All =
        {
            new G10_ShooterStage("RIFT", G10_ShooterBackdrop.ShardField, 10, 15, "THE DRIFTER"),
            new G10_ShooterStage("CANOPY", G10_ShooterBackdrop.SporeGrove, 12, 19, "ROOT ENGINE"),
            new G10_ShooterStage("IRONWORKS", G10_ShooterBackdrop.Ironworks, 14, 24, "NULL FOUNDRY")
        };

        G10_ShooterStage(string name, G10_ShooterBackdrop backdrop, int enemiesToDefeat, int bossHealth, string bossName)
        {
            Name = name;
            Backdrop = backdrop;
            EnemiesToDefeat = enemiesToDefeat;
            BossHealth = bossHealth;
            BossName = bossName;
        }

        public string Name { get; private set; }
        public G10_ShooterBackdrop Backdrop { get; private set; }
        public int EnemiesToDefeat { get; private set; }
        public int BossHealth { get; private set; }
        public string BossName { get; private set; }
    }

    internal enum G10_ShooterEnemyKind
    {
        Driftling,
        Needlewing,
        Bulwark
    }

    internal sealed class G10_SolarShooterSession
    {
        const float WorldWidth = 160;
        const float WorldHeight = 144;
        const float PlayfieldTop = 18;

        readonly List<G10_SolarShooterEnemy> _Enemies = new List<G10_SolarShooterEnemy>();
        readonly List<G10_SolarShooterProjectile> _PlayerProjectiles = new List<G10_SolarShooterProjectile>();
        readonly List<G10_SolarShooterProjectile> _EnemyProjectiles = new List<G10_SolarShooterProjectile>();
        readonly List<G10_SolarShooterPowerCell> _PowerCells = new List<G10_SolarShooterPowerCell>();
        float _SpawnCooldown = 0.8f;
        float _ShotCooldown;
        float _BossWarningTimer;
        float _StageTransitionTimer;
        int _SpawnCount;

        public G10_SolarShooterSession()
        {
            Player = new G10_SolarShooterPlayer(WorldWidth / 2, 124);
        }

        public G10_SolarShooterPlayer Player { get; private set; }
        public IReadOnlyList<G10_SolarShooterEnemy> Enemies { get { return _Enemies; } }
        public IReadOnlyList<G10_SolarShooterProjectile> PlayerProjectiles { get { return _PlayerProjectiles; } }
        public IReadOnlyList<G10_SolarShooterProjectile> EnemyProjectiles { get { return _EnemyProjectiles; } }
        public IReadOnlyList<G10_SolarShooterPowerCell> PowerCells { get { return _PowerCells; } }
        public G10_SolarShooterBoss Boss { get; private set; }
        public G10_ShooterStage Stage { get { return G10_ShooterStage.All[StageIndex]; } }
        public int StageIndex { get; private set; }
        public int StageNumber { get { return StageIndex + 1; } }
        public int Score { get; private set; }
        public int TickNumber { get; private set; }
        public int DefeatedEnemies { get; private set; }
        public bool IsGameOver { get { return Player.Lives <= 0; } }
        public bool IsVictory { get; private set; }
        public bool IsBossWarning { get { return _BossWarningTimer > 0; } }
        public bool IsStageTransition { get { return _StageTransitionTimer > 0; } }

        public void Update(float deltaSeconds, G10_SolarShooterInput input)
        {
            if (IsGameOver || IsVictory) return;

            TickNumber++;
            Player.UpdateInvulnerability(deltaSeconds);
            Player.Move(input.Horizontal, input.Vertical, deltaSeconds);

            if (_StageTransitionTimer > 0)
            {
                _StageTransitionTimer -= deltaSeconds;
                if (_StageTransitionTimer <= 0) AdvanceStage();
                return;
            }

            if (_BossWarningTimer > 0)
            {
                _BossWarningTimer -= deltaSeconds;
                return;
            }

            FirePlayerWeapon(input.FirePressed, deltaSeconds);
            if (Boss == null) UpdateEnemies(deltaSeconds);
            else UpdateBoss(deltaSeconds);

            UpdateProjectiles(_PlayerProjectiles, deltaSeconds);
            UpdateProjectiles(_EnemyProjectiles, deltaSeconds);
            UpdatePowerCells(deltaSeconds);
            ResolvePlayerShots();
            ResolveEnemyHits();
            ResolvePowerCellPickups();
        }

        void FirePlayerWeapon(bool firePressed, float deltaSeconds)
        {
            _ShotCooldown = Math.Max(0, _ShotCooldown - deltaSeconds);
            if (!firePressed || _ShotCooldown > 0) return;

            switch (Player.WeaponLevel)
            {
                case 1:
                    AddPlayerShot(0);
                    break;
                case 2:
                    AddPlayerShot(-3);
                    AddPlayerShot(3);
                    break;
                default:
                    AddPlayerShot(-5);
                    AddPlayerShot(0);
                    AddPlayerShot(5);
                    break;
            }

            _ShotCooldown = Player.WeaponLevel == 3 ? 0.3f : 0.24f;
        }

        void AddPlayerShot(float horizontalOffset)
        {
            _PlayerProjectiles.Add(new G10_SolarShooterProjectile(
                Player.X + horizontalOffset, Player.Y - 7, horizontalOffset * 1.5f, -150));
        }

        void UpdateEnemies(float deltaSeconds)
        {
            if (DefeatedEnemies < Stage.EnemiesToDefeat)
            {
                _SpawnCooldown -= deltaSeconds;
                if (_SpawnCooldown <= 0)
                {
                    SpawnEnemy();
                    _SpawnCooldown = Math.Max(0.55f, 1.05f - StageIndex * 0.12f);
                }
            }

            for (int i = _Enemies.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterEnemy enemy = _Enemies[i];
                enemy.Update(deltaSeconds);
                if (enemy.TryFire(deltaSeconds))
                {
                    float aim = Math.Sign(Player.X - enemy.X) * enemy.ShotDrift;
                    _EnemyProjectiles.Add(new G10_SolarShooterProjectile(
                        enemy.X, enemy.Y + 4, aim, enemy.ShotSpeed));
                }

                if (enemy.Y > WorldHeight + 8) _Enemies.RemoveAt(i);
            }

            if (DefeatedEnemies >= Stage.EnemiesToDefeat && _Enemies.Count == 0)
                BeginBossEncounter();
        }

        void SpawnEnemy()
        {
            if (_Enemies.Count >= 5) return;

            G10_ShooterEnemyKind kind = (G10_ShooterEnemyKind)(_SpawnCount % 3);
            float x = 14 + (_SpawnCount * 43 % 132);
            float direction = _SpawnCount % 2 == 0 ? 1 : -1;
            float firstShot = 1.1f + (_SpawnCount % 3) * 0.25f;
            _Enemies.Add(new G10_SolarShooterEnemy(kind, x, PlayfieldTop, direction, firstShot));
            _SpawnCount++;
        }

        void BeginBossEncounter()
        {
            Boss = new G10_SolarShooterBoss(StageIndex, Stage.BossHealth);
            _BossWarningTimer = 1.5f;
            _PlayerProjectiles.Clear();
            _EnemyProjectiles.Clear();
        }

        void UpdateBoss(float deltaSeconds)
        {
            Boss.Update(deltaSeconds);
            if (!Boss.TryFire(deltaSeconds)) return;

            if (StageIndex == 0)
            {
                FireAimedBossShot();
            }
            else if (StageIndex == 1)
            {
                FireBossSpread();
            }
            else
            {
                FireBossSpread();
                if (Boss.AttackNumber % 2 == 0)
                    _EnemyProjectiles.Add(new G10_SolarShooterProjectile(Boss.X, Boss.Y + 8, 0, 76));
            }
        }

        void FireAimedBossShot()
        {
            float direction = Math.Sign(Player.X - Boss.X);
            _EnemyProjectiles.Add(new G10_SolarShooterProjectile(Boss.X, Boss.Y + 8, direction * 22, 72));
        }

        void FireBossSpread()
        {
            _EnemyProjectiles.Add(new G10_SolarShooterProjectile(Boss.X - 10, Boss.Y + 7, -32, 68));
            _EnemyProjectiles.Add(new G10_SolarShooterProjectile(Boss.X, Boss.Y + 9, 0, 74));
            _EnemyProjectiles.Add(new G10_SolarShooterProjectile(Boss.X + 10, Boss.Y + 7, 32, 68));
        }

        void UpdatePowerCells(float deltaSeconds)
        {
            for (int i = _PowerCells.Count - 1; i >= 0; i--)
            {
                _PowerCells[i].Update(deltaSeconds);
                if (_PowerCells[i].Y > WorldHeight + 6) _PowerCells.RemoveAt(i);
            }
        }

        static void UpdateProjectiles(List<G10_SolarShooterProjectile> projectiles, float deltaSeconds)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterProjectile projectile = projectiles[i];
                projectile.Update(deltaSeconds);
                if (projectile.Y < PlayfieldTop - 8 || projectile.Y > WorldHeight + 8 ||
                    projectile.X < -8 || projectile.X > WorldWidth + 8)
                    projectiles.RemoveAt(i);
            }
        }

        void ResolvePlayerShots()
        {
            for (int projectileIndex = _PlayerProjectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                G10_SolarShooterProjectile projectile = _PlayerProjectiles[projectileIndex];
                bool consumed = false;

                if (Boss != null && Boss.Bounds.Intersects(projectile.Bounds))
                {
                    _PlayerProjectiles.RemoveAt(projectileIndex);
                    Boss.TakeHit();
                    if (Boss.Health <= 0) DefeatBoss();
                    continue;
                }

                for (int enemyIndex = _Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
                {
                    G10_SolarShooterEnemy enemy = _Enemies[enemyIndex];
                    if (!projectile.Bounds.Intersects(enemy.Bounds)) continue;

                    _PlayerProjectiles.RemoveAt(projectileIndex);
                    _Enemies.RemoveAt(enemyIndex);
                    Score += enemy.PointValue;
                    DefeatedEnemies++;
                    if (DefeatedEnemies % 4 == 0)
                        _PowerCells.Add(new G10_SolarShooterPowerCell(enemy.X, enemy.Y));
                    consumed = true;
                    break;
                }

                if (consumed) continue;
            }
        }

        void DefeatBoss()
        {
            Score += 1500 + StageIndex * 500;
            Boss = null;
            _EnemyProjectiles.Clear();
            _StageTransitionTimer = 1.8f;
        }

        void AdvanceStage()
        {
            if (StageIndex == G10_ShooterStage.All.Length - 1)
            {
                IsVictory = true;
                return;
            }

            StageIndex++;
            DefeatedEnemies = 0;
            _SpawnCooldown = 1;
            _Enemies.Clear();
            _PlayerProjectiles.Clear();
            _EnemyProjectiles.Clear();
            _PowerCells.Clear();
            Player.ResetPosition();
        }

        void ResolveEnemyHits()
        {
            for (int i = _EnemyProjectiles.Count - 1; i >= 0; i--)
            {
                if (_EnemyProjectiles[i].Bounds.Intersects(Player.Bounds))
                {
                    _EnemyProjectiles.RemoveAt(i);
                    Player.TakeHit();
                }
            }

            for (int i = _Enemies.Count - 1; i >= 0; i--)
            {
                if (Player.Bounds.Intersects(_Enemies[i].Bounds))
                {
                    _Enemies.RemoveAt(i);
                    Player.TakeHit();
                }
            }
        }

        void ResolvePowerCellPickups()
        {
            for (int i = _PowerCells.Count - 1; i >= 0; i--)
            {
                if (!Player.Bounds.Intersects(_PowerCells[i].Bounds)) continue;

                _PowerCells.RemoveAt(i);
                if (Player.WeaponLevel < 3) Player.UpgradeWeapon();
                else Score += 250;
            }
        }
    }

    internal struct G10_SolarShooterInput
    {
        public float Horizontal;
        public float Vertical;
        public bool FirePressed;
    }

    internal sealed class G10_SolarShooterPlayer
    {
        const float Speed = 92;
        float _InvulnerabilitySeconds;

        public G10_SolarShooterPlayer(float x, float y)
        {
            X = x;
            Y = y;
            Lives = 3;
            WeaponLevel = 1;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public int Lives { get; private set; }
        public int WeaponLevel { get; private set; }
        public bool IsInvulnerable { get { return _InvulnerabilitySeconds > 0; } }
        public G10_SolarShooterBounds Bounds { get { return new G10_SolarShooterBounds(X - 4, Y - 4, 8, 8); } }

        public void Move(float horizontal, float vertical, float deltaSeconds)
        {
            float length = (float)Math.Sqrt(horizontal * horizontal + vertical * vertical);
            if (length > 1)
            {
                horizontal /= length;
                vertical /= length;
            }

            X = Clamp(X + horizontal * Speed * deltaSeconds, 7, 153);
            Y = Clamp(Y + vertical * Speed * deltaSeconds, 28, 135);
        }

        public void UpdateInvulnerability(float deltaSeconds)
        {
            _InvulnerabilitySeconds = Math.Max(0, _InvulnerabilitySeconds - deltaSeconds);
        }

        public void UpgradeWeapon()
        {
            WeaponLevel = Math.Min(3, WeaponLevel + 1);
        }

        public void TakeHit()
        {
            if (IsInvulnerable) return;

            Lives--;
            WeaponLevel = Math.Max(1, WeaponLevel - 1);
            ResetPosition();
            _InvulnerabilitySeconds = 1.4f;
        }

        public void ResetPosition()
        {
            X = 80;
            Y = 124;
        }

        static float Clamp(float value, float minimum, float maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }

    internal sealed class G10_SolarShooterEnemy
    {
        float _Direction;
        float _Phase;
        float _ShotCooldown;

        public G10_SolarShooterEnemy(G10_ShooterEnemyKind kind, float x, float y, float direction, float firstShot)
        {
            Kind = kind;
            X = x;
            Y = y;
            _Direction = direction;
            _ShotCooldown = firstShot;
        }

        public G10_ShooterEnemyKind Kind { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public int PointValue { get { return Kind == G10_ShooterEnemyKind.Bulwark ? 175 : Kind == G10_ShooterEnemyKind.Needlewing ? 125 : 75; } }
        public float ShotDrift { get { return Kind == G10_ShooterEnemyKind.Bulwark ? 25 : 16; } }
        public float ShotSpeed { get { return Kind == G10_ShooterEnemyKind.Bulwark ? 68 : 60; } }
        public G10_SolarShooterBounds Bounds
        {
            get
            {
                float size = Kind == G10_ShooterEnemyKind.Bulwark ? 8 : 6;
                return new G10_SolarShooterBounds(X - size / 2, Y - size / 2, size, size);
            }
        }

        public void Update(float deltaSeconds)
        {
            _Phase += deltaSeconds * (Kind == G10_ShooterEnemyKind.Needlewing ? 4 : 2);
            if (Kind == G10_ShooterEnemyKind.Driftling)
            {
                X += _Direction * (18 + (float)Math.Sin(_Phase) * 22) * deltaSeconds;
                Y += 22 * deltaSeconds;
            }
            else if (Kind == G10_ShooterEnemyKind.Needlewing)
            {
                X += _Direction * 47 * deltaSeconds;
                if (X < 8)
                {
                    X = 8;
                    _Direction = 1;
                }
                else if (X > 152)
                {
                    X = 152;
                    _Direction = -1;
                }
                Y += 34 * deltaSeconds;
            }
            else
            {
                X += (float)Math.Sin(_Phase) * 10 * deltaSeconds;
                Y += 14 * deltaSeconds;
            }

            X = Math.Max(7, Math.Min(153, X));
        }

        public bool TryFire(float deltaSeconds)
        {
            _ShotCooldown -= deltaSeconds;
            if (_ShotCooldown > 0) return false;
            _ShotCooldown = Kind == G10_ShooterEnemyKind.Bulwark ? 1.35f : 1.9f;
            return true;
        }
    }

    internal sealed class G10_SolarShooterBoss
    {
        readonly int _StageIndex;
        float _Phase;
        float _ShotCooldown = 1.1f;

        public G10_SolarShooterBoss(int stageIndex, int health)
        {
            _StageIndex = stageIndex;
            Health = health;
            MaxHealth = health;
            X = 80;
            Y = 35;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int AttackNumber { get; private set; }
        public G10_SolarShooterBounds Bounds { get { return new G10_SolarShooterBounds(X - 21, Y - 9, 42, 18); } }

        public void Update(float deltaSeconds)
        {
            _Phase += deltaSeconds * (0.8f + _StageIndex * 0.18f);
            X = 80 + (float)Math.Sin(_Phase) * (38 - _StageIndex * 3);
            Y = 31 + (float)Math.Sin(_Phase * 1.7f) * 4;
        }

        public bool TryFire(float deltaSeconds)
        {
            _ShotCooldown -= deltaSeconds;
            if (_ShotCooldown > 0) return false;
            _ShotCooldown = Math.Max(0.62f, 1.05f - _StageIndex * 0.16f);
            AttackNumber++;
            return true;
        }

        public void TakeHit()
        {
            Health = Math.Max(0, Health - 1);
        }
    }

    internal sealed class G10_SolarShooterPowerCell
    {
        public G10_SolarShooterPowerCell(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public G10_SolarShooterBounds Bounds { get { return new G10_SolarShooterBounds(X - 3, Y - 3, 6, 6); } }

        public void Update(float deltaSeconds)
        {
            Y += 20 * deltaSeconds;
        }
    }

    internal sealed class G10_SolarShooterProjectile
    {
        public G10_SolarShooterProjectile(float x, float y, float velocityX, float velocityY)
        {
            X = x;
            Y = y;
            VelocityX = velocityX;
            VelocityY = velocityY;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float VelocityX { get; private set; }
        public float VelocityY { get; private set; }
        public G10_SolarShooterBounds Bounds { get { return new G10_SolarShooterBounds(X - 1, Y - 2, 2, 4); } }

        public void Update(float deltaSeconds)
        {
            X += VelocityX * deltaSeconds;
            Y += VelocityY * deltaSeconds;
        }
    }

    internal struct G10_SolarShooterBounds
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
}
