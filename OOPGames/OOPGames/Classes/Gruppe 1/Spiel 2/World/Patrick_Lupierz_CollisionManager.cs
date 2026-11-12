namespace OOPGames
{
    /// <summary>Prüft, ob eine Kachel betreten werden darf (Wände, Möbel, NPCs).</summary>
    public class G1C_CollisionManager
    {
        private readonly G1C_CasinoWorld _world;

        public G1C_CollisionManager(G1C_CasinoWorld world)
        {
            _world = world;
        }

        public bool IsBlocked(int x, int y)
        {
            if (!_world.Map.IsWalkable(x, y)) return true;
            G1C_WorldObject obj = _world.ObjectAt(x, y);
            return obj != null && obj.Solid;
        }
    }
}
