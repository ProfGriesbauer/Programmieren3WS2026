using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public interface IEntities
    {
        int HitPoints { get; set; }
        double X { get; set; }
        double Y { get; set; }
        PlayerFacing Facing { get; set; }
        bool IsMoving { get; }
        double Speed { get; set; }
        double Size { get; }
        Brush Color { get; }
        bool IsAlive { get; }

        void Move(double directionX, double directionY, Rect movementBounds);
        void CheckAndKill();
        void TakeDamage(int damage);
    }
}
