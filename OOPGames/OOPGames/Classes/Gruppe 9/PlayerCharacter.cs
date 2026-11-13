using System.Diagnostics;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PlayerCharacter : Entity
    {
        private const double AttackAnimationDurationMilliseconds = 200;
        private long _attackStartTimestamp;

        public PlayerCharacter(double x, double y)
            : base(x, y, 100, 15, 34, Brushes.DodgerBlue)
        {
        }

        public PlayerFacing AttackFacing { get; private set; } = PlayerFacing.Front;
        public bool IsAttacking
        {
            get
            {
                return _attackStartTimestamp != 0
                    && Stopwatch.GetElapsedTime(_attackStartTimestamp).TotalMilliseconds
                        < AttackAnimationDurationMilliseconds;
            }
        }

        public void StartAttack()
        {
            AttackFacing = Facing;
            _attackStartTimestamp = Stopwatch.GetTimestamp();
        }
    }
}