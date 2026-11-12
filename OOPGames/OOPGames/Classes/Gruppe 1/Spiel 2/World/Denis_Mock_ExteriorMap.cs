namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ExteriorMap
    // Entwickler: Denis Mock
    // Beschreibung: Quadratische Stadtkarte mit geschlossener, nicht begehbarer Außengrenze.
    // =====================================================
    public class G1C_ExteriorMap : IG1C_WorldMap
    {
        public const int MapSize = 18;

        private readonly char[,] _tiles;

        public int Width { get { return MapSize; } }
        public int Height { get { return MapSize; } }

        // Entwickler: Denis Mock
        public G1C_ExteriorMap()
        {
            _tiles = CreateLayout();
        }

        // Entwickler: Denis Mock
        public bool IsWalkable(int x, int y)
        {
            char tile = GetTile(x, y);
            return tile != '#' && tile != 'H' && tile != 'C';
        }

        // Entwickler: Denis Mock
        // Die beiden Türfelder führen in die vorhandenen, getrennten Innenräume.
        public G1C_WorldLocation? GetTransitionAt(int x, int y)
        {
            char tile = GetTile(x, y);
            if (tile == 'H') return G1C_WorldLocation.Home;
            if (tile == 'C') return G1C_WorldLocation.Casino;
            return null;
        }

        // Entwickler: Denis Mock
        public string GetTileSpriteId(int x, int y)
        {
            switch (GetTile(x, y))
            {
                case '#': return "tile_city_border";
                case 'r': return "tile_city_road";
                case 's': return "tile_city_sidewalk";
                case 'p': return "tile_city_path";
                case 'H': return "tile_city_home_entry";
                case 'C': return "tile_city_casino_entry";
                default: return (x + y) % 2 == 0 ? "tile_city_grass" : "tile_city_grass_alt";
            }
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            return _tiles[x, y];
        }

        // Entwickler: Denis Mock
        private static char[,] CreateLayout()
        {
            char[,] tiles = new char[MapSize, MapSize];
            for (int y = 0; y < MapSize; y++)
            {
                for (int x = 0; x < MapSize; x++)
                {
                    tiles[x, y] = x == 0 || y == 0 || x == MapSize - 1 || y == MapSize - 1 ? '#' : 'g';
                }
            }

            for (int y = 1; y < MapSize - 1; y++)
            {
                tiles[8, y] = 'r';
                tiles[9, y] = 'r';
            }
            for (int x = 1; x < MapSize - 1; x++)
            {
                tiles[x, 8] = 'r';
                tiles[x, 9] = 'r';
            }

            for (int y = 5; y <= 7; y++)
            {
                tiles[4, y] = 'p';
                tiles[14, y] = 'p';
            }
            for (int y = 10; y <= 15; y++)
            {
                tiles[6, y] = 'p';
                tiles[11, y] = 'p';
            }
            for (int x = 4; x <= 6; x++) tiles[x, 15] = 'p';
            for (int x = 11; x <= 14; x++) tiles[x, 15] = 'p';
            tiles[4, 14] = 'H';
            tiles[14, 14] = 'C';

            return tiles;
        }
    }
}
