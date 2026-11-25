using System;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class MirrorImage : NonPlayerCharacter
    {
        public const double SpawnStationaryDurationSeconds = 0.5;
        public const double SpawnInvulnerabilityDurationSeconds = 3;
        private const double MinimumWanderDurationSeconds = 1.5;
        private const double WanderDurationVarianceSeconds = 2.5;
        private double _spawnStationaryRemainingSeconds =
            SpawnStationaryDurationSeconds;
        private double _spawnInvulnerabilityRemainingSeconds =
            SpawnInvulnerabilityDurationSeconds;
        private double _wanderRemainingSeconds;
        private double _wanderDirectionX;
        private double _wanderDirectionY;

        public MirrorImage(double x, double y, double speed, double size)
            : base(x, y, 1, speed, size, Brushes.DodgerBlue, CombatTeam.Player)
        {
            CanBePushed = false;
            IsInvulnerable = true;
            ChooseWanderDirection();
        }

        public override void UpdateBehavior(ArenaField field)
        {
            UpdateBehavior(field, 1);
        }

        public override void UpdateBehavior(ArenaField field, double speedScale)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            if (!IsAlive)
            {
                Move(0, 0, field.MovementArea, speedScale);
                return;
            }

            if (_spawnStationaryRemainingSeconds > 0)
            {
                Move(0, 0, field.MovementArea, speedScale);
                return;
            }

            _wanderRemainingSeconds -= ArenaField.GameTickDurationSeconds * speedScale;
            if (_wanderRemainingSeconds <= 0)
            {
                ChooseWanderDirection();
            }

            double previousX = PositionX;
            double previousY = PositionY;
            Move(_wanderDirectionX, _wanderDirectionY, field.MovementArea, speedScale);
            if (PositionX == previousX && PositionY == previousY)
            {
                ChooseWanderDirection();
                Move(_wanderDirectionX, _wanderDirectionY, field.MovementArea, speedScale);
            }
        }

        protected override void OnGameTimeAdvanced(double deltaSeconds)
        {
            _spawnStationaryRemainingSeconds = Math.Max(
                0, _spawnStationaryRemainingSeconds - deltaSeconds);
            _spawnInvulnerabilityRemainingSeconds = Math.Max(
                0, _spawnInvulnerabilityRemainingSeconds - deltaSeconds);
            IsInvulnerable = _spawnInvulnerabilityRemainingSeconds > 0;
        }

        private void ChooseWanderDirection()
        {
            double angle = Random.Shared.NextDouble() * Math.PI * 2;
            _wanderDirectionX = Math.Cos(angle);
            _wanderDirectionY = Math.Sin(angle);
            _wanderRemainingSeconds = MinimumWanderDurationSeconds
                + Random.Shared.NextDouble() * WanderDurationVarianceSeconds;
        }
    }
}
