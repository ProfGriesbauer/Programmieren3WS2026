using System.Windows.Media;

namespace OOPGames
{
    public sealed class PlayerCharacter : Entity
    {
        public PlayerCharacter(double x, double y)
            : base(x, y, 100, 15, 34, Brushes.DodgerBlue)
        {
        }
    }
}