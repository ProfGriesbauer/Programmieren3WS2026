using System;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class MirrorImage : NonPlayerCharacter
    {
        public const double SpawnStationaryDurationSeconds = 0.5;
        public const double SpawnInvulnerabilityDurationSeconds = 3;
        private double _spawnStationaryRemainingSeconds =
            SpawnStationaryDurationSeconds;
        private double _spawnInvulnerabilityRemainingSeconds =
            SpawnInvulnerabilityDurationSeconds;

        public MirrorImage(double x, double y, double speed, double size)
            : base(x, y, 1, speed, size, Brushes.DodgerBlue, CombatTeam.Player)
        {
            CanBePushed = false;
            BlocksMovement = false;
            IsInvulnerable = true;
            // Mirror images wander at full speed and change direction every 1.5-4 s.
            WanderSpeedFactor = 1;
            WanderMinSeconds = 1.5;
            WanderMaxSeconds = 4;
            ChooseWanderDirection();
        }

        public double SpawnStationaryRemainingSeconds
        {
            get { return _spawnStationaryRemainingSeconds; }
        }
        public double SpawnInvulnerabilityRemainingSeconds
        {
            get { return _spawnInvulnerabilityRemainingSeconds; }
        }

        // Continues the spawn phase of a saved image (standing still, protected).
        public void RestoreSpawnTimers(double stationarySeconds, double invulnerabilitySeconds)
        {
            _spawnStationaryRemainingSeconds = Math.Max(0, stationarySeconds);
            _spawnInvulnerabilityRemainingSeconds = Math.Max(0, invulnerabilitySeconds);
            IsInvulnerable = _spawnInvulnerabilityRemainingSeconds > 0;
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

            AdvanceBehaviorTimers(ArenaField.GameTickDurationSeconds * speedScale);
            Wander(field, speedScale, false);
        }

        protected override void OnGameTimeAdvanced(double deltaSeconds)
        {
            base.OnGameTimeAdvanced(deltaSeconds);
            _spawnStationaryRemainingSeconds = Math.Max(
                0, _spawnStationaryRemainingSeconds - deltaSeconds);
            _spawnInvulnerabilityRemainingSeconds = Math.Max(
                0, _spawnInvulnerabilityRemainingSeconds - deltaSeconds);
            IsInvulnerable = _spawnInvulnerabilityRemainingSeconds > 0;
        }
    }
}
