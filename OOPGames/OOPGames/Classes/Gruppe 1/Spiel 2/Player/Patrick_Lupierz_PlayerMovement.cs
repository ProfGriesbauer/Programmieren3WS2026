using System;

namespace OOPGames
{
    /// <summary>
    /// Steuert die Bewegung der Spielfigur über das Kachelraster inkl. Kollisionsprüfung.
    /// Ein Tastendruck startet einen Schritt; ob danach weitergelaufen wird, entscheidet der
    /// aktuell gehaltene Tastenzustand, der im Framework-Tick abgefragt wird.
    /// </summary>
    // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: WASD-Reparatur. Bisher hing das
    // Weiterlaufen an der Tastenwiederholung des Betriebssystems (ca. 500 ms Verzögerung nach dem ersten
    // Schritt = Ruckeln) und gepufferte Wiederholungen liefen nach dem Loslassen noch einen Schritt weiter.
    // Jetzt: Weiterlaufen nur, solange die Taste gehalten wird; gepuffert wird nur ein Richtungswechsel.
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

        // Entwickler: Patrick Lupierz
        // Aufgabe: Tastendruck verarbeiten. Während eines Schritts wird nur ein Richtungswechsel gemerkt;
        //          Wiederholungen derselben Richtung übernimmt die Abfrage des gehaltenen Zustands.
        public void RequestStep(G1C_Direction direction)
        {
            if (_player.IsMoving)
            {
                if (direction != _player.Facing) _queuedDirection = direction;
                return;
            }
            TryStartStep(direction);
        }

        public void ClearQueue()
        {
            _queuedDirection = null;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Einen Tick weiterlaufen. Nach einem abgeschlossenen Schritt läuft die Figur nur weiter,
        //          wenn ein Richtungswechsel gemerkt wurde oder eine Richtungstaste gehalten wird.
        //          heldDirection == null (z. B. in Tests) bedeutet: keine Taste gehalten.
        public void Update(Func<G1C_Direction?> heldDirection = null)
        {
            if (!_player.AdvanceStep()) return;

            G1C_Direction? next = _queuedDirection;
            _queuedDirection = null;
            if (!next.HasValue && heldDirection != null) next = heldDirection();
            if (next.HasValue) TryStartStep(next.Value);
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
