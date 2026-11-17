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
        public const double PlayerAttackRange = 80;
        public const double PlayerAttackHalfAngleRadians = Math.PI / 6;
        public const int PlayerAttackDamage = 10;
        private const double EnemySpawnVisibilityMargin = 50;
        private const int EnemySpawnPositionAttemptLimit = 10000;
        private readonly List<PursuerEnemy> _enemies = new List<PursuerEnemy>();
        private readonly ArenaCombatSystem _combatSystem = new ArenaCombatSystem();
        private Weapon _selectedPlayerWeapon = WeaponCatalog.PracticeSword;

        // The field owns the current player instance.
        public PlayerCharacter Player { get; private set; }
        public IReadOnlyList<PursuerEnemy> Enemies { get { return _enemies; } }
        public IReadOnlyList<Weapon> TestWeapons { get { return WeaponCatalog.TestWeapons; } }
        public Weapon SelectedPlayerWeapon { get { return _selectedPlayerWeapon; } }
        public int ActiveAttackCount { get { return _combatSystem.ActiveAttackCount; } }
        public IReadOnlyList<AttackVisualState> ActiveAttackVisuals
        {
            get { return _combatSystem.GetActiveVisuals(); }
        }
        public bool IsGameStarted { get; private set; }
        public bool HasActiveGame { get; private set; }
        public bool IsGameOver { get { return HasActiveGame && !Player.IsAlive; } }
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
            _enemies.Clear();
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

            Weapon availableWeapon = null;
            foreach (Weapon candidate in WeaponCatalog.TestWeapons)
            {
                if (candidate.Id == weapon.Id)
                {
                    availableWeapon = candidate;
                    break;
                }
            }
            if (availableWeapon == null)
            {
                throw new ArgumentException(
                    "The selected weapon is not available in the test menu.",
                    nameof(weapon));
            }

            _selectedPlayerWeapon = availableWeapon;
            Player.EquippedWeapon = availableWeapon;
        }

        public void ResetGame()
        {
            ResetPlayer();
            ZoomLevel = 1.0;
            ShowDamageRanges = false;
            ShowDebugHud = false;
            Score = 0;
            AimDirectionX = 0;
            AimDirectionY = 1;
            IsGameStarted = false;
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
            IsGameStarted = false;
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            // The arena field has a matching painter that knows its layout.
            return painter is PainterG9;
        }

        public void UpdatePlayerPosition()
        {
            if (!IsGameStarted)
            {
                return;
            }

            double previousX = Player.PositionX;
            double previousY = Player.PositionY;
            double directionX = 0;
            double directionY = 0;

            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up)) directionY -= 1;
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down)) directionY += 1;
            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left)) directionX -= 1;
            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right)) directionX += 1;

            Rect movementBounds = MovementBounds;
            Player.Move(directionX, directionY, movementBounds);
            if (OverlapsAnotherEntity(Player))
            {
                Player.PositionX = previousX;
                Player.PositionY = previousY;
                Player.Move(0, 0, movementBounds);
            }
        }

        public void UpdateEnemyPositions()
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

                double previousX = enemy.PositionX;
                double previousY = enemy.PositionY;
                enemy.UpdateBehavior(this);
                if (OverlapsAnotherEntity(enemy))
                {
                    enemy.PositionX = previousX;
                    enemy.PositionY = previousY;
                    enemy.Move(0, 0, movementBounds);
                }
                enemy.TryAttackPlayerInRange(Player, _combatSystem);
            }

            if (!Player.IsAlive)
            {
                IsGameStarted = false;
            }
        }

        public void UpdateCombat(double deltaSeconds)
        {
            if (!IsGameStarted)
            {
                return;
            }

            _combatSystem.Update(deltaSeconds, GetCombatEntities());
            if (!Player.IsAlive)
            {
                IsGameStarted = false;
            }
        }

        public void SpawnEnemy()
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return;
            }

            Point spawnPosition = GetRandomEnemySpawnPosition(Player);
            _enemies.Add(new PursuerEnemy(
                spawnPosition.X, spawnPosition.Y, Player.MaxSpeed));
        }

        private void OnEntityDefeated(Entity source, Entity target)
        {
            if (target is PursuerEnemy enemy)
            {
                _enemies.Remove(enemy);
                if (source.Team == CombatTeam.Player)
                {
                    Score++;
                }
                if (IsGameStarted && Player.IsAlive)
                {
                    SpawnEnemy();
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

        private bool OverlapsAnotherEntity(Entity entity)
        {
            if (!ReferenceEquals(entity, Player) && Player.IsAlive
                && HitboxesOverlap(entity.PositionX, entity.PositionY, entity.Size,
                    Player.PositionX, Player.PositionY, Player.Size))
            {
                return true;
            }

            foreach (PursuerEnemy otherEnemy in _enemies)
            {
                if (!ReferenceEquals(entity, otherEnemy) && otherEnemy.IsAlive
                    && HitboxesOverlap(entity.PositionX, entity.PositionY, entity.Size,
                        otherEnemy.PositionX, otherEnemy.PositionY, otherEnemy.Size))
                {
                    return true;
                }
            }

            return false;
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

        public void AttackWithPlayer(double aimX, double aimY)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return;
            }

            SetAimDirection(aimX, aimY);
            aimX = AimDirectionX;
            aimY = AimDirectionY;
            Player.Facing = Math.Abs(aimX) >= Math.Abs(aimY)
                ? (aimX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (aimY < 0 ? PlayerFacing.Back : PlayerFacing.Front);

            if (_combatSystem.TryStartAttack(Player, aimX, aimY))
            {
                Player.StartAttack();
            }
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