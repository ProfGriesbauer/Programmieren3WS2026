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
                // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Teppich vor dem Bett, eigene Seitenwände
                case '.':
                    if (IsRug(x, y)) return "tile_rug_home_" + G1C_TileMask.Of(x, y, IsRug);
                    return (x + y) % 2 == 0 ? "tile_home_floor" : "tile_home_floor_alt";
                case 'E': return "tile_home_exit";
                default:
                    if (y == 0) return "tile_home_wall";
                    return "tile_wallside_home_" + (x == 0 ? "l" : x == Width - 1 ? "r" : "b");
            }
        }

        // Entwickler: Patrick Lupierz – Teppichfläche im Zuhause (nur Bodengrafik, bleibt begehbar)
        private bool IsRug(int x, int y)
        {
            return x >= 3 && x <= 6 && y >= 6 && y <= 8 && GetTile(x, y) == '.';
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            return Layout[y][x];
        }
    }
}
