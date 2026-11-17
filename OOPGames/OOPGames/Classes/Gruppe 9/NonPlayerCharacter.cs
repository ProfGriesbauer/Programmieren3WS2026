using System.Windows.Media;

namespace OOPGames
{
    public abstract class NonPlayerCharacter : Entity
    {
        protected NonPlayerCharacter(double positionX, double positionY, int maxHitPoints,
            double maxSpeed, double size, Brush color,
            CombatTeam team = CombatTeam.Neutral)
            : base(positionX, positionY, maxHitPoints, maxSpeed, size, color, team)
        {
        }

        public abstract void UpdateBehavior(ArenaField field);
    }
}
