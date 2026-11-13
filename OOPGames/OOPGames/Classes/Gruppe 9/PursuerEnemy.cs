using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PursuerEnemy : NonPlayerCharacter
    {
        public const int ContactDamagePerHit = 10;
        public const int ContactDamageIntervalMilliseconds = 1000;
        public const double SightRange = 200;
        private const double WanderSpeedMultiplier = 0.5;
        private const double MinimumWanderDurationSeconds = 1.5;
        private const double WanderDurationVarianceSeconds = 2.5;
        private const int MaximumPathPoints = 200;
        private static readonly TimeSpan ContactDamageInterval =
            TimeSpan.FromMilliseconds(ContactDamageIntervalMilliseconds);
        private static readonly TimeSpan AttackVisualDuration = TimeSpan.FromMilliseconds(250);
        private readonly List<Point> _pathHistory = new List<Point>();
        private long _lastContactDamageTimestamp;
        private long _attackTimestamp;
        private long _nextWanderDirectionChange;
        private double _wanderDirectionX;
        private double _wanderDirectionY;

        public PursuerEnemy(double x, double y, double playerSpeed)
            : base(x, y, 30, playerSpeed * 0.75, 34, Brushes.Crimson)
        {
            ChooseWanderDirection();
            RecordPathPosition();
        }

        public IReadOnlyList<Point> PathHistory { get { return _pathHistory; } }

        public bool IsAttacking
        {
            get
            {
                return _attackTimestamp != 0
                    && Stopwatch.GetElapsedTime(_attackTimestamp) < AttackVisualDuration;
            }
        }

        public void TryDamagePlayerOnContact(PlayerCharacter player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            double offsetX = player.PositionX - PositionX;
            double offsetY = player.PositionY - PositionY;
            double contactDistance = (Size + player.Size) / 2;
            if (!IsAlive || !player.IsAlive
                || offsetX * offsetX + offsetY * offsetY > contactDistance * contactDistance)
            {
                _lastContactDamageTimestamp = 0;
                return;
            }

            long now = Stopwatch.GetTimestamp();
            if (_lastContactDamageTimestamp != 0
                && Stopwatch.GetElapsedTime(_lastContactDamageTimestamp, now)
                    < ContactDamageInterval)
            {
                return;
            }

            player.TakeDamage(ContactDamagePerHit);
            _lastContactDamageTimestamp = now;
            _attackTimestamp = now;
        }

        public override void UpdateBehavior(ArenaField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            PlayerCharacter player = field.Player;
            if (!IsAlive || !player.IsAlive)
            {
                Move(0, 0, new System.Windows.Rect(
                    ArenaField.WallThickness, ArenaField.WallThickness,
                    ArenaField.MapWidth - 2 * ArenaField.WallThickness,
                    ArenaField.MapHeight - 2 * ArenaField.WallThickness));
                return;
            }

            double offsetX = player.PositionX - PositionX;
            double offsetY = player.PositionY - PositionY;
            System.Windows.Rect movementBounds = new System.Windows.Rect(
                ArenaField.WallThickness, ArenaField.WallThickness,
                ArenaField.MapWidth - 2 * ArenaField.WallThickness,
                ArenaField.MapHeight - 2 * ArenaField.WallThickness);

            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distance > SightRange)
            {
                UpdateWanderBehavior(movementBounds);
                return;
            }

            FacePlayer(offsetX, offsetY);
            if (currentSpeed == 0)
            {
                Move(0, 0, movementBounds);
                return;
            }

            double stoppingDistance = (Size + player.Size) / 2;
            if (distance <= stoppingDistance)
            {
                Move(0, 0, movementBounds);
                return;
            }

            double travel = Math.Min(currentSpeed, distance - stoppingDistance);
            Move(offsetX / distance * travel / currentSpeed,
                offsetY / distance * travel / currentSpeed, movementBounds);
            FacePlayer(offsetX, offsetY);
            RecordPathPosition();
        }

        private void UpdateWanderBehavior(System.Windows.Rect movementBounds)
        {
            if (currentSpeed == 0)
            {
                Move(0, 0, movementBounds);
                return;
            }

            if (Stopwatch.GetTimestamp() >= _nextWanderDirectionChange)
            {
                ChooseWanderDirection();
            }

            double previousX = PositionX;
            double previousY = PositionY;
            Move(_wanderDirectionX * WanderSpeedMultiplier,
                _wanderDirectionY * WanderSpeedMultiplier, movementBounds);
            if (PositionX == previousX && PositionY == previousY)
            {
                TurnTowardArenaCenter();
                Move(_wanderDirectionX * WanderSpeedMultiplier,
                    _wanderDirectionY * WanderSpeedMultiplier, movementBounds);
            }
            RecordPathPosition();
        }

        private void RecordPathPosition()
        {
            Point position = new Point(PositionX, PositionY);
            if (_pathHistory.Count == 0 || _pathHistory[_pathHistory.Count - 1] != position)
            {
                _pathHistory.Add(position);
                if (_pathHistory.Count > MaximumPathPoints)
                {
                    _pathHistory.RemoveAt(0);
                }
            }
        }

        private void ChooseWanderDirection()
        {
            double angle = Random.Shared.NextDouble() * Math.PI * 2;
            _wanderDirectionX = Math.Cos(angle);
            _wanderDirectionY = Math.Sin(angle);
            ScheduleNextWanderDirectionChange();
        }

        private void TurnTowardArenaCenter()
        {
            double directionX = ArenaField.MapWidth / 2 - PositionX;
            double directionY = ArenaField.MapHeight / 2 - PositionY;
            double distance = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (distance == 0)
            {
                ChooseWanderDirection();
                return;
            }

            _wanderDirectionX = directionX / distance;
            _wanderDirectionY = directionY / distance;
            ScheduleNextWanderDirectionChange();
        }

        private void ScheduleNextWanderDirectionChange()
        {
            double duration = MinimumWanderDurationSeconds
                + Random.Shared.NextDouble() * WanderDurationVarianceSeconds;
            _nextWanderDirectionChange = Stopwatch.GetTimestamp()
                + (long)(duration * Stopwatch.Frequency);
        }

        private void FacePlayer(double offsetX, double offsetY)
        {
            Facing = Math.Abs(offsetX) >= Math.Abs(offsetY)
                ? (offsetX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (offsetY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
        }
    }
}
