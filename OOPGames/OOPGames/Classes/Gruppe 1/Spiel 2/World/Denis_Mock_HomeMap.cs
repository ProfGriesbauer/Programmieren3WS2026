namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_HomeMap
    // Entwickler: Denis Mock
    // Beschreibung: Begehbarer Grundriss des Spielerhauses.
    // =====================================================
    public class G1C_HomeMap : IG1C_WorldMap
    {
        private static readonly string[] Layout =
        {
            "############",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#..........#",
            "#####E######",
        };

        public int Width { get { return Layout[0].Length; } }
        public int Height { get { return Layout.Length; } }

        // Entwickler: Denis Mock
        public bool IsWalkable(int x, int y)
        {
            return GetTile(x, y) == '.';
        }

        // Entwickler: Denis Mock
        public G1C_WorldLocation? GetTransitionAt(int x, int y)
        {
            return GetTile(x, y) == 'E' ? G1C_WorldLocation.Exterior : (G1C_WorldLocation?)null;
        }

        // Entwickler: Denis Mock
        public string GetTileSpriteId(int x, int y)
        {
            switch (GetTile(x, y))
            {
                case '.': return (x + y) % 2 == 0 ? "tile_home_floor" : "tile_home_floor_alt";
                case 'E': return "tile_home_exit";
                default: return "tile_home_wall";
            }
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            return Layout[y][x];
        }
    }
}
