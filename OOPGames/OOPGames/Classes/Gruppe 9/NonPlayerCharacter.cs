using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // Everything the game steers itself (enemies, mirror images). An NPC is configured by
    // an EnemyType (Assets/Enemies/Types, dev tool "Gegner-Editor"); NPCs created without
    // one use the standard values and may adjust them (e.g. mirror images wander faster).
    // Shared here: perception (sight, alert after a hit), wandering, the current target
    // and the attack hook. Subclasses decide how they move in UpdateBehavior.
    public abstract class NonPlayerCharacter : Entity
    {
        private double _wanderRemainingSeconds;
        private double _wanderDirectionX;
        private double _wanderDirectionY;
        private double _alertRemainingSeconds;
        private double _regeneratedFraction;
        private double _idleSoundRemaining;
        private readonly List<DevourGrip> _devouring = new List<DevourGrip>();

        protected NonPlayerCharacter(double positionX, double positionY, int maxHitPoints,
            double maxSpeed, double size, Brush color,
            CombatTeam team = CombatTeam.Neutral)
            : base(positionX, positionY, maxHitPoints, maxSpeed, size, color, team)
        {
            ApplyTypeValues(EnemyType.Standard);
        }

        // Creates the NPC from an enemy kind: hit points, speed (relative to the player's
        // base speed), size and pushability come from the kind.
        protected NonPlayerCharacter(double positionX, double positionY, EnemyType type,
            double playerSpeed, Brush color, CombatTeam team)
            : base(positionX, positionY,
                (type ?? throw new ArgumentNullException(nameof(type))).MaxHitPoints,
                playerSpeed * type.SpeedFactor, type.Size, color, team)
        {
            CanBePushed = type.CanBePushed;
            BlocksMovement = type.BlocksMovement;
            ApplyTypeValues(type);
        }

        public EnemyType Type { get; private set; }
        public string TypeId { get { return Type.Id; } }

        // ---- configuration (from the kind; subclasses may adjust) ----
        public double SightRange { get; protected set; }
        public double SightBonusAfterDetection { get; protected set; }
        public bool TargetsMirrorImages { get; protected set; }
        public double AlertDurationSeconds { get; protected set; }
        public double AlertRadius { get; protected set; }
        public bool WarnsOthers { get; protected set; }
        public bool CanBeWarned { get; protected set; }
        public double WanderSpeedFactor { get; protected set; }
        public double WanderMinSeconds { get; protected set; }
        public double WanderMaxSeconds { get; protected set; }
        public double RegenerationPerSecond { get; protected set; }
        // Attacks the NPC chooses from (never empty for NPCs created from a kind).
        public IReadOnlyList<EnemyAttack> Attacks { get; protected set; }
        public EnemyBehavior Behavior { get; protected set; }
        public double PreferredDistance { get; protected set; }
        public double FleeBelowHealthPercent { get; protected set; }
        public EnemyTargetSelection TargetSelection { get; protected set; }
        public double RetargetSeconds { get; protected set; }
        public EnemyMovementStyle MovementStyle { get; protected set; }
        public double HopSeconds { get; protected set; }
        public double HopPauseSeconds { get; protected set; }
        // Played now and then (null: never), every IdleSoundMin..MaxSeconds of game time.
        public string IdleSound { get; protected set; }
        public double IdleSoundMinSeconds { get; protected set; }
        public double IdleSoundMaxSeconds { get; protected set; }
        // Direction of the last attack or devour (for directional attack animations).
        public double AttackDirectionX { get; protected set; }
        public double AttackDirectionY { get; protected set; } = 1;

        // Raised when the NPC has pulled an entity in and killed it (see StartDevouring).
        public event Action<NonPlayerCharacter, Entity> Devoured;
        // A sound group of ArenaAudio.EnemySoundNames should play (attack, windup step).
        public event Action<NonPlayerCharacter, string> SoundRequested;
        // The NPC killed itself (e.g. a mine after its explosion); no one scores.
        public event Action<NonPlayerCharacter> SelfDestructed;

        // Clip of the running attack windup (e.g. "Hello"), or null.
        public string CurrentWindupClip { get; protected set; }

        protected void RequestSound(string sound)
        {
            if (!String.IsNullOrEmpty(sound))
            {
                SoundRequested?.Invoke(this, sound);
            }
        }

        protected void SelfDestruct()
        {
            TakeFatalDamage();
            if (!IsAlive)
            {
                SelfDestructed?.Invoke(this);
            }
        }

        public bool IsDevouring { get { return _devouring.Count > 0; } }

        // ---- state ----
        public bool HasSeenPlayer { get; protected set; }
        public Entity CurrentTarget { get; protected set; }

        public double CurrentSightRange
        {
            get { return HasSeenPlayer ? SightRange + SightBonusAfterDetection : SightRange; }
        }

        public bool IsAlerted { get { return _alertRemainingSeconds > 0; } }
        public double AlertRemainingSeconds { get { return _alertRemainingSeconds; } }

        public abstract void UpdateBehavior(ArenaField field);

        public virtual void UpdateBehavior(ArenaField field, double speedScale)
        {
            UpdateBehavior(field);
        }

        // Where the NPC is heading (debug view); NPCs without a plan stay where they are.
        public virtual Point GetPlannedDestination(ArenaField field)
        {
            return new Point(PositionX, PositionY);
        }

        // Attacks the target when it is in reach; NPCs that do not attack return false.
        public virtual bool TryAttackTargetInRange(
            Entity target, ArenaCombatSystem combatSystem)
        {
            return false;
        }

        // Called by the arena every tick: attacks whatever the NPC wants to attack now.
        public virtual bool TryAttack(ArenaField field, ArenaCombatSystem combatSystem)
        {
            return TryAttackTargetInRange(CurrentTarget, combatSystem);
        }

        // Devour: holds the victim (it cannot move), pulls it to this NPC within
        // pullSeconds and kills it there. Invulnerable or already held victims are ignored.
        public void StartDevouring(Entity victim, double pullSeconds)
        {
            if (victim == null || ReferenceEquals(victim, this) || !IsAlive
                || !victim.IsAlive || victim.IsInvulnerable
                || _devouring.Exists(grip => ReferenceEquals(grip.Victim, victim)))
            {
                return;
            }
            victim.ApplyMovementStatus("devoured", "Verschlungen", 0, pullSeconds + 0.2);
            _devouring.Add(new DevourGrip(victim, pullSeconds));
            FaceAttack(victim.PositionX - PositionX, victim.PositionY - PositionY);
        }

        // Moves held victims towards the mouth and kills those that arrived. A dead NPC
        // lets go of its victims.
        protected void UpdateDevouring(double deltaSeconds)
        {
            if (_devouring.Count == 0)
            {
                return;
            }
            if (!IsAlive)
            {
                _devouring.Clear();
                return;
            }

            for (int i = _devouring.Count - 1; i >= 0; i--)
            {
                DevourGrip grip = _devouring[i];
                Entity victim = grip.Victim;
                if (!victim.IsAlive || victim.IsInvulnerable)
                {
                    _devouring.RemoveAt(i);
                    continue;
                }

                grip.RemainingSeconds -= deltaSeconds;
                double progress = grip.PullSeconds <= 0
                    ? 1
                    : Math.Min(1, 1 - Math.Max(0, grip.RemainingSeconds) / grip.PullSeconds);
                double offsetX = grip.StartX - PositionX;
                double offsetY = grip.StartY - PositionY;
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                double mouth = Math.Min(distance, Size * 0.4);
                double endX = distance == 0 ? PositionX : PositionX + offsetX / distance * mouth;
                double endY = distance == 0 ? PositionY : PositionY + offsetY / distance * mouth;
                victim.SetPosition(grip.StartX + (endX - grip.StartX) * progress,
                    grip.StartY + (endY - grip.StartY) * progress);
                FaceAttack(offsetX, offsetY);

                if (grip.RemainingSeconds <= 0)
                {
                    _devouring.RemoveAt(i);
                    victim.TakeFatalDamage();
                    if (!victim.IsAlive)
                    {
                        Devoured?.Invoke(this, victim);
                    }
                }
            }
        }

        protected void FaceAttack(double directionX, double directionY)
        {
            double length = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (length > 0)
            {
                AttackDirectionX = directionX / length;
                AttackDirectionY = directionY / length;
            }
        }

        private sealed class DevourGrip
        {
            public DevourGrip(Entity victim, double pullSeconds)
            {
                Victim = victim;
                PullSeconds = pullSeconds;
                RemainingSeconds = pullSeconds;
                StartX = victim.PositionX;
                StartY = victim.PositionY;
            }

            public Entity Victim { get; }
            public double PullSeconds { get; }
            public double RemainingSeconds { get; set; }
            public double StartX { get; }
            public double StartY { get; }
        }

        // A direct hit tells the NPC where the player is (see ArenaField.AlertEnemyAndNeighbours).
        public void Alert()
        {
            _alertRemainingSeconds = AlertDurationSeconds;
        }

        // Ends the alert, e.g. once the player was in sight.
        protected void EndAlert()
        {
            _alertRemainingSeconds = 0;
        }

        // Save games restore a running alert.
        public void RestoreAlert(double remainingSeconds)
        {
            _alertRemainingSeconds = Math.Max(0, Math.Min(AlertDurationSeconds, remainingSeconds));
        }

        // Regeneration heals silently (no healing events, particles or sounds).
        protected override void OnGameTimeAdvanced(double deltaSeconds)
        {
            if (RegenerationPerSecond <= 0 || !IsAlive || CurrentHitPoints >= MaxHitPoints)
            {
                _regeneratedFraction = 0;
                return;
            }
            _regeneratedFraction += RegenerationPerSecond * deltaSeconds;
            int healed = (int)_regeneratedFraction;
            if (healed > 0)
            {
                _regeneratedFraction -= healed;
                RestoreHitPoints(CurrentHitPoints + healed);
            }
        }

        // Counts the alert and the wander timer down by game time (follows slow motion).
        protected void AdvanceBehaviorTimers(double deltaSeconds)
        {
            if (IdleSound != null && IsAlive)
            {
                _idleSoundRemaining -= deltaSeconds;
                if (_idleSoundRemaining <= 0)
                {
                    RequestSound(IdleSound);
                    ScheduleIdleSound();
                }
            }
            _wanderRemainingSeconds -= deltaSeconds;
            if (_alertRemainingSeconds > 0)
            {
                _alertRemainingSeconds = Math.Max(0, _alertRemainingSeconds - deltaSeconds);
            }
        }

        protected double WanderRemainingSeconds { get { return _wanderRemainingSeconds; } }
        protected double WanderDirectionX { get { return _wanderDirectionX; } }
        protected double WanderDirectionY { get { return _wanderDirectionY; } }

        // Walks in the wander direction at WanderSpeedFactor and picks a new direction when
        // the time is up. Blocked by a wall it turns to the arena center (or, with
        // turnToCenterWhenBlocked false, to a new random direction) and tries once more.
        protected void Wander(ArenaField field, double speedScale, bool turnToCenterWhenBlocked)
        {
            Rect movementBounds = field.MovementArea;
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
            Move(_wanderDirectionX * WanderSpeedFactor,
                _wanderDirectionY * WanderSpeedFactor, movementBounds, speedScale);
            if (PositionX == previousX && PositionY == previousY)
            {
                if (turnToCenterWhenBlocked)
                {
                    TurnTowardArenaCenter(field);
                }
                else
                {
                    ChooseWanderDirection();
                }
                Move(_wanderDirectionX * WanderSpeedFactor,
                    _wanderDirectionY * WanderSpeedFactor, movementBounds, speedScale);
            }
        }

        protected void ChooseWanderDirection()
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
            _wanderRemainingSeconds = WanderMinSeconds
                + Random.Shared.NextDouble() * (WanderMaxSeconds - WanderMinSeconds);
        }

        private void ApplyTypeValues(EnemyType type)
        {
            Type = type;
            SightRange = type.SightRange;
            SightBonusAfterDetection = type.SightBonusAfterDetection;
            TargetsMirrorImages = type.TargetsMirrorImages;
            AlertDurationSeconds = type.AlertDurationSeconds;
            AlertRadius = type.AlertRadius;
            WarnsOthers = type.WarnsOthers;
            CanBeWarned = type.CanBeWarned;
            WanderSpeedFactor = type.WanderSpeedFactor;
            WanderMinSeconds = type.WanderMinSeconds;
            WanderMaxSeconds = type.WanderMaxSeconds;
            RegenerationPerSecond = type.RegenerationPerSecond;
            DamageTakenMultiplier = type.DamageTakenMultiplier;
            KnockbackResistance = type.KnockbackResistance;
            SetStatusImmunities(type.StatusImmunities);
            Attacks = type.Attacks;
            Behavior = type.Behavior;
            PreferredDistance = type.PreferredDistance;
            FleeBelowHealthPercent = type.FleeBelowHealthPercent;
            TargetSelection = type.TargetSelection;
            RetargetSeconds = type.RetargetSeconds;
            MovementStyle = type.MovementStyle;
            HopSeconds = type.HopSeconds;
            HopPauseSeconds = type.HopPauseSeconds;
            IdleSound = type.IdleSound;
            IdleSoundMinSeconds = type.IdleSoundMinSeconds;
            IdleSoundMaxSeconds = type.IdleSoundMaxSeconds;
            ScheduleIdleSound();
        }

        private void ScheduleIdleSound()
        {
            _idleSoundRemaining = IdleSoundMinSeconds
                + Random.Shared.NextDouble() * (IdleSoundMaxSeconds - IdleSoundMinSeconds);
        }
    }
}
