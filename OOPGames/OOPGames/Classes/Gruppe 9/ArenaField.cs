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
        private const double EnemySpawnDistance = 250;
        private const double EnemySpawnAngleStep = 2.399963229728653;
        private readonly List<PursuerEnemy> _enemies = new List<PursuerEnemy>();

        // The field owns the current player instance.
        public PlayerCharacter Player { get; private set; }
        public IReadOnlyList<PursuerEnemy> Enemies { get { return _enemies; } }
        public bool IsGameStarted { get; private set; }
        public bool HasActiveGame { get; private set; }
        public bool ShowDamageRanges { get; private set; }
        public double AimDirectionX { get; private set; }
        public double AimDirectionY { get; private set; } = 1;
        public double ZoomLevel { get; private set; } = 1.0;

        public ArenaField()
        {
            // Create the player in the middle when a new field is constructed.
            ResetPlayer();
        }

        public void ResetPlayer()
        {
            // Resetting starts the player at the center of the arena.
            Player = new PlayerCharacter(MapWidth / 2, MapHeight / 2);
            _enemies.Clear();
            _enemies.Add(new PursuerEnemy(
                Player.PositionX + EnemySpawnDistance, Player.PositionY, Player.MaxSpeed));
        }

        public void ResetGame()
        {
            ResetPlayer();
            ZoomLevel = 1.0;
            ShowDamageRanges = false;
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

        public void StartGame()
        {
            IsGameStarted = true;
            HasActiveGame = true;
        }

        public void ToggleDamageRanges()
        {
            ShowDamageRanges = !ShowDamageRanges;
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
            if (!HasActiveGame)
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
            double directionX = 0;
            double directionY = 0;

            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up)) directionY -= 1;
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down)) directionY += 1;
            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left)) directionX -= 1;
            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right)) directionX += 1;

            Player.Move(directionX, directionY,
                new Rect(WallThickness, WallThickness,
                    MapWidth - 2 * WallThickness, MapHeight - 2 * WallThickness));
        }

        public void UpdateEnemyPositions()
        {
            foreach (PursuerEnemy enemy in _enemies)
            {
                enemy.UpdateBehavior(this);
                enemy.TryDamagePlayerOnContact(Player);
            }
        }

        public void SpawnEnemy()
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return;
            }

            double angle = _enemies.Count * EnemySpawnAngleStep;
            double positionX = Player.PositionX + Math.Cos(angle) * EnemySpawnDistance;
            double positionY = Player.PositionY + Math.Sin(angle) * EnemySpawnDistance;
            double minimumX = WallThickness + 17;
            double maximumX = MapWidth - WallThickness - 17;
            double minimumY = WallThickness + 17;
            double maximumY = MapHeight - WallThickness - 17;

            if (positionX < minimumX || positionX > maximumX
                || positionY < minimumY || positionY > maximumY)
            {
                double towardCenterX = MapWidth / 2 - Player.PositionX;
                double towardCenterY = MapHeight / 2 - Player.PositionY;
                double length = Math.Sqrt(
                    towardCenterX * towardCenterX + towardCenterY * towardCenterY);
                if (length > 0)
                {
                    positionX = Player.PositionX
                        + towardCenterX / length * EnemySpawnDistance;
                    positionY = Player.PositionY
                        + towardCenterY / length * EnemySpawnDistance;
                }
            }

            positionX = Math.Max(minimumX, Math.Min(maximumX, positionX));
            positionY = Math.Max(minimumY, Math.Min(maximumY, positionY));
            _enemies.Add(new PursuerEnemy(positionX, positionY, Player.MaxSpeed));
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

            PursuerEnemy target = null;
            double nearestDistance = PlayerAttackRange;
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                double offsetX = enemy.PositionX - Player.PositionX;
                double offsetY = enemy.PositionY - Player.PositionY;
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                double forwardDistance = offsetX * aimX + offsetY * aimY;
                double sideDistance = Math.Abs(offsetX * aimY - offsetY * aimX);
                if (distance <= nearestDistance
                    && forwardDistance >= 0
                    && sideDistance <= forwardDistance
                        * Math.Tan(PlayerAttackHalfAngleRadians))
                {
                    nearestDistance = distance;
                    target = enemy;
                }
            }

            if (target != null)
            {
                target.TakeDamage(10);
            }

            Player.StartAttack();
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