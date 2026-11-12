using System.Windows.Media;

namespace OOPGames
{
    public abstract class NonPlayerCharacter : Entity
    {
        protected NonPlayerCharacter(double positionX, double positionY, int maxHitPoints,
            double maxSpeed, double size, Brush color)
            : base(positionX, positionY, maxHitPoints, maxSpeed, size, color)
        {
        }

        public abstract void UpdateBehavior(ArenaField field);
    }
}
