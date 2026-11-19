namespace OOPGames
{
    /// <summary>Prüft, ob eine Kachel betreten werden darf (Wände, Möbel, NPCs).</summary>
    public class G1C_CollisionManager
    {
        private readonly IG1C_WorldScene _world;

        // Änderung durch: Denis Mock – Kollisionen nutzen die aktive Szene, nicht nur das Casino.
        public G1C_CollisionManager(IG1C_WorldScene world)
        {
            _world = world ?? throw new System.ArgumentNullException(nameof(world));
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: Fehler "durch Bäume laufen" behoben.
        // ObjectAt liefert nur das oberste Objekt; sitzt ein (nicht massiver) Vogel auf einem Baum, war die
        // Kachel begehbar. Jetzt blockiert jedes massive Objekt auf der Kachel.
        public bool IsBlocked(int x, int y)
        {
            if (!_world.Grid.IsWalkable(x, y)) return true;
            foreach (G1C_WorldObject obj in _world.Objects)
                if (obj.Solid && obj.Occupies(x, y)) return true;
            return false;
        }
    }
}
