//P06.11.2026
using System.Windows.Media;

namespace OOPGames
{
    // The controllable entity starts facing forward with 100 hit points.
    public sealed class PlayerCharacter : Entities
    {
        // Start coordinates are supplied by the arena field.
        public PlayerCharacter(double x, double y)
            : base(x, y, 34, Brushes.DodgerBlue, 100, 15)
        {
        }
    }
}