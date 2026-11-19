using System.Diagnostics;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PlayerCharacter : Entity
    {
        private const double AttackAnimationDurationMilliseconds = 200;
        private long _attackStartTimestamp;
        public const int BaseMaxHitPoints = 100;
        public const double BaseMaxSpeed = 15;

        public PlayerCharacter(double x, double y)
            : base(x, y, BaseMaxHitPoints, BaseMaxSpeed, 34,
                Brushes.DodgerBlue, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.Sword;
            CanBePushed = true;
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