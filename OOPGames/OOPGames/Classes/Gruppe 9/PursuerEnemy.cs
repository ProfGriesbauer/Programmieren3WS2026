using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PursuerEnemy : NonPlayerCharacter
    {
        public const double AttackRange = 60;
        public const int AttackDamagePerHit = 10;
        public const int AttackIntervalMilliseconds = 1000;
        public const double SightRange = 200;
        public const double SightRangeAfterDetectionBonus = 50;
        public const double HitboxSize = 34;
        private const double GameTickDurationSeconds = 0.04;
        private const double WanderSpeedMultiplier = 0.5;
        private const double MinimumWanderDurationSeconds = 1.5;
        private const double WanderDurationVarianceSeconds = 2.5;
        private static readonly TimeSpan AttackInterval =
            TimeSpan.FromMilliseconds(AttackIntervalMilliseconds);
        private static readonly TimeSpan AttackVisualDuration = TimeSpan.FromMilliseconds(250);
        private long _lastAttackTimestamp;
        private long _attackTimestamp;
        private long _nextWanderDirectionChange;
        private bool _hasSeenPlayer;
        private double _wanderDirectionX;
        private double _wanderDirectionY;

        public PursuerEnemy(double x, double y, double playerSpeed)
            : base(x, y, 30, playerSpeed * 0.75, HitboxSize, Brushes.Crimson)
        {
            ChooseWanderDirection();
        }

        public double CurrentSightRange
        {
            get
            {
                return _hasSeenPlayer
                    ? SightRange + SightRangeAfterDetectionBonus
                    : SightRange;
            }
        }

        public bool HasSeenPlayer { get { return _hasSeenPlayer; } }

        public Point GetPlannedDestination(ArenaField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            Point currentPosition = new Point(PositionX, PositionY);
            PlayerCharacter player = field.Player;
            if (!IsAlive || !player.IsAlive || currentSpeed == 0)
            {
                return currentPosition;
            }

            double offsetX = player.PositionX - PositionX;
            double offsetY = player.PositionY - PositionY;
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distance <= CurrentSightRange)
            {
                double stoppingDistance = GetMinimumSeparation(player);
                if (distance <= stoppingDistance)
                {
                    return currentPosition;
                }

                double travelDistance = distance - stoppingDistance;
                return new Point(
                    PositionX + offsetX / distance * travelDistance,
                    PositionY + offsetY / distance * travelDistance);
            }

            long remainingTicks = _nextWanderDirectionChange - Stopwatch.GetTimestamp();
            if (remainingTicks <= 0)
            {
                return currentPosition;
            }

            double remainingSeconds = (double)remainingTicks / Stopwatch.Frequency;
            double travel = currentSpeed * WanderSpeedMultiplier
                * remainingSeconds / GameTickDurationSeconds;
            return ClampToMovementBounds(currentPosition,
                _wanderDirectionX, _wanderDirectionY, travel);
        }

        public bool IsAttacking
        {
            get
            {
                return _attackTimestamp != 0
                    && Stopwatch.GetElapsedTime(_attackTimestamp) < AttackVisualDuration;
            }
        }

        public void TryAttackPlayerInRange(PlayerCharacter player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            double offsetX = player.PositionX - PositionX;
            double offsetY = player.PositionY - PositionY;
            double attackRangeSquared = AttackRange * AttackRange;
            if (!IsAlive || !player.IsAlive
                || offsetX * offsetX + offsetY * offsetY > attackRangeSquared)
            {
                _lastAttackTimestamp = 0;
                return;
            }

            long now = Stopwatch.GetTimestamp();
            if (_lastAttackTimestamp != 0
                && Stopwatch.GetElapsedTime(_lastAttackTimestamp, now)
                    < AttackInterval)
            {
                return;
            }

            player.TakeDamage(AttackDamagePerHit);
            _lastAttackTimestamp = now;
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
            if (distance > CurrentSightRange)
            {
                _hasSeenPlayer = false;
                UpdateWanderBehavior(movementBounds);
                return;
            }

            _hasSeenPlayer = true;
            FacePlayer(offsetX, offsetY);
            if (currentSpeed == 0)
            {
                Move(0, 0, movementBounds);
                return;
            }

            double stoppingDistance = GetMinimumSeparation(player);
            if (distance <= stoppingDistance)
            {
                Move(0, 0, movementBounds);
                return;
            }

            double travel = Math.Min(currentSpeed, distance - stoppingDistance);
            Move(offsetX / distance * travel / currentSpeed,
                offsetY / distance * travel / currentSpeed, movementBounds);
            FacePlayer(offsetX, offsetY);
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
        }

        private Point ClampToMovementBounds(
            Point start, double directionX, double directionY, double distance)
        {
            double radius = Size / 2;
            double minimumX = ArenaField.WallThickness + radius;
            double maximumX = ArenaField.MapWidth - ArenaField.WallThickness - radius;
            double minimumY = ArenaField.WallThickness + radius;
            double maximumY = ArenaField.MapHeight - ArenaField.WallThickness - radius;
            double destinationX = start.X + directionX * distance;
            double destinationY = start.Y + directionY * distance;
            return new Point(
                Math.Max(minimumX, Math.Min(maximumX, destinationX)),
                Math.Max(minimumY, Math.Min(maximumY, destinationY)));
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

        private double GetMinimumSeparation(PlayerCharacter player)
        {
            return (Size + player.Size) / 2;
        }
    }
}
