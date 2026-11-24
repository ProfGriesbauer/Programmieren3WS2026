using System;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PursuerEnemy : NonPlayerCharacter
    {
        public const double AttackRange = 60;
        public const int AttackDamagePerHit = 10;
        public const double SightRange = 200;
        public const double SightRangeAfterDetectionBonus = 50;
        public const double HitboxSize = 34;
        private const double WanderSpeedMultiplier = 0.5;
        private const double MinimumWanderDurationSeconds = 1.5;
        private const double WanderDurationVarianceSeconds = 2.5;
        // Game time until the next wander direction change; follows slow motion.
        private double _wanderRemainingSeconds;
        private bool _hasSeenPlayer;
        private double _wanderDirectionX;
        private double _wanderDirectionY;

        public PursuerEnemy(double x, double y, double playerSpeed)
            : base(x, y, 30, playerSpeed * 0.75, HitboxSize,
                Brushes.Crimson, CombatTeam.Enemies)
        {
            EquippedWeapon = WeaponCatalog.EnemyClaws;
            CanBePushed = true;
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
        public Entity CurrentTarget { get; private set; }

        public Point GetPlannedDestination(ArenaField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            Point currentPosition = new Point(PositionX, PositionY);
            Entity target = GetCurrentTarget(field);
            if (!IsAlive || EffectiveSpeed == 0)
            {
                return currentPosition;
            }

            if (target != null)
            {
                double offsetX = target.PositionX - PositionX;
                double offsetY = target.PositionY - PositionY;
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                double stoppingDistance = GetMinimumSeparation(target);
                if (distance <= stoppingDistance)
                {
                    return currentPosition;
                }

                double travelDistance = distance - stoppingDistance;
                return new Point(
                    PositionX + offsetX / distance * travelDistance,
                    PositionY + offsetY / distance * travelDistance);
            }

            if (_wanderRemainingSeconds <= 0)
            {
                return currentPosition;
            }

            // Speeds are distances per regular tick; convert the remaining game time to ticks.
            double travel = EffectiveSpeed * WanderSpeedMultiplier
                * _wanderRemainingSeconds / ArenaField.GameTickDurationSeconds;
            return ClampToMovementBounds(field, currentPosition,
                _wanderDirectionX, _wanderDirectionY, travel);
        }

        public bool TryAttackPlayerInRange(
            PlayerCharacter player, ArenaCombatSystem combatSystem)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }
            return TryAttackTargetInRange(player, combatSystem);
        }

        public bool TryAttackTargetInRange(
            Entity target, ArenaCombatSystem combatSystem)
        {
            if (target == null)
            {
                return false;
            }
            if (combatSystem == null)
            {
                throw new ArgumentNullException(nameof(combatSystem));
            }

            double offsetX = target.PositionX - PositionX;
            double offsetY = target.PositionY - PositionY;
            double range = EquippedWeapon.AttackPattern.Range;
            double attackRangeSquared = range * range;
            if (!IsAlive || !target.IsAlive || target.Team != CombatTeam.Player
                || offsetX * offsetX + offsetY * offsetY > attackRangeSquared)
            {
                return false;
            }

            return combatSystem.TryStartAttack(this, offsetX, offsetY);
        }

        public override void UpdateBehavior(ArenaField field)
        {
            UpdateBehavior(field, 1);
        }

        public override void UpdateBehavior(ArenaField field, double speedScale)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            _wanderRemainingSeconds -= ArenaField.GameTickDurationSeconds * speedScale;
            if (!IsAlive)
            {
                Move(0, 0, field.MovementArea, speedScale);
                return;
            }

            Entity target = GetCurrentTarget(field);
            if (target == null)
            {
                UpdateWanderBehavior(field, field.MovementArea, speedScale);
                return;
            }

            double offsetX = target.PositionX - PositionX;
            double offsetY = target.PositionY - PositionY;
            System.Windows.Rect movementBounds = field.MovementArea;

            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            FacePlayer(offsetX, offsetY);
            if (EffectiveSpeed == 0)
            {
                Move(0, 0, movementBounds, speedScale);
                return;
            }

            double stoppingDistance = GetMinimumSeparation(target);
            if (distance <= stoppingDistance)
            {
                Move(0, 0, movementBounds, speedScale);
                return;
            }

            double travel = Math.Min(currentSpeed, distance - stoppingDistance);
            Move(offsetX / distance * travel / currentSpeed,
                offsetY / distance * travel / currentSpeed, movementBounds,
                speedScale);
            FacePlayer(offsetX, offsetY);
        }

        private Entity GetCurrentTarget(ArenaField field)
        {
            Entity target = FindClosestVisibleTarget(field, CurrentSightRange);
            if (target == null && _hasSeenPlayer)
            {
                target = FindClosestVisibleTarget(field, SightRange);
            }

            _hasSeenPlayer = target != null;
            CurrentTarget = target;
            return target;
        }

        private Entity FindClosestVisibleTarget(ArenaField field, double sightRange)
        {
            Entity closestTarget = null;
            double closestDistanceSquared = sightRange * sightRange;
            ConsiderTarget(field.Player, ref closestTarget, ref closestDistanceSquared);
            foreach (MirrorImage mirrorImage in field.MirrorImages)
            {
                ConsiderTarget(
                    mirrorImage, ref closestTarget, ref closestDistanceSquared);
            }
            return closestTarget;
        }

        private void ConsiderTarget(
            Entity target, ref Entity closestTarget, ref double closestDistanceSquared)
        {
            if (!target.IsAlive)
            {
                return;
            }

            double offsetX = target.PositionX - PositionX;
            double offsetY = target.PositionY - PositionY;
            double distanceSquared = offsetX * offsetX + offsetY * offsetY;
            if (distanceSquared <= closestDistanceSquared)
            {
                closestTarget = target;
                closestDistanceSquared = distanceSquared;
            }
        }

        private void UpdateWanderBehavior(
            ArenaField field,
            System.Windows.Rect movementBounds, double speedScale)
        {
            if (EffectiveSpeed == 0)
            {
                Move(0, 0, movementBounds, speedScale);
                return;
            }

            if (_wanderRemainingSeconds <= 0)
            {
                ChooseWanderDirection();
            }

            double previousX = PositionX;
            double previousY = PositionY;
            Move(_wanderDirectionX * WanderSpeedMultiplier,
                _wanderDirectionY * WanderSpeedMultiplier, movementBounds,
                speedScale);
            if (PositionX == previousX && PositionY == previousY)
            {
                TurnTowardArenaCenter(field);
                Move(_wanderDirectionX * WanderSpeedMultiplier,
                    _wanderDirectionY * WanderSpeedMultiplier, movementBounds,
                    speedScale);
            }
        }

        private Point ClampToMovementBounds(
            ArenaField field,
            Point start, double directionX, double directionY, double distance)
        {
            double radius = Size / 2;
            double minimumX = ArenaField.WallThickness + radius;
            double maximumX = field.WorldWidth - ArenaField.WallThickness - radius;
            double minimumY = ArenaField.WallThickness + radius;
            double maximumY = field.WorldHeight - ArenaField.WallThickness - radius;
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

        private void TurnTowardArenaCenter(ArenaField field)
        {
            double directionX = field.WorldWidth / 2 - PositionX;
            double directionY = field.WorldHeight / 2 - PositionY;
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
            _wanderRemainingSeconds = MinimumWanderDurationSeconds
                + Random.Shared.NextDouble() * WanderDurationVarianceSeconds;
        }

        private void FacePlayer(double offsetX, double offsetY)
        {
            Facing = Math.Abs(offsetX) >= Math.Abs(offsetY)
                ? (offsetX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (offsetY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
        }

        private double GetMinimumSeparation(Entity target)
        {
            return (Size + target.Size) / 2;
        }
    }
}
