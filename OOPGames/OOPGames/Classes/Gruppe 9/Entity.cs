using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public abstract class Entity : IEntities
    {
        private int _currentHitPoints;
        private int _maxHitPoints;
        private double _maxSpeed;
        private double _currentSpeed;
        private double _damageFlashRemainingSeconds;
        private readonly List<ActiveStatusEffect> _activeStatusEffects =
            new List<ActiveStatusEffect>();
        private readonly List<ActiveDamageOverTimeEffect> _damageOverTimeEffects =
            new List<ActiveDamageOverTimeEffect>();

        protected Entity(double positionX, double positionY, int maxHitPoints,
            double maxSpeed, double size, Brush color,
            CombatTeam team = CombatTeam.Neutral)
        {
            if (maxHitPoints <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHitPoints));
            }
            if (maxSpeed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSpeed));
            }
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            PositionX = positionX;
            PositionY = positionY;
            _maxHitPoints = maxHitPoints;
            _currentHitPoints = maxHitPoints;
            MaxSpeed = maxSpeed;
            currentSpeed = maxSpeed;
            Size = size;
            Color = color ?? throw new ArgumentNullException(nameof(color));
            Team = team;
        }

        public int MaxHitPoints
        {
            get { return _maxHitPoints; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maxHitPoints = value;
                CurrentHitPoints = Math.Min(_currentHitPoints, _maxHitPoints);
            }
        }

        public int CurrentHitPoints
        {
            get { return _currentHitPoints; }
            set
            {
                _currentHitPoints = Math.Max(0, Math.Min(value, MaxHitPoints));
                if (_currentHitPoints == 0)
                {
                    IsAlive = false;
                    IsMoving = false;
                }
            }
        }

        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _maxSpeed = value;
                _currentSpeed = Math.Min(_currentSpeed, _maxSpeed);
            }
        }

        public double currentSpeed
        {
            get { return _currentSpeed; }
            set { _currentSpeed = Math.Max(0, Math.Min(value, MaxSpeed)); }
        }

        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public PlayerFacing Facing { get; set; } = PlayerFacing.Front;
        public bool IsMoving { get; private set; }
        public double Size { get; }
        public Brush Color { get; }
        public bool IsAlive { get; private set; } = true;
        public bool IsInvulnerable { get; set; }
        public double DamageFlashIntensity
        {
            get
            {
                return Math.Max(0, Math.Min(
                    1, _damageFlashRemainingSeconds / DamageFlashDurationSeconds));
            }
        }
        public CombatTeam Team { get; protected set; }
        public Weapon EquippedWeapon { get; set; }
        internal Weapon EffectWeaponContext { get; set; }
        public bool HasActiveAttack { get; internal set; }
        public AttackVisualState CurrentAttackVisual { get; internal set; }
        public IReadOnlyList<ActiveStatusEffect> ActiveStatusEffects
        {
            get { return _activeStatusEffects.AsReadOnly(); }
        }
        public IReadOnlyList<ActiveDamageOverTimeEffect> ActiveDamageOverTimeEffects
        {
            get { return _damageOverTimeEffects.AsReadOnly(); }
        }
        public double EffectiveSpeed
        {
            get { return currentSpeed * GetStatusSpeedMultiplier(); }
        }
        public string StatusSummary
        {
            get
            {
                if (_activeStatusEffects.Count == 0
                    && _damageOverTimeEffects.Count == 0)
                {
                    return "None";
                }

                List<string> statuses = new List<string>();
                foreach (ActiveStatusEffect effect in _activeStatusEffects)
                {
                    statuses.Add(effect.Name + " "
                        + effect.RemainingSeconds.ToString("0.0") + "s");
                }
                foreach (ActiveDamageOverTimeEffect effect in _damageOverTimeEffects)
                {
                    statuses.Add(effect.Name + " "
                        + effect.RemainingSeconds.ToString("0.0") + "s");
                }
                return string.Join(", ", statuses);
            }
        }

        public void Move(double directionX, double directionY, Rect movementBounds)
        {
            Move(directionX, directionY, movementBounds, 1);
        }

        public void Move(
            double directionX, double directionY, Rect movementBounds,
            double speedScale)
        {
            if (double.IsNaN(speedScale) || double.IsInfinity(speedScale)
                || speedScale < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(speedScale));
            }

            double length = Math.Sqrt(directionX * directionX + directionY * directionY);
            double speed = EffectiveSpeed * speedScale;
            if (length == 0 || speed == 0 || !IsAlive)
            {
                IsMoving = false;
                return;
            }

            directionX /= Math.Max(1, length);
            directionY /= Math.Max(1, length);

            if (directionX < 0) Facing = PlayerFacing.Left;
            else if (directionX > 0) Facing = PlayerFacing.Right;
            else if (directionY < 0) Facing = PlayerFacing.Back;
            else Facing = PlayerFacing.Front;

            double radius = Size / 2;
            double minimumX = movementBounds.Left + radius;
            double maximumX = movementBounds.Right - radius;
            double minimumY = movementBounds.Top + radius;
            double maximumY = movementBounds.Bottom - radius;
            double nextX = minimumX > maximumX
                ? movementBounds.Left + movementBounds.Width / 2
                : Math.Max(minimumX, Math.Min(maximumX, PositionX + directionX * speed));
            double nextY = minimumY > maximumY
                ? movementBounds.Top + movementBounds.Height / 2
                : Math.Max(minimumY, Math.Min(maximumY, PositionY + directionY * speed));

            IsMoving = nextX != PositionX || nextY != PositionY;
            PositionX = nextX;
            PositionY = nextY;
        }

        public void CheckAndKill()
        {
            if (CurrentHitPoints == 0)
            {
                IsAlive = false;
                IsMoving = false;
            }
        }

        public void TakeDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }
            if (!IsAlive || IsInvulnerable)
            {
                return;
            }

            int previousHitPoints = CurrentHitPoints;
            CurrentHitPoints -= damage;
            if (CurrentHitPoints < previousHitPoints)
            {
                _damageFlashRemainingSeconds = DamageFlashDurationSeconds;
            }
            CheckAndKill();
        }

        public int Heal(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
            if (!IsAlive)
            {
                return 0;
            }

            int previousHitPoints = CurrentHitPoints;
            CurrentHitPoints = (int)Math.Min(
                MaxHitPoints, (long)CurrentHitPoints + amount);
            return CurrentHitPoints - previousHitPoints;
        }

        public void ApplyMovementStatus(
            string id, string name, double speedMultiplier, double durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A status id is required.", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A status name is required.", nameof(name));
            }
            if (speedMultiplier < 0 || speedMultiplier > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(speedMultiplier));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }
            if (!IsAlive)
            {
                return;
            }

            for (int i = 0; i < _activeStatusEffects.Count; i++)
            {
                ActiveStatusEffect activeEffect = _activeStatusEffects[i];
                if (activeEffect.Id == id)
                {
                    double remainingSeconds = Math.Max(
                        activeEffect.RemainingSeconds, durationSeconds);
                    if (speedMultiplier < activeEffect.SpeedMultiplier)
                    {
                        _activeStatusEffects[i] = new ActiveStatusEffect(
                            id, name, speedMultiplier, remainingSeconds);
                    }
                    else
                    {
                        activeEffect.RemainingSeconds = remainingSeconds;
                    }
                    return;
                }
            }

            _activeStatusEffects.Add(new ActiveStatusEffect(
                id, name, speedMultiplier, durationSeconds));
        }

        public void ApplyDamageOverTime(
            string id, string name, double damagePerSecond,
            double durationSeconds, double spreadRadius, Entity source,
            Weapon sourceWeapon = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A damage effect id is required.", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A damage effect name is required.", nameof(name));
            }
            if (damagePerSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerSecond));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }
            if (spreadRadius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spreadRadius));
            }
            if (!IsAlive)
            {
                return;
            }

            foreach (ActiveDamageOverTimeEffect activeEffect in _damageOverTimeEffects)
            {
                if (activeEffect.Id == id)
                {
                    activeEffect.DamagePerSecond = Math.Max(
                        activeEffect.DamagePerSecond, damagePerSecond);
                    activeEffect.RemainingSeconds = Math.Max(
                        activeEffect.RemainingSeconds, durationSeconds);
                    activeEffect.SpreadRadius = Math.Max(
                        activeEffect.SpreadRadius, spreadRadius);
                    activeEffect.Source = source;
                    activeEffect.SourceWeapon = sourceWeapon
                        ?? (source == null ? null
                            : source.EffectWeaponContext ?? source.EquippedWeapon);
                    return;
                }
            }

            _damageOverTimeEffects.Add(new ActiveDamageOverTimeEffect(
                id, name, damagePerSecond, durationSeconds, spreadRadius,
                source, sourceWeapon ?? (source == null ? null
                    : source.EffectWeaponContext ?? source.EquippedWeapon)));
        }

        public void UpdateStatusEffects(double deltaSeconds)
        {
            if (deltaSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            _damageFlashRemainingSeconds = Math.Max(
                0, _damageFlashRemainingSeconds - deltaSeconds);
            for (int i = _activeStatusEffects.Count - 1; i >= 0; i--)
            {
                ActiveStatusEffect effect = _activeStatusEffects[i];
                effect.RemainingSeconds -= deltaSeconds;
                if (effect.RemainingSeconds <= 0)
                {
                    _activeStatusEffects.RemoveAt(i);
                }
            }

            if (EffectiveSpeed == 0)
            {
                IsMoving = false;
            }
        }

        internal void RemoveExpiredDamageOverTimeEffects()
        {
            for (int i = _damageOverTimeEffects.Count - 1; i >= 0; i--)
            {
                if (_damageOverTimeEffects[i].IsExpired)
                {
                    _damageOverTimeEffects.RemoveAt(i);
                }
            }
        }

        private double GetStatusSpeedMultiplier()
        {
            double multiplier = 1;
            foreach (ActiveStatusEffect effect in _activeStatusEffects)
            {
                multiplier = Math.Min(multiplier, effect.SpeedMultiplier);
            }
            return multiplier;
        }

        private const double DamageFlashDurationSeconds = 0.18;
    }
}
