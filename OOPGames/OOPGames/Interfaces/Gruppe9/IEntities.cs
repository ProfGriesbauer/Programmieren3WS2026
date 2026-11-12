using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public interface IEntities
    {
        int MaxHitPoints { get; set; }
        int CurrentHitPoints { get; set; }
        double MaxSpeed { get; set; }
        double currentSpeed { get; set; }
        double PositionX { get; set; }
        double PositionY { get; set; }
        PlayerFacing Facing { get; set; }
        bool IsMoving { get; }
        double Size { get; }
        Brush Color { get; }
        bool IsAlive { get; }

        void Move(double directionX, double directionY, Rect movementBounds);
        void CheckAndKill();
        void TakeDamage(int damage);
    }
}
