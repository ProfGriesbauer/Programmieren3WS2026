namespace OOPGames
{
    public enum G1C_Direction { Down, Up, Left, Right }

    /// <summary>Hilfsfunktionen für Laufrichtungen.</summary>
    public static class G1C_DirectionHelper
    {
        public static int DeltaX(G1C_Direction d)
        {
            return d == G1C_Direction.Left ? -1 : (d == G1C_Direction.Right ? 1 : 0);
        }

        public static int DeltaY(G1C_Direction d)
        {
            return d == G1C_Direction.Up ? -1 : (d == G1C_Direction.Down ? 1 : 0);
        }

        public static G1C_Direction Opposite(G1C_Direction d)
        {
            switch (d)
            {
                case G1C_Direction.Up: return G1C_Direction.Down;
                case G1C_Direction.Down: return G1C_Direction.Up;
                case G1C_Direction.Left: return G1C_Direction.Right;
                default: return G1C_Direction.Left;
            }
        }

        /// <summary>Name der Richtung in Sprite-IDs ("down", "up", "left", "right").</summary>
        public static string ToSpriteName(G1C_Direction d)
        {
            return d.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Die Spielfigur. Bewegt sich kachelweise; während eines Schritts wird zwischen
    /// Start- und Zielkachel interpoliert, damit die Bewegung flüssig aussieht.
    /// </summary>
    public class G1C_Player
    {
        public const int TicksPerStep = 5; // 5 x 40 ms = 200 ms pro Kachel

        private int _fromX;
        private int _fromY;

        public string Skin { get { return "player"; } }
        public int TileX { get; private set; }
        public int TileY { get; private set; }
        public G1C_Direction Facing { get; set; }
        public bool IsMoving { get; private set; }
        public int StepProgress { get; private set; }
        public int StepCount { get; private set; }

        public G1C_Player(int tileX, int tileY, G1C_Direction facing)
        {
            TileX = _fromX = tileX;
            TileY = _fromY = tileY;
            Facing = facing;
        }

        private double Progress { get { return IsMoving ? StepProgress / (double)TicksPerStep : 1.0; } }

        /// <summary>Position in Kacheln inkl. Zwischenschritt (für das Zeichnen).</summary>
        public double VisualX { get { return _fromX + (TileX - _fromX) * Progress; } }
        public double VisualY { get { return _fromY + (TileY - _fromY) * Progress; } }

        public int FacingTileX { get { return TileX + G1C_DirectionHelper.DeltaX(Facing); } }
        public int FacingTileY { get { return TileY + G1C_DirectionHelper.DeltaY(Facing); } }

        public void BeginStep(int targetX, int targetY)
        {
            _fromX = TileX;
            _fromY = TileY;
            TileX = targetX;
            TileY = targetY;
            StepProgress = 0;
            IsMoving = true;
        }

        /// <summary>Einen Tick weiterlaufen. Gibt true zurück, wenn der Schritt abgeschlossen ist.</summary>
        public bool AdvanceStep()
        {
            if (!IsMoving) return false;
            StepProgress++;
            if (StepProgress < TicksPerStep) return false;

            IsMoving = false;
            _fromX = TileX;
            _fromY = TileY;
            StepCount++;
            return true;
        }
    }
}
