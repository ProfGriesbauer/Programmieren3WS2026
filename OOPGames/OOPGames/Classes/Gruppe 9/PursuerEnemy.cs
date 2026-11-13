using System;
using System.Diagnostics;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class PursuerEnemy : NonPlayerCharacter
    {
        private const int ContactDamage = 10;
        private static readonly TimeSpan ContactDamageInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan AttackVisualDuration = TimeSpan.FromMilliseconds(250);
        private long _lastContactDamageTimestamp;
        private long _attackTimestamp;

        public PursuerEnemy(double x, double y, double playerSpeed)
            : base(x, y, 30, playerSpeed * 0.75, 34, Brushes.Crimson)
        {
        }

        public bool IsAttacking
        {
            get
            {
                return _attackTimestamp != 0
                    && Stopwatch.GetElapsedTime(_attackTimestamp) < AttackVisualDuration;
            }
        }

        public void TryDamagePlayerOnContact(PlayerCharacter player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            double offsetX = player.PositionX - PositionX;
            double offsetY = player.PositionY - PositionY;
            double contactDistance = (Size + player.Size) / 2;
            if (!IsAlive || !player.IsAlive
                || offsetX * offsetX + offsetY * offsetY > contactDistance * contactDistance)
            {
                _lastContactDamageTimestamp = 0;
                return;
            }

            long now = Stopwatch.GetTimestamp();
            if (_lastContactDamageTimestamp != 0
                && Stopwatch.GetElapsedTime(_lastContactDamageTimestamp, now)
                    < ContactDamageInterval)
            {
                return;
            }

            player.TakeDamage(ContactDamage);
            _lastContactDamageTimestamp = now;
            _attackTimestamp = now;
        }

        public override void UpdateBehavior(ArenaField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            PlayerCharacter player = field.Player;
            if (!IsAlive || !player.IsAlive)
            {
                Move(0, 0, new System.Windows.Rect(
                    ArenaField.WallThickness, ArenaField.WallThickness,
                    ArenaField.MapWidth - 2 * ArenaField.WallThickness,
                    ArenaField.MapHeight - 2 * ArenaField.WallThickness));
                return;
            }

            double offsetX = player.PositionX - PositionX;
            double offsetY = player.PositionY - PositionY;
            FacePlayer(offsetX, offsetY);

            System.Windows.Rect movementBounds = new System.Windows.Rect(
                ArenaField.WallThickness, ArenaField.WallThickness,
                ArenaField.MapWidth - 2 * ArenaField.WallThickness,
                ArenaField.MapHeight - 2 * ArenaField.WallThickness);
            if (currentSpeed == 0)
            {
                Move(0, 0, movementBounds);
                return;
            }

            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            double stoppingDistance = (Size + player.Size) / 2;
            if (distance <= stoppingDistance)
            {
                Move(0, 0, movementBounds);
                return;
            }

            double travel = Math.Min(currentSpeed, distance - stoppingDistance);
            Move(offsetX / distance * travel / currentSpeed,
                offsetY / distance * travel / currentSpeed, movementBounds);
            FacePlayer(offsetX, offsetY);
        }

        private void FacePlayer(double offsetX, double offsetY)
        {
            Facing = Math.Abs(offsetX) >= Math.Abs(offsetY)
                ? (offsetX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (offsetY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
        }
    }
}
