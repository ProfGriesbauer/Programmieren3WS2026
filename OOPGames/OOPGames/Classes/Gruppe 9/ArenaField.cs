//P06.11.2026
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace OOPGames
{
    // Holds world dimensions, the player, and movement/collision behavior.
    public sealed class ArenaField : IGameField
    {
        // All world positions and sizes are measured in WPF device-independent units.
        public const double MapWidth = 1800;
        public const double MapHeight = 1200;
        public const double WallThickness = 32;
        public const double MinimumZoom = 0.5;
        public const double MaximumZoom = 2.0;
        private const double ZoomStep = 0.1;
        public const int MaximumEnemyCount = 15;
        public const double EnemySpawnIntervalSeconds = 7;
        private const double GameTickDurationSeconds = 0.04;
        public const double PlayerAttackRange = 80;
        public const double PlayerAttackHalfAngleRadians = Math.PI / 6;
        public const int PlayerAttackDamage = 10;
        public const double WeaponWheelSlowMotionScale = 0.15;
        private const double EnemySpawnVisibilityMargin = 50;
        private const int EnemySpawnPositionAttemptLimit = 10000;
        private readonly List<PursuerEnemy> _enemies = new List<PursuerEnemy>();
        private double _enemySpawnElapsedSeconds;
        private readonly ArenaCombatSystem _combatSystem = new ArenaCombatSystem();
        private Weapon _selectedPlayerWeapon = WeaponCatalog.Sword;
        private readonly ArenaProgression _progression = new ArenaProgression();
        private readonly ArenaKeyBindings _keyBindings = ArenaKeyBindings.LoadDefault();

        // The field owns the current player instance.
        public PlayerCharacter Player { get; private set; }
        public IReadOnlyList<PursuerEnemy> Enemies { get { return _enemies; } }
        public Weapon SelectedPlayerWeapon
        {
            get { return Player == null ? _selectedPlayerWeapon : Player.EquippedWeapon; }
        }
        public ArenaProgression Progression { get { return _progression; } }
        public ArenaKeyBindings KeyBindings { get { return _keyBindings; } }
        public int ActiveAttackCount { get { return _combatSystem.ActiveAttackCount; } }
        public IReadOnlyList<AttackVisualState> ActiveAttackVisuals
        {
            get { return _combatSystem.GetActiveVisuals(); }
        }
        public bool IsGameStarted { get; private set; }
        public bool IsWeaponWheelOpen { get; private set; }
        public int WeaponWheelSelectionIndex
        {
            get
            {
                return ArenaWeaponWheel.GetSelectionIndex(
                    AimDirectionX, AimDirectionY);
            }
        }
        public double GameTimeScale
        {
            get
            {
                return IsWeaponWheelOpen ? WeaponWheelSlowMotionScale : 1;
            }
        }
        public bool HasActiveGame { get; private set; }
        public bool IsGameOver { get { return HasActiveGame && !Player.IsAlive; } }
        public bool IsGodMode { get { return _progression.IsGodMode; } }
        public bool ShowDamageRanges { get; private set; }
        public bool ShowDebugHud { get; private set; }
        public int Score { get; private set; }
        public double AimDirectionX { get; private set; }
        public double AimDirectionY { get; private set; } = 1;
        public double ZoomLevel { get; private set; } = 1.0;
        public static Rect MovementBounds
        {
            get
            {
                return new Rect(WallThickness, WallThickness,
                    MapWidth - 2 * WallThickness, MapHeight - 2 * WallThickness);
            }
        }

        public ArenaField()
        {
            _combatSystem.EntityDefeated += OnEntityDefeated;
            // Create the player in the middle when a new field is constructed.
            ResetPlayer();
        }

        public void ResetPlayer()
        {
            // Resetting starts the player at the center of the arena.
            _combatSystem.Clear();
            Player = new PlayerCharacter(MapWidth / 2, MapHeight / 2);
            Player.EquippedWeapon = _selectedPlayerWeapon;
            Player.IsInvulnerable = IsGodMode;
            _enemies.Clear();
            _enemySpawnElapsedSeconds = 0;
            Point spawnPosition = GetRandomEnemySpawnPosition(Player);
            _enemies.Add(new PursuerEnemy(
                spawnPosition.X, spawnPosition.Y, Player.MaxSpeed));
        }

        public void SelectPlayerWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            if (weapon.Id == WeaponCatalog.Sword.Id)
            {
                _selectedPlayerWeapon = WeaponCatalog.Sword;
            }
            else if (weapon.Id == WeaponCatalog.MagicWand.Id)
            {
                _selectedPlayerWeapon = WeaponCatalog.MagicWand;
            }
            else if (weapon.Id == WeaponCatalog.Bow.Id)
            {
                _selectedPlayerWeapon = WeaponCatalog.Bow;
            }
            else
            {
                throw new ArgumentException(
                    "The selected weapon is not available as player equipment.",
                    nameof(weapon));
            }

            RefreshPlayerWeapon();
        }

        public bool TryUpgradeWeapon(string upgradeId, UpgradeCurrency currency)
        {
            bool upgraded;
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                upgraded = _progression.TryUpgradeSword(upgradeId, currency);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                upgraded = _progression.TryUpgradeMagicWand(upgradeId, currency);
            }
            else
            {
                upgraded = _progression.TryUpgradeBow(upgradeId, currency);
            }
            if (!upgraded)
            {
                return false;
            }

            RefreshPlayerWeapon();
            return true;
        }

        public bool CanUpgradeWeapon(
            string upgradeId, out int cost,
            out int weaponPointsSpent, out int skillPointsSpent)
        {
            bool canUpgrade;
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                canUpgrade = _progression.CanUpgradeSword(
                    upgradeId, UpgradeCurrency.Weapon, out cost, true);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                canUpgrade = _progression.CanUpgradeMagicWand(
                    upgradeId, UpgradeCurrency.Weapon, out cost, true);
            }
            else
            {
                canUpgrade = _progression.CanUpgradeBow(
                    upgradeId, UpgradeCurrency.Weapon, out cost, true);
            }

            _progression.GetWeaponUpgradePayment(
                _selectedPlayerWeapon.Id, cost,
                out weaponPointsSpent, out skillPointsSpent);
            return canUpgrade;
        }

        public bool TryUpgradeWeaponWithSkillPoints(string upgradeId)
        {
            bool upgraded;
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                upgraded = _progression.TryUpgradeSword(
                    upgradeId, UpgradeCurrency.Weapon, true);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                upgraded = _progression.TryUpgradeMagicWand(
                    upgradeId, UpgradeCurrency.Weapon, true);
            }
            else
            {
                upgraded = _progression.TryUpgradeBow(
                    upgradeId, UpgradeCurrency.Weapon, true);
            }
            if (!upgraded)
            {
                return false;
            }

            RefreshPlayerWeapon();
            return true;
        }

        public bool TryUpgradePlayerStat(string statId)
        {
            if (!_progression.TryUpgradePlayerStat(statId))
            {
                return false;
            }

            Player.MaxHitPoints = PlayerCharacter.BaseMaxHitPoints
                + _progression.GetSkillRank(ArenaProgression.MaximumHealth)
                    * ArenaProgression.HealthIncreasePerRank;
            Player.MaxSpeed = PlayerCharacter.BaseMaxSpeed
                + _progression.GetSkillRank(ArenaProgression.MaximumSpeed)
                    * ArenaProgression.SpeedIncreasePerRank;
            Player.currentSpeed = Player.MaxSpeed;
            return true;
        }

        public void ToggleGodMode()
        {
            _progression.SetGodMode(!IsGodMode);
            Player.IsInvulnerable = IsGodMode;
        }

        private void RefreshPlayerWeapon()
        {
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                Player.EquippedWeapon = SwordUpgradeCatalog.CreateSword(_progression);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                Player.EquippedWeapon = MagicWandUpgradeCatalog.CreateMagicWand(
                    _progression);
            }
            else
            {
                Player.EquippedWeapon = BowUpgradeCatalog.CreateBow(_progression);
            }
        }

        public void ResetGame()
        {
            ResetPlayer();
            ZoomLevel = 1.0;
            ShowDamageRanges = false;
            ShowDebugHud = false;
            Score = 0;
            _progression.Reset();
            Player.IsInvulnerable = false;
            AimDirectionX = 0;
            AimDirectionY = 1;
            IsGameStarted = false;
            IsWeaponWheelOpen = false;
            HasActiveGame = false;
        }

        public void StartNewGame()
        {
            ResetGame();
            StartGame();
        }

        public void RestartGame()
        {
            double zoomLevel = ZoomLevel;
            bool showDamageRanges = ShowDamageRanges;
            bool showDebugHud = ShowDebugHud;

            ResetGame();
            ZoomLevel = zoomLevel;
            ShowDamageRanges = showDamageRanges;
            ShowDebugHud = showDebugHud;
            StartGame();
        }

        public void StartGame()
        {
            IsWeaponWheelOpen = false;
            IsGameStarted = true;
            HasActiveGame = true;
        }

        public void ToggleDamageRanges()
        {
            ShowDamageRanges = !ShowDamageRanges;
        }

        public void ToggleDebugHud()
        {
            ShowDebugHud = !ShowDebugHud;
        }

        public void SetAimDirection(double aimX, double aimY)
        {
            double length = Math.Sqrt(aimX * aimX + aimY * aimY);
            if (length == 0)
            {
                return;
            }

            AimDirectionX = aimX / length;
            AimDirectionY = aimY / length;
        }

        public bool ContinueGame()
        {
            if (!HasActiveGame || !Player.IsAlive)
            {
                return false;
            }

            StartGame();
            return true;
        }

        public void ReturnToStartMenu()
        {
            IsWeaponWheelOpen = false;
            IsGameStarted = false;
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            // The arena field has a matching painter that knows its layout.
            return painter is PainterG9;
        }

        public void UpdatePlayerPosition(double speedScale = 1)
        {
            if (!IsGameStarted)
            {
                return;
            }

            IReadOnlyList<Entity> entities = GetCombatEntities();
            List<EntityPositionSnapshot> previousPositions =
                CaptureEntityPositions(entities);
            double previousX = Player.PositionX;
            double previousY = Player.PositionY;
            double directionX = 0;
            double directionY = 0;

            if (_keyBindings.IsDown(ArenaAction.MoveUp) || Keyboard.IsKeyDown(Key.Up)) directionY -= 1;
            if (_keyBindings.IsDown(ArenaAction.MoveDown) || Keyboard.IsKeyDown(Key.Down)) directionY += 1;
            if (_keyBindings.IsDown(ArenaAction.MoveLeft) || Keyboard.IsKeyDown(Key.Left)) directionX -= 1;
            if (_keyBindings.IsDown(ArenaAction.MoveRight) || Keyboard.IsKeyDown(Key.Right)) directionX += 1;

            Rect movementBounds = MovementBounds;
            double sprintScale = Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightCtrl)
                    ? 1.5
                    : 1;
            Player.Move(directionX, directionY, movementBounds,
                speedScale * sprintScale);
            if (!TryResolveEntityOverlaps(
                Player, entities, movementBounds,
                Player.PositionX - previousX,
                Player.PositionY - previousY,
                new HashSet<Entity>()))
            {
                RestoreEntityPositions(previousPositions, Player);
            }
        }

        public void UpdateEnemyPositions(double speedScale = 1)
        {
            if (!IsGameStarted)
            {
                return;
            }

            Rect movementBounds = MovementBounds;
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                IReadOnlyList<Entity> entities = GetCombatEntities();
                List<EntityPositionSnapshot> previousPositions =
                    CaptureEntityPositions(entities);
                double previousX = enemy.PositionX;
                double previousY = enemy.PositionY;
                enemy.UpdateBehavior(this, speedScale);
                if (!TryResolveEntityOverlaps(
                    enemy, entities, movementBounds,
                    enemy.PositionX - previousX, enemy.PositionY - previousY,
                    new HashSet<Entity>()))
                {
                    RestoreEntityPositions(previousPositions, enemy);
                }
                enemy.TryAttackPlayerInRange(Player, _combatSystem);
            }

            if (Player.IsAlive)
            {
                UpdateEnemySpawning(GameTickDurationSeconds * speedScale);
            }
            else
            {
                IsWeaponWheelOpen = false;
                IsGameStarted = false;
            }
        }

        public void UpdateCombat(double deltaSeconds)
        {
            if (!IsGameStarted)
            {
                return;
            }

            _combatSystem.Update(
                deltaSeconds, GetCombatEntities(), MovementBounds);
            if (!Player.IsAlive)
            {
                IsWeaponWheelOpen = false;
                IsGameStarted = false;
            }
        }

        public void SpawnEnemy()
        {
            if (!IsGameStarted || !Player.IsAlive
                || GetLivingEnemyCount() >= MaximumEnemyCount)
            {
                return;
            }

            Point spawnPosition = GetRandomEnemySpawnPosition(Player);
            _enemies.Add(new PursuerEnemy(
                spawnPosition.X, spawnPosition.Y, Player.MaxSpeed));
        }

        private void UpdateEnemySpawning(double deltaSeconds)
        {
            _enemySpawnElapsedSeconds += deltaSeconds;
            if (_enemySpawnElapsedSeconds < EnemySpawnIntervalSeconds)
            {
                return;
            }

            _enemySpawnElapsedSeconds %= EnemySpawnIntervalSeconds;
            if (GetLivingEnemyCount() < MaximumEnemyCount)
            {
                SpawnEnemy();
            }
        }

        private int GetLivingEnemyCount()
        {
            int count = 0;
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    count++;
                }
            }
            return count;
        }

        private void OnEntityDefeated(Entity source, Entity target, Weapon weapon)
        {
            if (target is PursuerEnemy enemy)
            {
                _enemies.Remove(enemy);
                if (source.Team == CombatTeam.Player)
                {
                    Score++;
                    Weapon progressionWeapon = weapon;
                    if (weapon != null && Player.EquippedWeapon != null
                        && weapon.Id != Player.EquippedWeapon.Id)
                    {
                        progressionWeapon = Player.EquippedWeapon;
                    }
                    _progression.AwardKill(progressionWeapon);
                }
            }
        }

        private Point GetRandomEnemySpawnPosition(PlayerCharacter player)
        {
            double radius = PursuerEnemy.HitboxSize / 2;
            double minimumX = WallThickness + radius;
            double maximumX = MapWidth - WallThickness - radius;
            double minimumY = WallThickness + radius;
            double maximumY = MapHeight - WallThickness - radius;
            double minimumDistance = PursuerEnemy.SightRange + EnemySpawnVisibilityMargin;
            double minimumDistanceSquared = minimumDistance * minimumDistance;

            for (int attempt = 0; attempt < EnemySpawnPositionAttemptLimit; attempt++)
            {
                double positionX = minimumX + Random.Shared.NextDouble() * (maximumX - minimumX);
                double positionY = minimumY + Random.Shared.NextDouble() * (maximumY - minimumY);
                double offsetX = positionX - player.PositionX;
                double offsetY = positionY - player.PositionY;
                if (offsetX * offsetX + offsetY * offsetY >= minimumDistanceSquared)
                {
                    if (!OverlapsAnyEnemy(positionX, positionY, PursuerEnemy.HitboxSize))
                    {
                        return new Point(positionX, positionY);
                    }
                }
            }

            throw new InvalidOperationException(
                "Could not find a free enemy spawn position without overlapping another entity.");
        }

        private static List<EntityPositionSnapshot> CaptureEntityPositions(
            IReadOnlyList<Entity> entities)
        {
            List<EntityPositionSnapshot> snapshots =
                new List<EntityPositionSnapshot>(entities.Count);
            foreach (Entity entity in entities)
            {
                snapshots.Add(new EntityPositionSnapshot(
                    entity, entity.PositionX, entity.PositionY, entity.IsMoving));
            }
            return snapshots;
        }

        private static void RestoreEntityPositions(
            IReadOnlyList<EntityPositionSnapshot> snapshots, Entity blockedMover)
        {
            foreach (EntityPositionSnapshot snapshot in snapshots)
            {
                snapshot.Entity.RestorePosition(
                    snapshot.PositionX, snapshot.PositionY,
                    !ReferenceEquals(snapshot.Entity, blockedMover)
                        && snapshot.IsMoving);
            }
        }

        private static bool TryResolveEntityOverlaps(
            Entity entity, IReadOnlyList<Entity> entities,
            Rect movementBounds, double movementX, double movementY,
            HashSet<Entity> resolving)
        {
            if (!resolving.Add(entity))
            {
                return false;
            }

            try
            {
                foreach (Entity other in entities)
                {
                    if (ReferenceEquals(entity, other) || !other.IsAlive
                        || !HitboxesOverlap(
                            entity.PositionX, entity.PositionY, entity.Size,
                            other.PositionX, other.PositionY, other.Size))
                    {
                        continue;
                    }
                    if (!other.CanBePushed)
                    {
                        return false;
                    }

                    double directionX = other.PositionX - entity.PositionX;
                    double directionY = other.PositionY - entity.PositionY;
                    double distance = Math.Sqrt(
                        directionX * directionX + directionY * directionY);
                    if (distance == 0)
                    {
                        directionX = movementX;
                        directionY = movementY;
                        distance = Math.Sqrt(
                            directionX * directionX + directionY * directionY);
                    }
                    if (distance == 0)
                    {
                        return false;
                    }

                    directionX /= distance;
                    directionY /= distance;
                    double separation = (entity.Size + other.Size) / 2
                        - Math.Sqrt(
                            Math.Pow(other.PositionX - entity.PositionX, 2)
                            + Math.Pow(other.PositionY - entity.PositionY, 2))
                        + 0.01;
                    double pushedX = other.PositionX + directionX * separation;
                    double pushedY = other.PositionY + directionY * separation;
                    if (!IsWithinMovementBounds(other, pushedX, pushedY, movementBounds))
                    {
                        return false;
                    }

                    other.SetPosition(pushedX, pushedY);
                    if (!TryResolveEntityOverlaps(
                            other, entities, movementBounds,
                            directionX, directionY, resolving)
                        || HitboxesOverlap(
                            entity.PositionX, entity.PositionY, entity.Size,
                            other.PositionX, other.PositionY, other.Size))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                resolving.Remove(entity);
            }
        }

        private static bool IsWithinMovementBounds(
            Entity entity, double positionX, double positionY, Rect bounds)
        {
            double radius = entity.Size / 2;
            return positionX - radius >= bounds.Left
                && positionX + radius <= bounds.Right
                && positionY - radius >= bounds.Top
                && positionY + radius <= bounds.Bottom;
        }

        private bool OverlapsAnyEnemy(double positionX, double positionY, double size)
        {
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (enemy.IsAlive
                    && HitboxesOverlap(positionX, positionY, size,
                        enemy.PositionX, enemy.PositionY, enemy.Size))
                {
                    return true;
                }
            }

            return false;
        }

        private IReadOnlyList<Entity> GetCombatEntities()
        {
            List<Entity> entities = new List<Entity>(_enemies.Count + 1)
            {
                Player
            };
            foreach (PursuerEnemy enemy in _enemies)
            {
                entities.Add(enemy);
            }
            return entities;
        }

        public double GetAttackCooldownRemaining(Entity entity)
        {
            return _combatSystem.GetCooldownRemaining(entity);
        }

        public double GetAttackCooldownRemaining(Entity entity, Weapon weapon)
        {
            return _combatSystem.GetCooldownRemaining(entity, weapon);
        }

        private static bool HitboxesOverlap(
            double firstX, double firstY, double firstSize,
            double secondX, double secondY, double secondSize)
        {
            double minimumDistance = (firstSize + secondSize) / 2;
            double offsetX = firstX - secondX;
            double offsetY = firstY - secondY;
            return offsetX * offsetX + offsetY * offsetY
                < minimumDistance * minimumDistance;
        }

        private sealed class EntityPositionSnapshot
        {
            public EntityPositionSnapshot(
                Entity entity, double positionX, double positionY, bool isMoving)
            {
                Entity = entity;
                PositionX = positionX;
                PositionY = positionY;
                IsMoving = isMoving;
            }

            public Entity Entity { get; }
            public double PositionX { get; }
            public double PositionY { get; }
            public bool IsMoving { get; }
        }

        public void AttackWithPlayer(double aimX, double aimY)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return;
            }

            SetAimDirection(aimX, aimY);
            aimX = AimDirectionX;
            aimY = AimDirectionY;
            FaceAttackDirection(aimX, aimY);

            if (_combatSystem.TryStartAttack(Player, aimX, aimY))
            {
                Player.StartAttack();
            }
        }

        public bool AttackWithWeaponAbility(int slotIndex)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return false;
            }

            Weapon ability = _progression.GetWeaponAbilityWeapon(
                _selectedPlayerWeapon.Id, slotIndex);
            if (ability == null)
            {
                return false;
            }

            FaceAttackDirection(AimDirectionX, AimDirectionY);
            bool started = _combatSystem.TryStartAttack(
                Player, ability, AimDirectionX, AimDirectionY);
            if (started)
            {
                Player.StartAttack();
            }
            return started;
        }

        public bool AttackWithEntityAbility(int slotIndex)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return false;
            }

            Weapon ability = _progression.GetEntityAbilityWeapon(slotIndex);
            if (ability == null)
            {
                return false;
            }

            bool started = _combatSystem.TryStartAttack(
                Player, ability, AimDirectionX, AimDirectionY);
            if (started)
            {
                Player.StartAttack();
            }
            return started;
        }

        private void FaceAttackDirection(double directionX, double directionY)
        {
            Player.Facing = Math.Abs(directionX) >= Math.Abs(directionY)
                ? (directionX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (directionY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
        }

        public bool IsPlayerSpinning
        {
            get
            {
                return GetPlayerSpinVisual() != null;
            }
        }

        public PlayerFacing PlayerSpinFacing
        {
            get
            {
                AttackVisualState visual = GetPlayerSpinVisual();
                if (visual == null)
                {
                    return Player.Facing;
                }

                return Math.Abs(visual.DirectionX) >= Math.Abs(visual.DirectionY)
                    ? (visual.DirectionX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                    : (visual.DirectionY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
            }
        }

        private AttackVisualState GetPlayerSpinVisual()
        {
            foreach (AttackVisualState visual in _combatSystem.GetActiveVisuals())
            {
                if (ReferenceEquals(visual.Source, Player)
                    && visual.AnimationPlaceholder == "sword-spin")
                {
                    return visual;
                }
            }
            return null;
        }

        public bool HandleMouseDown(MouseButton button)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return false;
            }

            if (_keyBindings.MatchesMouseButton(ArenaAction.WeaponAttack, button))
            {
                AttackWithPlayer(AimDirectionX, AimDirectionY);
                return true;
            }

            ArenaAction[] weaponAbilityActions =
            {
                ArenaAction.WeaponAbility1, ArenaAction.WeaponAbility2,
                ArenaAction.WeaponAbility3, ArenaAction.WeaponAbility4
            };
            for (int i = 0; i < weaponAbilityActions.Length; i++)
            {
                if (_keyBindings.MatchesMouseButton(weaponAbilityActions[i], button))
                {
                    AttackWithWeaponAbility(i);
                    return true;
                }
            }

            ArenaAction[] entityAbilityActions =
            {
                ArenaAction.EntityAbility1, ArenaAction.EntityAbility2,
                ArenaAction.EntityAbility3, ArenaAction.EntityAbility4
            };
            for (int i = 0; i < entityAbilityActions.Length; i++)
            {
                if (_keyBindings.MatchesMouseButton(entityAbilityActions[i], button))
                {
                    AttackWithEntityAbility(i);
                    return true;
                }
            }
            return false;
        }

        public bool HandleKeyDown(Key key)
        {
            if (key == Key.None)
            {
                return false;
            }

            if (IsGameStarted && Player.IsAlive
                && _keyBindings.MatchesKey(ArenaAction.WeaponWheel, key))
            {
                IsWeaponWheelOpen = true;
                return true;
            }

            if (key == _keyBindings.GetKey(ArenaAction.Pause))
            {
                if (IsWeaponWheelOpen)
                {
                    IsWeaponWheelOpen = false;
                    return true;
                }
                if (IsGameStarted)
                {
                    ReturnToStartMenu();
                    return true;
                }
                if (HasActiveGame && Player.IsAlive)
                {
                    ContinueGame();
                    return true;
                }
            }

            if (IsWeaponWheelOpen)
            {
                return true;
            }

            if (IsGameStarted)
            {
                if (_keyBindings.MatchesKey(ArenaAction.WeaponAttack, key))
                {
                    AttackWithPlayer(AimDirectionX, AimDirectionY);
                    return true;
                }
                if (key == _keyBindings.GetKey(ArenaAction.ToggleDebugRanges))
                {
                    ToggleDamageRanges();
                    return true;
                }
                if (key == _keyBindings.GetKey(ArenaAction.ToggleDebugHud))
                {
                    ToggleDebugHud();
                    return true;
                }
                if (key == _keyBindings.GetKey(ArenaAction.SpawnEnemy))
                {
                    SpawnEnemy();
                    return true;
                }

                ArenaAction[] weaponAbilityActions =
                {
                    ArenaAction.WeaponAbility1, ArenaAction.WeaponAbility2,
                    ArenaAction.WeaponAbility3, ArenaAction.WeaponAbility4
                };
                for (int i = 0; i < weaponAbilityActions.Length; i++)
                {
                    if (_keyBindings.MatchesKey(weaponAbilityActions[i], key))
                    {
                        AttackWithWeaponAbility(i);
                        return true;
                    }
                }

                ArenaAction[] entityAbilityActions =
                {
                    ArenaAction.EntityAbility1, ArenaAction.EntityAbility2,
                    ArenaAction.EntityAbility3, ArenaAction.EntityAbility4
                };
                for (int i = 0; i < entityAbilityActions.Length; i++)
                {
                    if (_keyBindings.MatchesKey(entityAbilityActions[i], key))
                    {
                        AttackWithEntityAbility(i);
                        return true;
                    }
                }
            }

            if (key == _keyBindings.GetKey(ArenaAction.ZoomIn))
            {
                HandleZoomKey(Key.K);
                return true;
            }
            if (key == _keyBindings.GetKey(ArenaAction.ZoomOut))
            {
                HandleZoomKey(Key.L);
                return true;
            }
            return false;
        }

        public bool HandleKeyUp(Key key)
        {
            if (!IsWeaponWheelOpen
                || !_keyBindings.MatchesKey(ArenaAction.WeaponWheel, key))
            {
                return false;
            }

            int selectedIndex = WeaponWheelSelectionIndex;
            SelectPlayerWeapon(ArenaWeaponWheel.Weapons[selectedIndex]);
            IsWeaponWheelOpen = false;
            return true;
        }

        public void CancelWeaponWheel()
        {
            IsWeaponWheelOpen = false;
        }

        // Handle zoom from the routed key event so short key presses are not missed between ticks.
        public void HandleZoomKey(Key key)
        {
            if (key == Key.K)
            {
                ChangeZoom(1);
            }
            else if (key == Key.L)
            {
                ChangeZoom(-1);
            }
        }

        public void HandleZoomWheel(int delta)
        {
            if (delta != 0)
            {
                ChangeZoom(Math.Sign(delta));
            }
        }

        private void ChangeZoom(int direction)
        {
            ZoomLevel = Math.Max(MinimumZoom,
                Math.Min(MaximumZoom, ZoomLevel + direction * ZoomStep));
        }
    }
}