namespace OOPGames
{
    public enum G1C_TileType { Floor, Wall, WallFace, Exit }

    /// <summary>
    /// Kachelkarte des Casinos (20 x 12 Kacheln).
    /// '#' = Mauer (von oben), 'W' = Wand mit Tapete, '.' = Teppich, 'E' = Ausgang.
    /// </summary>
    public class G1C_CasinoMap
    {
        private static readonly string[] Layout =
        {
            "####################",
            "#WWWWWWWWWWWWWWWWWW#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "#..................#",
            "########EE##########",
        };
        

        private readonly G1C_TileType[,] _tiles;

        public int Width { get; }
        public int Height { get; }

        public G1C_CasinoMap()
        {
            Height = Layout.Length;
            Width = Layout[0].Length;
            _tiles = new G1C_TileType[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _tiles[x, y] = Parse(Layout[y][x]);
                }
            }
        }

        private static G1C_TileType Parse(char c)
        {
            switch (c)
            {
                case '.': return G1C_TileType.Floor;
                case 'W': return G1C_TileType.WallFace;
                case 'E': return G1C_TileType.Exit;
                default: return G1C_TileType.Wall;
            }
        }

        /// <summary>Außerhalb der Karte gilt alles als Mauer (Begrenzung des Bereichs).</summary>
        public G1C_TileType GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return G1C_TileType.Wall;
            return _tiles[x, y];
        }

        public bool IsWalkable(int x, int y)
        {
            return GetTile(x, y) == G1C_TileType.Floor;
        }

        public string GetTileSpriteId(int x, int y)
        {
            switch (GetTile(x, y))
            {
                case G1C_TileType.Floor: return (x + y) % 2 == 0 ? "tile_floor" : "tile_floor_alt";
                case G1C_TileType.WallFace: return "tile_wall_face";
                case G1C_TileType.Exit: return "tile_exit";
                default: return "tile_wall";
            }
        }
    }
}
