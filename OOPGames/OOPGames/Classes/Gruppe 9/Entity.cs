using System;
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

        protected Entity(double positionX, double positionY, int maxHitPoints,
            double maxSpeed, double size, Brush color)
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

        public void Move(double directionX, double directionY, Rect movementBounds)
        {
            double length = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (length == 0 || currentSpeed == 0 || !IsAlive)
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
                : Math.Max(minimumX, Math.Min(maximumX, PositionX + directionX * currentSpeed));
            double nextY = minimumY > maximumY
                ? movementBounds.Top + movementBounds.Height / 2
                : Math.Max(minimumY, Math.Min(maximumY, PositionY + directionY * currentSpeed));

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
            if (!IsAlive)
            {
                return;
            }

            CurrentHitPoints -= damage;
            CheckAndKill();
        }
    }
}
