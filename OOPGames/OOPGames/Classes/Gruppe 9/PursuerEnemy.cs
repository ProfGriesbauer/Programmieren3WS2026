using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // The enemy of the arena: wanders until it sees the player or a mirror image (or was
    // alerted by a hit), then moves according to its behaviour type (chase to contact, keep
    // a distance, stand still, flee when hurt) and attacks with one of its kind's attacks.
    // Its values come from its EnemyType; the constants below are the standard enemy's.
    public sealed class PursuerEnemy : NonPlayerCharacter
    {
        public const double AttackRange = 60;
        public const int AttackDamagePerHit = 10;
        public const double HitboxSize = EnemyType.DefaultSize;
        // The painter draws the 128-pixel enemy frame 2 * Size tall.
        private const double FrameHeightInSizes = 2;
        // Ranged enemies hold their distance within this tolerance instead of jittering.
        private const double RangedDistanceTolerance = 12;
        // How far ahead a fleeing enemy plans (debug view); it keeps fleeing anyway.
        private const double FleePlanningDistance = 200;
        // Hopping kinds: whether a hop is under way and how long the phase still lasts.
        private bool _hopping;
        private double _hopPhaseRemaining;
        // Kinds with a random target keep it until it dies or RetargetSeconds pass.
        private Entity _randomTarget;
        private double _retargetRemaining;
        // An attack with a windup waits here until its steps have run (see
        // TryAttackAmong); a self-destructing attack kills the enemy once it is over.
        private EnemyAttack _windupAttack;
        private double _windupElapsed;
        private int _windupStep;
        private double _windupDirectionX;
        private double _windupDirectionY;
        private bool _selfDestructPending;

        public PursuerEnemy(double x, double y, double playerSpeed)
            : this(x, y, playerSpeed, EnemyType.Standard)
        {
        }

        public PursuerEnemy(double x, double y, double playerSpeed, EnemyType type)
            : base(x, y, type, playerSpeed, Brushes.Crimson, CombatTeam.Enemies)
        {
            EquippedWeapon = Attacks[0].Weapon;
            ChooseWanderDirection();
            // Hoppers start somewhere in their pause so several do not hop in step.
            _hopPhaseRemaining = Random.Shared.NextDouble() * HopPauseSeconds;
        }

        // Hopping kinds only move during a hop; walking kinds always may.
        public bool CanMoveNow
        {
            get { return MovementStyle != EnemyMovementStyle.Hop || _hopping; }
        }

        // A coward below its flee threshold runs away and does not attack.
        public bool IsFleeing
        {
            get
            {
                return Behavior == EnemyBehavior.Coward
                    && CurrentHitPoints * 100.0 < FleeBelowHealthPercent * MaxHitPoints;
            }
        }

        // Hitbox around the visible pixels of the kind's sprite sheet.
        public override Rect HitboxOffset
        {
            get { return GetHitboxOffsetForSize(Size, Type.SpriteSheet); }
        }

        // Hitbox of an enemy of the given size before it exists, e.g. for spawn checks.
        public static Rect GetHitboxOffsetForSize(double size)
        {
            return GetHitboxOffsetForSize(size, EnemyType.DefaultSpriteSheet);
        }

        public static Rect GetHitboxOffsetForSize(double size, string spriteSheet)
        {
            return EntityHitboxCatalog.GetEnemyHitboxOffset(
                    spriteSheet, size * FrameHeightInSizes)
                ?? new Rect(-size / 2, -size / 2, size, size);
        }

        public override Point GetPlannedDestination(ArenaField field)
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
                GetMovementGoal(target, out double directionX, out double directionY,
                    out double travelDistance);
                return travelDistance <= 0
                    ? currentPosition
                    : ClampToMovementBounds(field, currentPosition,
                        directionX, directionY, travelDistance);
            }

            if (WanderRemainingSeconds <= 0 || Behavior == EnemyBehavior.Turret)
            {
                return currentPosition;
            }

            // Speeds are distances per regular tick; convert the remaining game time to ticks.
            double travel = EffectiveSpeed * WanderSpeedFactor
                * WanderRemainingSeconds / ArenaField.GameTickDurationSeconds;
            return ClampToMovementBounds(field, currentPosition,
                WanderDirectionX, WanderDirectionY, travel);
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

        public override bool TryAttackTargetInRange(
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
            return TryAttackAmong(new[] { target }, combatSystem, false);
        }

        // Runs a pending windup or self-destruct, otherwise looks for an attack. Kinds
        // whose attacks may be set off by others than their target (attacks on everyone,
        // other trigger sources, contact triggers) look at every entity of the arena.
        public override bool TryAttack(ArenaField field, ArenaCombatSystem combatSystem)
        {
            if (combatSystem == null)
            {
                throw new ArgumentNullException(nameof(combatSystem));
            }
            if (_selfDestructPending)
            {
                // The attacker dies once its own attack (e.g. the explosion) is over.
                if (!combatSystem.IsAttacking(this))
                {
                    _selfDestructPending = false;
                    SelfDestruct();
                }
                return false;
            }
            if (_windupAttack != null)
            {
                if (!IsAlive || _windupElapsed < _windupAttack.WindupSeconds)
                {
                    return false;
                }
                EnemyAttack attack = _windupAttack;
                _windupAttack = null;
                return Fire(attack, _windupDirectionX, _windupDirectionY, combatSystem);
            }

            bool looksAround = false;
            foreach (EnemyAttack attack in Attacks)
            {
                looksAround |= attack.Weapon.TargetingMode == WeaponTargetingMode.Everyone
                    || attack.TriggeredBy != EnemyTriggerSource.Targets
                    || attack.Trigger == EnemyAttackTrigger.Contact;
            }
            return looksAround
                ? TryAttackAmong(GetEntities(field), combatSystem, true)
                : base.TryAttack(field, combatSystem);
        }

        public bool IsWindingUp { get { return _windupAttack != null; } }

        // Of the ready attacks, each takes the nearest entity that may set it off and fits
        // its trigger (distance to the hitbox or hitbox contact); one of these pairs is
        // chosen by weight. Searching the whole arena skips invulnerable entities except
        // for contact triggers (a mine still goes off under a protected player).
        private bool TryAttackAmong(IEnumerable<Entity> entities,
            ArenaCombatSystem combatSystem, bool skipInvulnerable)
        {
            if (!IsAlive || IsFleeing || _windupAttack != null || _selfDestructPending
                || combatSystem.IsAttacking(this))
            {
                return false;
            }

            List<Entity> living = new List<Entity>();
            foreach (Entity entity in entities)
            {
                if (entity != null && entity.IsAlive && !ReferenceEquals(entity, this))
                {
                    living.Add(entity);
                }
            }

            Point center = new Point(PositionX, PositionY);
            List<EnemyAttack> chosenAttacks = new List<EnemyAttack>();
            List<Entity> chosenTargets = new List<Entity>();
            double totalWeight = 0;
            foreach (EnemyAttack attack in Attacks)
            {
                if (combatSystem.GetCooldownRemaining(this, attack.Weapon) > 0)
                {
                    continue;
                }
                Entity nearest = null;
                double nearestDistance = double.MaxValue;
                foreach (Entity entity in living)
                {
                    bool contact = attack.Trigger == EnemyAttackTrigger.Contact;
                    if (!attack.MayBeTriggeredBy(entity)
                        || (skipInvulnerable && !contact && entity.IsInvulnerable))
                    {
                        continue;
                    }
                    double distance = AttackGeometry.DistanceToRect(center, entity.Hitbox);
                    bool fits = contact
                        ? AttackGeometry.RectsOverlap(Hitbox, entity.Hitbox)
                        : attack.FitsDistance(distance);
                    if (fits && distance < nearestDistance)
                    {
                        nearest = entity;
                        nearestDistance = distance;
                    }
                }
                if (nearest != null)
                {
                    chosenAttacks.Add(attack);
                    chosenTargets.Add(nearest);
                    totalWeight += attack.Weight;
                }
            }
            if (chosenAttacks.Count == 0)
            {
                return false;
            }

            int chosen = chosenAttacks.Count - 1;
            double roll = Random.Shared.NextDouble() * totalWeight;
            for (int i = 0; i < chosenAttacks.Count; i++)
            {
                roll -= chosenAttacks[i].Weight;
                if (roll < 0)
                {
                    chosen = i;
                    break;
                }
            }

            EnemyAttack selected = chosenAttacks[chosen];
            double directionX = chosenTargets[chosen].PositionX - PositionX;
            double directionY = chosenTargets[chosen].PositionY - PositionY;
            if (selected.WindupSteps.Count > 0)
            {
                // The attack fires after its windup (e.g. button down, "Hello").
                _windupAttack = selected;
                _windupElapsed = 0;
                _windupStep = -1;
                _windupDirectionX = directionX;
                _windupDirectionY = directionY;
                FaceAttack(directionX, directionY);
                AdvanceWindup(0);
                return true;
            }
            return Fire(selected, directionX, directionY, combatSystem);
        }

        private bool Fire(EnemyAttack attack, double directionX, double directionY,
            ArenaCombatSystem combatSystem)
        {
            if (directionX == 0 && directionY == 0)
            {
                // A target exactly on the enemy's centre (standing on a mine) gives no
                // direction; the combat system needs one.
                directionX = AttackDirectionX;
                directionY = AttackDirectionY == 0 && AttackDirectionX == 0 ? 1 : AttackDirectionY;
            }
            if (!combatSystem.TryStartAttack(this, attack.Weapon, directionX, directionY))
            {
                CurrentWindupClip = null;
                return false;
            }
            FaceAttack(directionX, directionY);
            RequestSound(attack.Sound);
            if (attack.KillsAttacker)
            {
                // The last windup clip stays visible until the enemy is gone.
                _selfDestructPending = true;
            }
            else
            {
                CurrentWindupClip = null;
            }
            return true;
        }

        // Shows the windup step that is due and starts its sound once.
        private void AdvanceWindup(double deltaSeconds)
        {
            if (_windupAttack == null)
            {
                return;
            }
            _windupElapsed += deltaSeconds;
            double stepEnd = 0;
            int step = _windupAttack.WindupSteps.Count - 1;
            for (int i = 0; i < _windupAttack.WindupSteps.Count; i++)
            {
                stepEnd += _windupAttack.WindupSteps[i].Seconds;
                if (_windupElapsed < stepEnd)
                {
                    step = i;
                    break;
                }
            }
            if (step != _windupStep)
            {
                _windupStep = step;
                CurrentWindupClip = _windupAttack.WindupSteps[step].Clip;
                RequestSound(_windupAttack.WindupSteps[step].Sound);
            }
        }

        // Every other living entity of the arena: player, mirror images and enemies.
        private IEnumerable<Entity> GetEntities(ArenaField field)
        {
            if (field.Player != null && field.Player.IsAlive)
            {
                yield return field.Player;
            }
            foreach (MirrorImage mirrorImage in field.MirrorImages)
            {
                if (mirrorImage.IsAlive)
                {
                    yield return mirrorImage;
                }
            }
            foreach (NonPlayerCharacter enemy in field.Enemies)
            {
                if (enemy.IsAlive && !ReferenceEquals(enemy, this))
                {
                    yield return enemy;
                }
            }
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

            double deltaSeconds = ArenaField.GameTickDurationSeconds * speedScale;
            AdvanceBehaviorTimers(deltaSeconds);
            UpdateDevouring(deltaSeconds);
            UpdateHopping(deltaSeconds);
            if (IsAlive)
            {
                AdvanceWindup(deltaSeconds);
            }
            _retargetRemaining -= deltaSeconds;
            if (!IsAlive)
            {
                Move(0, 0, field.MovementArea, speedScale);
                return;
            }

            Entity target = GetCurrentTarget(field);
            Rect movementBounds = field.MovementArea;
            if (!CanMoveNow || IsDevouring || IsWindingUp || _selfDestructPending)
            {
                // Between hops, while eating and while an attack winds up or explodes the
                // enemy stays where it is.
                if (target != null)
                {
                    FacePlayer(target.PositionX - PositionX, target.PositionY - PositionY);
                }
                Move(0, 0, movementBounds, speedScale);
                return;
            }
            if (target == null)
            {
                if (Behavior == EnemyBehavior.Turret)
                {
                    Move(0, 0, movementBounds, speedScale);
                }
                else
                {
                    Wander(field, speedScale, true);
                }
                return;
            }

            double offsetX = target.PositionX - PositionX;
            double offsetY = target.PositionY - PositionY;
            FacePlayer(offsetX, offsetY);
            GetMovementGoal(target, out double directionX, out double directionY,
                out double goalDistance);
            if (EffectiveSpeed == 0 || goalDistance <= 0)
            {
                Move(0, 0, movementBounds, speedScale);
                return;
            }

            double travel = Math.Min(currentSpeed, goalDistance);
            Move(directionX * travel / currentSpeed, directionY * travel / currentSpeed,
                movementBounds, speedScale);
            if (IsFleeing)
            {
                FacePlayer(directionX, directionY);
            }
            else
            {
                FacePlayer(offsetX, offsetY);
            }
        }

        // Where the behaviour type wants to go now: a unit direction and the distance to
        // cover (0 = stay).
        private void GetMovementGoal(Entity target,
            out double directionX, out double directionY, out double travelDistance)
        {
            directionX = 0;
            directionY = 0;
            travelDistance = 0;
            double offsetX = target.PositionX - PositionX;
            double offsetY = target.PositionY - PositionY;
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distance == 0 || Behavior == EnemyBehavior.Turret)
            {
                return;
            }

            double towardX = offsetX / distance;
            double towardY = offsetY / distance;
            double contactDistance = GetMinimumSeparation(target, offsetX, offsetY);
            if (IsFleeing)
            {
                directionX = -towardX;
                directionY = -towardY;
                travelDistance = FleePlanningDistance;
                return;
            }

            if (Behavior == EnemyBehavior.Ranged)
            {
                double desired = Math.Max(contactDistance, PreferredDistance);
                if (distance > desired + RangedDistanceTolerance)
                {
                    directionX = towardX;
                    directionY = towardY;
                    travelDistance = distance - desired;
                }
                else if (distance < desired - RangedDistanceTolerance)
                {
                    directionX = -towardX;
                    directionY = -towardY;
                    travelDistance = desired - distance;
                }
                return;
            }

            if (distance > contactDistance)
            {
                directionX = towardX;
                directionY = towardY;
                travelDistance = distance - contactDistance;
            }
        }

        private void UpdateHopping(double deltaSeconds)
        {
            if (MovementStyle != EnemyMovementStyle.Hop)
            {
                return;
            }
            _hopPhaseRemaining -= deltaSeconds;
            while (_hopPhaseRemaining <= 0)
            {
                _hopping = !_hopping;
                _hopPhaseRemaining += _hopping ? HopSeconds : Math.Max(0.01, HopPauseSeconds);
            }
        }

        private Entity GetCurrentTarget(ArenaField field)
        {
            if (TargetSelection == EnemyTargetSelection.RandomEntity)
            {
                CurrentTarget = GetRandomTarget(field);
                return CurrentTarget;
            }

            Entity target = FindClosestVisibleTarget(field, CurrentSightRange);
            if (target == null && HasSeenPlayer)
            {
                target = FindClosestVisibleTarget(field, SightRange);
            }

            HasSeenPlayer = target != null;

            if (IsAlerted)
            {
                // Once the player was in sight the normal pursuit takes over; until then
                // the alerted enemy walks straight to the player wherever it is.
                if (!field.Player.IsAlive || IsWithinSight(field.Player))
                {
                    EndAlert();
                }
                else if (target == null)
                {
                    target = field.Player;
                }
            }
            CurrentTarget = target;
            return target;
        }

        // A random living entity anywhere (not this enemy); kept until it dies, leaves the
        // arena or RetargetSeconds have passed.
        private Entity GetRandomTarget(ArenaField field)
        {
            bool stillThere = false;
            if (_randomTarget != null && _randomTarget.IsAlive && _retargetRemaining > 0)
            {
                foreach (Entity entity in GetEntities(field))
                {
                    stillThere |= ReferenceEquals(entity, _randomTarget);
                }
            }
            if (!stillThere)
            {
                List<Entity> candidates = new List<Entity>(GetEntities(field));
                _randomTarget = candidates.Count == 0
                    ? null
                    : candidates[Random.Shared.Next(candidates.Count)];
                _retargetRemaining = RetargetSeconds;
            }
            return _randomTarget;
        }

        // Chooses a new random target at the next update (tests, later: events).
        internal void ForceRetarget()
        {
            _retargetRemaining = 0;
        }

        private bool IsWithinSight(Entity target)
        {
            double offsetX = target.PositionX - PositionX;
            double offsetY = target.PositionY - PositionY;
            return offsetX * offsetX + offsetY * offsetY
                <= CurrentSightRange * CurrentSightRange;
        }

        private Entity FindClosestVisibleTarget(ArenaField field, double sightRange)
        {
            Entity closestTarget = null;
            double closestDistanceSquared = sightRange * sightRange;
            ConsiderTarget(field.Player, ref closestTarget, ref closestDistanceSquared);
            if (TargetsMirrorImages)
            {
                foreach (MirrorImage mirrorImage in field.MirrorImages)
                {
                    ConsiderTarget(
                        mirrorImage, ref closestTarget, ref closestDistanceSquared);
                }
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

        private Point ClampToMovementBounds(
            ArenaField field,
            Point start, double directionX, double directionY, double distance)
        {
            Rect hitbox = HitboxOffset;
            double minimumX = ArenaField.WallThickness - hitbox.Left;
            double maximumX = field.WorldWidth - ArenaField.WallThickness - hitbox.Right;
            double minimumY = ArenaField.WallThickness - hitbox.Top;
            double maximumY = field.WorldHeight - ArenaField.WallThickness - hitbox.Bottom;
            double destinationX = start.X + directionX * distance;
            double destinationY = start.Y + directionY * distance;
            return new Point(
                Math.Max(minimumX, Math.Min(maximumX, destinationX)),
                Math.Max(minimumY, Math.Min(maximumY, destinationY)));
        }

        private void FacePlayer(double offsetX, double offsetY)
        {
            Facing = Math.Abs(offsetX) >= Math.Abs(offsetY)
                ? (offsetX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (offsetY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
        }

        // Distance between the centers at which the two hitboxes touch when the enemy walks
        // straight towards its target (the player or a mirror image).
        private double GetMinimumSeparation(
            Entity target, double offsetX, double offsetY)
        {
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distance == 0)
            {
                return 0;
            }
            return AttackGeometry.GetContactDistance(
                HitboxOffset, target.HitboxOffset, offsetX / distance, offsetY / distance);
        }
    }
}
