//P06.11.2026
using System;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // Common data and movement behavior shared by objects in the game world.
    public class Entities
    {
        // Position is stored in map coordinates; the painter converts it to screen coordinates.
        public int HitPoints { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        // All entities share a facing direction, initially facing forward.
        public PlayerFacing Facing { get; set; } = PlayerFacing.Front;
        // The painter uses movement state to switch between idle and walk animations.
        public bool IsMoving { get; private set; }
        // Distance this entity moves on each movement update.
        public double Speed { get; set; }
        public double Size { get; }
        public Brush Color { get; }
        public bool IsAlive { get; private set; } = true;


        // Store the basic properties needed to position and draw an entity.
        public Entities(double x, double y, double size, Brush color, int hitPoints, double speed)
        {
            X = x;
            Y = y;
            Size = size;
            Color = color;
            HitPoints = hitPoints;
            Speed = speed;
        }

        // Move by the requested direction, keeping this entity inside the supplied arena bounds.
        public void Move(double directionX, double directionY, Rect movementBounds)
        {
            double dx = directionX * Speed;
            double dy = directionY * Speed;

            // Normalize diagonal movement so it is not faster than movement on one axis.
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

            // Face the dominant direction of movement; horizontal facing wins on diagonals.
            if (dx < 0) Facing = PlayerFacing.Left;
            else if (dx > 0) Facing = PlayerFacing.Right;
            else if (dy < 0) Facing = PlayerFacing.Back;
            else Facing = PlayerFacing.Front;

            // Treat the position as the entity's center so its full size remains in the arena.
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