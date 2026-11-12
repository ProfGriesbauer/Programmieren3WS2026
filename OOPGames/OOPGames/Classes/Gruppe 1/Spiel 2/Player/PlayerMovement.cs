namespace OOPGames
{
    /// <summary>
    /// Steuert die Bewegung der Spielfigur über das Kachelraster inkl. Kollisionsprüfung.
    /// Tastendrücke während eines Schritts werden gepuffert, damit gedrückt gehaltene
    /// Tasten (Tastenwiederholung des Betriebssystems) zu einer flüssigen Bewegung führen.
    /// </summary>
    public class G1C_PlayerMovement
    {
        private readonly G1C_Player _player;
        private readonly G1C_CollisionManager _collision;
        private G1C_Direction? _queuedDirection;

        public G1C_PlayerMovement(G1C_Player player, G1C_CollisionManager collision)
        {
            _player = player;
            _collision = collision;
        }

        public void RequestStep(G1C_Direction direction)
        {
            if (_player.IsMoving)
            {
                _queuedDirection = direction;
                return;
            }
            TryStartStep(direction);
        }

        public void ClearQueue()
        {
            _queuedDirection = null;
        }

        public void Update()
        {
            if (_player.AdvanceStep() && _queuedDirection.HasValue)
            {
                G1C_Direction next = _queuedDirection.Value;
                _queuedDirection = null;
                TryStartStep(next);
            }
        }

        private void TryStartStep(G1C_Direction direction)
        {
            // Wie bei Pokémon: die Figur dreht sich immer, läuft aber nur, wenn das Feld frei ist
            _player.Facing = direction;
            int targetX = _player.TileX + G1C_DirectionHelper.DeltaX(direction);
            int targetY = _player.TileY + G1C_DirectionHelper.DeltaY(direction);
            if (!_collision.IsBlocked(targetX, targetY))
            {
                _player.BeginStep(targetX, targetY);
            }
        }
    }
}
