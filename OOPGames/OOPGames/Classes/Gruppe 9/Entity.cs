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
        private double _weaknessDamageMultiplier = 1;
        private double _weaknessRemainingSeconds;
        private Weapon _equippedWeapon;
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
        // Base size of the entity; the painter scales its sprite from it. Collisions and hits
        // use Hitbox, which matches the texture for players and enemies.
        public double Size { get; }
        // Hitbox relative to the position in world units; by default a square of Size.
        public virtual Rect HitboxOffset
        {
            get { return new Rect(-Size / 2, -Size / 2, Size, Size); }
        }
        public Rect Hitbox { get { return GetHitboxAt(PositionX, PositionY); } }

        public Rect GetHitboxAt(double positionX, double positionY)
        {
            Rect offset = HitboxOffset;
            return new Rect(
                positionX + offset.X, positionY + offset.Y, offset.Width, offset.Height);
        }
        public Brush Color { get; }
        public bool IsAlive { get; private set; } = true;
        public bool CanBePushed { get; set; }
        public bool IsInvulnerable { get; set; }
        // Raised with the hit points actually lost, so presentation code (sounds) can react.
        public event Action<Entity, int> Damaged;
        // Raised with the hit points actually restored; from is the entity the life was stolen
        // from (Leach), null for ordinary healing.
        public event Action<Entity, int, Entity> Healed;
        // Raised with the status id ("stun", "freeze", "slow", ...) whenever a movement status
        // is applied or renewed, so presentation code can react (e.g. electric shock sound).
        public event Action<Entity, string> StatusApplied;
        public double DamageFlashIntensity
        {
            get
            {
                return Math.Max(0, Math.Min(
                    1, _damageFlashRemainingSeconds / DamageFlashDurationSeconds));
            }
        }
        public CombatTeam Team { get; protected set; }
        public Weapon EquippedWeapon
        {
            get { return _equippedWeapon; }
            set
            {
                // Only a different weapon restarts the clock; an upgraded copy of the same
                // weapon (same id) keeps it, so a drawn bow stays drawn after an upgrade.
                if (_equippedWeapon == null || value == null
                    || _equippedWeapon.Id != value.Id)
                {
                    WeaponEquippedSeconds = 0;
                }
                _equippedWeapon = value;
            }
        }
        // Game time since the current weapon was equipped; drives e.g. drawing the bow.
        public double WeaponEquippedSeconds { get; private set; }
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
        public bool IsWeakened { get { return _weaknessRemainingSeconds > 0; } }
        public double WeaknessDamageMultiplier { get { return _weaknessDamageMultiplier; } }
        public double WeaknessRemainingSeconds { get { return _weaknessRemainingSeconds; } }
        // Factor for the damage this entity deals; below 1 while it is weakened.
        public double OutgoingDamageMultiplier
        {
            get { return IsWeakened ? _weaknessDamageMultiplier : 1; }
        }
        public string StatusSummary
        {
            get
            {
                if (_activeStatusEffects.Count == 0
                    && _damageOverTimeEffects.Count == 0
                    && !IsWeakened)
                {
                    return "None";
                }

                List<string> statuses = new List<string>();
                if (IsWeakened)
                {
                    statuses.Add("Weakness "
                        + _weaknessRemainingSeconds.ToString("0.0") + "s");
                }
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

            if (CanTurnWhileMoving)
            {
                if (directionX < 0) Facing = PlayerFacing.Left;
                else if (directionX > 0) Facing = PlayerFacing.Right;
                else if (directionY < 0) Facing = PlayerFacing.Back;
                else Facing = PlayerFacing.Front;
            }

            // The whole hitbox stays inside the movement bounds.
            Rect hitbox = HitboxOffset;
            double minimumX = movementBounds.Left - hitbox.Left;
            double maximumX = movementBounds.Right - hitbox.Right;
            double minimumY = movementBounds.Top - hitbox.Top;
            double maximumY = movementBounds.Bottom - hitbox.Bottom;
            double nextX = minimumX > maximumX
                ? movementBounds.Left + movementBounds.Width / 2
                : Math.Max(minimumX, Math.Min(maximumX, PositionX + directionX * speed));
            double nextY = minimumY > maximumY
                ? movementBounds.Top + movementBounds.Height / 2
                : Math.Max(minimumY, Math.Min(maximumY, PositionY + directionY * speed));

            SetPosition(nextX, nextY);
        }

        internal void SetPosition(double positionX, double positionY)
        {
            IsMoving = IsAlive
                && (positionX != PositionX || positionY != PositionY);
            PositionX = positionX;
            PositionY = positionY;
        }

        internal void RestorePosition(
            double positionX, double positionY, bool isMoving)
        {
            PositionX = positionX;
            PositionY = positionY;
            IsMoving = IsAlive && isMoving;
        }

        // Brings a defeated entity back with full hit points and no statuses (respawn).
        public void Revive()
        {
            _activeStatusEffects.Clear();
            _damageOverTimeEffects.Clear();
            _weaknessRemainingSeconds = 0;
            _damageFlashRemainingSeconds = 0;
            IsAlive = true;
            _currentHitPoints = _maxHitPoints;
            IsMoving = false;
        }

        // Restores saved hit points; 0 leaves the entity defeated.
        public void RestoreHitPoints(int hitPoints)
        {
            if (hitPoints <= 0)
            {
                CurrentHitPoints = 0;
                return;
            }
            IsAlive = true;
            _currentHitPoints = Math.Min(hitPoints, _maxHitPoints);
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
            int lostHitPoints = previousHitPoints - CurrentHitPoints;
            if (lostHitPoints > 0)
            {
                _damageFlashRemainingSeconds = DamageFlashDurationSeconds;
            }
            CheckAndKill();
            if (lostHitPoints > 0)
            {
                Damaged?.Invoke(this, lostHitPoints);
            }
        }

        public int Heal(int amount)
        {
            return Heal(amount, null);
        }

        public int Heal(int amount, Entity from)
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
            int healed = CurrentHitPoints - previousHitPoints;
            if (healed > 0)
            {
                Healed?.Invoke(this, healed, from);
            }
            return healed;
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

            StatusApplied?.Invoke(this, id);
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

        // Repeated hits keep the strongest reduction and the longest remaining time.
        public void ApplyWeakness(double damageMultiplier, double durationSeconds)
        {
            if (damageMultiplier < 0 || damageMultiplier > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(damageMultiplier));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }
            if (!IsAlive)
            {
                return;
            }

            _weaknessDamageMultiplier = IsWeakened
                ? Math.Min(_weaknessDamageMultiplier, damageMultiplier)
                : damageMultiplier;
            _weaknessRemainingSeconds = Math.Max(
                _weaknessRemainingSeconds, durationSeconds);
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

            OnGameTimeAdvanced(deltaSeconds);
            _damageFlashRemainingSeconds = Math.Max(
                0, _damageFlashRemainingSeconds - deltaSeconds);
            _weaknessRemainingSeconds = Math.Max(
                0, _weaknessRemainingSeconds - deltaSeconds);
            WeaponEquippedSeconds += deltaSeconds;
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

        // Offset of the hand that holds the weapon for a facing; entities without hands
        // hold weapons at their center.
        public virtual Vector GetHandOffset(PlayerFacing facing)
        {
            return new Vector(0, 0);
        }

        // Point where a projectile of the weapon starts. Entities without hands launch it
        // defaultDistance in front of their center.
        public virtual Point GetProjectileLaunchPoint(
            Weapon weapon, double directionX, double directionY, double defaultDistance)
        {
            return new Point(
                PositionX + directionX * defaultDistance,
                PositionY + directionY * defaultDistance);
        }

        // False while the entity must keep its facing, e.g. during an attack.
        protected virtual bool CanTurnWhileMoving
        {
            get { return true; }
        }

        // Called once per combat update with the scaled game time, so timers of derived
        // entities follow slow motion like every other game system.
        protected virtual void OnGameTimeAdvanced(double deltaSeconds)
        {
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

        private const double DamageFlashDurationSeconds = 0.35;
    }
}
