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
        public bool IsBlocked(int x, int y)
        {
            if (!_world.Grid.IsWalkable(x, y)) return true;
            G1C_WorldObject obj = _world.ObjectAt(x, y);
            return obj != null && obj.Solid;
        }
    }
}
