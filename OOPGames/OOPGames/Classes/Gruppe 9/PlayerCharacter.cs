using System;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PlayerCharacter : Entity
    {
        // Game time, so the attack pose lasts longer during slow motion.
        private const double AttackAnimationDurationSeconds = 0.2;
        private double _attackAnimationRemainingSeconds;
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
            get { return _attackAnimationRemainingSeconds > 1e-9; }
        }

        public void StartAttack()
        {
            AttackFacing = Facing;
            _attackAnimationRemainingSeconds = AttackAnimationDurationSeconds;
        }

        protected override void OnGameTimeAdvanced(double deltaSeconds)
        {
            _attackAnimationRemainingSeconds = Math.Max(
                0, _attackAnimationRemainingSeconds - deltaSeconds);
        }
    }
}