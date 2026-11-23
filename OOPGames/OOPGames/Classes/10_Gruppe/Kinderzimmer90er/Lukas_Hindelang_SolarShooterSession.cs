using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OOPGames
{
    internal enum G10_ShooterPhase
    {
        Playing,
        BossWarning,
        StageTransition,
        GameOver,
        Victory
    }

    // Read-only view for the renderer, the effects and the missions.
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
        float ShieldSeconds { get; }
        bool IsGameOver { get; }
        bool IsVictory { get; }
        bool IsBossWarning { get; }
        bool IsStageTransition { get; }
        event EventHandler<G10_EntityDestroyedEventArgs> EnemyDestroyed;
        event EventHandler<G10_EntityDestroyedEventArgs> PlayerHit;
        event EventHandler<G10_ShotFiredEventArgs> ShotFired;
        event EventHandler<EventArgs> PowerCellCollected;
        event EventHandler<EventArgs> PowerCellEscaped;
        event EventHandler<G10_BossHitEventArgs> BossHit;
        event EventHandler<G10_EntityDestroyedEventArgs> BossDefeated;
    }

    // Owns one run: entities, stage flow, score. It knows nothing about input keys,
    // drawing, missions or saving; those react to its events or read its view.
    internal sealed class G10_SolarShooterSession : IG10_ShooterView, IG10_ShooterCollisionContext
    {
        readonly IReadOnlyList<G10_SolarShooterStage> _Stages;
        readonly G10_EnemySpawner _Spawner = new G10_EnemySpawner();
        readonly G10_CollisionResolver _CollisionResolver = new G10_CollisionResolver();
        readonly G10_ShooterProgress _Progress;
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
        float _ShieldSeconds;

        public G10_SolarShooterSession()
            : this(G10_SolarShooterStage.All) { }

        public G10_SolarShooterSession(IReadOnlyList<G10_SolarShooterStage> stages)
            : this(stages, 0, 0, 1) { }

        public G10_SolarShooterSession(
            IReadOnlyList<G10_SolarShooterStage> stages,
            int startingStageIndex,
            int startingScore,
            int startingWeaponLevel)
        {
            if (stages == null || stages.Count == 0)
                throw new ArgumentException("At least one shooter stage is required.", "stages");
            _Stages = stages;
            StageIndex = Math.Max(0, Math.Min(stages.Count - 1, startingStageIndex));
            _Progress = new G10_ShooterProgress(startingScore);
            _EnemyView = _Enemies.AsReadOnly();
            _PlayerProjectileView = _PlayerProjectiles.AsReadOnly();
            _EnemyProjectileView = _EnemyProjectiles.AsReadOnly();
            _PowerCellView = _PowerCells.AsReadOnly();
            Player = new G10_SolarShooterPlayer(
                G10_ShooterConfig.PlayerStartX,
                G10_ShooterConfig.PlayerStartY);
            Player.RestoreWeaponLevel(startingWeaponLevel);
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
        public float ShieldSeconds { get { return _ShieldSeconds; } }

        public event EventHandler<G10_EntityDestroyedEventArgs> EnemyDestroyed;
        public event EventHandler<G10_EntityDestroyedEventArgs> PlayerHit;
        public event EventHandler<G10_ShotFiredEventArgs> ShotFired;
        public event EventHandler<EventArgs> PowerCellCollected;
        public event EventHandler<EventArgs> PowerCellEscaped;
        public event EventHandler<G10_BossHitEventArgs> BossHit;
        public event EventHandler<G10_EntityDestroyedEventArgs> BossDefeated;

        void IG10_ShooterCollisionContext.RemovePlayerProjectile(G10_SolarShooterProjectile projectile)
        {
            _PlayerProjectiles.Remove(projectile);
        }

        void IG10_ShooterCollisionContext.DestroyEnemy(G10_SolarShooterEnemy enemy, int weaponLevel)
        {
            if (_Enemies.Remove(enemy)) RegisterEnemyDestroyed(enemy, weaponLevel);
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
            if (_ShieldSeconds > 0) return;
            if (Player.TakeHit()) OnPlayerHit();
        }

        void IG10_ShooterCollisionContext.HitBoss() { HitBoss(); }

        void IG10_ShooterCollisionContext.CollectPowerCell(G10_SolarShooterPowerCell powerCell)
        {
            if (!_PowerCells.Remove(powerCell)) return;
            if (Player.WeaponLevel < 3) Player.UpgradeWeapon();
            else _Progress.RegisterPowerCellOverflow(250);
            EventHandler<EventArgs> handler = PowerCellCollected;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void Update(float deltaSeconds, G10_SolarShooterInput input)
        {
            if (IsGameOver || IsVictory) return;
            TickNumber++;
            Player.Update(deltaSeconds);
            _ShieldSeconds = Math.Max(0, _ShieldSeconds - deltaSeconds);
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

            if (Player.Fire(input.FirePressed, _PlayerProjectiles))
            {
                EventHandler<G10_ShotFiredEventArgs> handler = ShotFired;
                if (handler != null) handler(this, new G10_ShotFiredEventArgs(Player.WeaponLevel));
            }
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
                {
                    _PowerCells.RemoveAt(i);
                    EventHandler<EventArgs> handler = PowerCellEscaped;
                    if (handler != null) handler(this, EventArgs.Empty);
                }
            }
        }

        void RegisterEnemyDestroyed(G10_SolarShooterEnemy enemy, int weaponLevel)
        {
            if (_Progress.RegisterEnemyDefeated(enemy.PointValue))
                _PowerCells.Add(new G10_SolarShooterPowerCell(enemy.X, enemy.Y));
            EventHandler<G10_EntityDestroyedEventArgs> handler = EnemyDestroyed;
            if (handler != null)
                handler(this, new G10_EntityDestroyedEventArgs(
                    enemy.X, enemy.Y, false, enemy.Kind,
                    weaponLevel, enemy.PointValue));
        }

        public void GrantExtraLife() { Player.AddLife(); }
        public void AddScore(int points) { _Progress.AddScore(points); }
        public void ActivateShield(float seconds) { _ShieldSeconds = Math.Max(_ShieldSeconds, seconds); }
        public void ApplyScorePenalty(int points) { _Progress.ApplyPenalty(points); }

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
