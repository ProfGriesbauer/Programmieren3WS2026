//P06.11.2026
using System;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // The controllable entity starts facing forward with 100 hit points.
    public sealed class PlayerCharacter : IEntities
    {
        public int HitPoints { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public PlayerFacing Facing { get; set; } = PlayerFacing.Front;
        public bool IsMoving { get; private set; }
        public double Speed { get; set; }
        public double Size { get; }
        public Brush Color { get; }
        public bool IsAlive { get; private set; } = true;

        // Start coordinates are supplied by the arena field.
        public PlayerCharacter(double x, double y)
        {
            X = x;
            Y = y;
            Size = 34;
            Color = Brushes.DodgerBlue;
            HitPoints = 100;
            Speed = 15;
        }

        public void Move(double directionX, double directionY, Rect movementBounds)
        {
            double dx = directionX * Speed;
            double dy = directionY * Speed;

            if (dx != 0 && dy != 0)
            {
                dx *= 0.7071;
                dy *= 0.7071;
            }

            if (dx == 0 && dy == 0)
            {
                IsMoving = false;
                return;
            }

            if (dx < 0) Facing = PlayerFacing.Left;
            else if (dx > 0) Facing = PlayerFacing.Right;
            else if (dy < 0) Facing = PlayerFacing.Back;
            else Facing = PlayerFacing.Front;

            double radius = Size / 2;
            double nextX = Math.Max(movementBounds.Left + radius,
                Math.Min(movementBounds.Right - radius, X + dx));
            double nextY = Math.Max(movementBounds.Top + radius,
                Math.Min(movementBounds.Bottom - radius, Y + dy));
            IsMoving = nextX != X || nextY != Y;
            X = nextX;
            Y = nextY;
        }

        public void CheckAndKill()
        {
            if (HitPoints <= 0)
            {
                IsAlive = false;
            }
        }

        public void TakeDamage(int damage)
        {
            HitPoints -= damage;
            CheckAndKill();
        }
    }
}