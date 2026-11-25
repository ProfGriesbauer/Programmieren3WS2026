using System;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PlayerCharacter : Entity
    {
        // Game time, so the attack pose lasts longer during slow motion.
        public const double AttackAnimationDurationSeconds = 0.2;
        private double _attackAnimationRemainingSeconds;
        private double _facingHoldRemainingSeconds;
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
        // Facing whose hand launches the next projectile; set by ArenaField while starting
        // an attack, otherwise the facing follows the projectile direction.
        internal PlayerFacing? LaunchFacing { get; set; }
        
        // Animation profile determines hand positions
        public string AnimationProfileName { get; set; } = "BoxingKnight";

        // The painter draws the 128-pixel character frame 4 * Size tall.
        private const double FrameHeightInSizes = 4;
        private const double FramePixels = 128;

        // Hitbox around the visible pixels of the chosen character's texture.
        public override Rect HitboxOffset
        {
            get
            {
                return EntityHitboxCatalog.GetHitboxOffset(
                        AnimationProfileName, Size * FrameHeightInSizes)
                    ?? base.HitboxOffset;
            }
        }

        public static PlayerFacing GetFacingForDirection(double directionX, double directionY)
        {
            return AttackGeometry.GetFacingForDirection(directionX, directionY);
        }

        public override Vector GetHandOffset(PlayerFacing facing)
        {
            return GetRightHandOffset(facing);
        }

        // True when the right hand, and the weapon in it, is hidden behind the body: seen
        // from behind, and from the side whose sprite does not show the right arm (BoxingKnight
        // shows it when looking left, Kurt when looking right).
        public bool IsRightHandBehindBody(PlayerFacing facing)
        {
            if (facing == PlayerFacing.Back)
            {
                return true;
            }
            return AnimationProfileName == "Kurt"
                ? facing == PlayerFacing.Left
                : facing == PlayerFacing.Right;
        }

        // Offset of the right hand from the body center in world units for a facing.
        public Vector GetRightHandOffset(PlayerFacing facing)
        {
            double unitsPerPixel = Size * FrameHeightInSizes / FramePixels;
            return GetRightHandFramePixels(AnimationProfileName, facing) * unitsPerPixel;
        }

        // Right hand of each character sprite in frame pixels relative to the frame center,
        // measured on the idle frames. Seen from the front the right hand is on the left of
        // the screen, seen from behind on the right; seen from the right side it is on the far
        // side, behind the body.
        private static Vector GetRightHandFramePixels(string profileName, PlayerFacing facing)
        {
            if (profileName == "Kurt")
            {
                // Kurt is a cactus: the weapon sits just below the tip of his raised right arm,
                // the lower of his two arms. Seen from the right that arm is drawn on the
                // trunk; seen from the left it is hidden behind the trunk at the same height.
                switch (facing)
                {
                    case PlayerFacing.Back: return new Vector(16.5, -26);
                    case PlayerFacing.Right: return new Vector(-2, -26);
                    case PlayerFacing.Left: return new Vector(-2, -27);
                    default: return new Vector(-21.5, -27);
                }
            }

            // BoxingKnight: the fists at the lower end of the arms.
            switch (facing)
            {
                case PlayerFacing.Back: return new Vector(21, 10);
                case PlayerFacing.Right: return new Vector(2, 16);
                case PlayerFacing.Left: return new Vector(-2, 16);
                default: return new Vector(-21, 10);
            }
        }

        // Weapons with a sprite are held in the right hand, so their projectiles start in
        // front of it (arrows at the tip of the nocked arrow); the facing follows the attack
        // direction.
        public override Point GetProjectileLaunchPoint(
            Weapon weapon, double directionX, double directionY, double defaultDistance)
        {
            if (weapon == null || weapon.Appearance == null)
            {
                return base.GetProjectileLaunchPoint(
                    weapon, directionX, directionY, defaultDistance);
            }

            double distance = weapon.Appearance.ProjectileLaunchDistance ?? defaultDistance;
            PlayerFacing facing = LaunchFacing
                ?? GetFacingForDirection(directionX, directionY);
            return new Point(PositionX, PositionY)
                + GetRightHandOffset(facing)
                + new Vector(directionX, directionY) * distance;
        }
        public bool IsAttacking
        {
            get { return _attackAnimationRemainingSeconds > 1e-9; }
        }

        // While an attack holds the facing, walking does not turn the player away from the
        // direction of the strike.
        protected override bool CanTurnWhileMoving
        {
            get { return _facingHoldRemainingSeconds <= 1e-9; }
        }

        public void StartAttack()
        {
            StartAttack(Facing, AttackAnimationDurationSeconds);
        }

        // Turns the player into the direction of the strike and keeps that facing for
        // holdSeconds of game time.
        public void StartAttack(PlayerFacing facing, double holdSeconds)
        {
            if (holdSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(holdSeconds));
            }

            Facing = facing;
            AttackFacing = facing;
            _attackAnimationRemainingSeconds = AttackAnimationDurationSeconds;
            _facingHoldRemainingSeconds = Math.Max(
                AttackAnimationDurationSeconds, holdSeconds);
        }

        protected override void OnGameTimeAdvanced(double deltaSeconds)
        {
            _attackAnimationRemainingSeconds = Math.Max(
                0, _attackAnimationRemainingSeconds - deltaSeconds);
            _facingHoldRemainingSeconds = Math.Max(
                0, _facingHoldRemainingSeconds - deltaSeconds);
        }
    }
}