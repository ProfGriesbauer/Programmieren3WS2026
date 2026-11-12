using System;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // 180x180-Kachel-Stadt: genau die hundertfache Fläche der ursprünglichen 18x18-Karte.
    public class G1C_ExteriorMap : IG1C_WorldMap
    {
        public const int MapSize = 180;

        private static readonly int[] StreetLines = { 16, 40, 64, 88, 112, 136, 160 };
        private readonly char[,] _tiles;

        public int Width { get { return MapSize; } }
        public int Height { get { return MapSize; } }

        public int HomeDoorX { get; private set; }
        public int HomeDoorY { get; private set; }
        public int CasinoDoorX { get; private set; }
        public int CasinoDoorY { get; private set; }
        public int HospitalDoorX { get; private set; }
        public int HospitalDoorY { get; private set; }
        public int MarketDoorX { get; private set; }
        public int MarketDoorY { get; private set; }

        // Entwickler: Denis Mock
        public G1C_ExteriorMap()
        {
            _tiles = CreateLayout();
        }

        // Entwickler: Denis Mock
        public bool IsWalkable(int x, int y)
        {
            char tile = GetTile(x, y);
            return tile != '#' && tile != 'w' && tile != 'X' &&
                   tile != 'H' && tile != 'C' && tile != 'K' && tile != 'S';
        }

        // Entwickler: Denis Mock
        public G1C_WorldLocation? GetTransitionAt(int x, int y)
        {
            switch (GetTile(x, y))
            {
                case 'H': return G1C_WorldLocation.Home;
                case 'C': return G1C_WorldLocation.Casino;
                case 'K': return G1C_WorldLocation.Hospital;
                case 'S': return G1C_WorldLocation.Market;
                default: return null;
            }
        }

        // Entwickler: Denis Mock
        public string GetTileSpriteId(int x, int y)
        {
            char tile = GetTile(x, y);
            switch (tile)
            {
                case '#': return "tile_city_forest";
                case 'r': return "tile_city_road";
                case 's': return "tile_city_sidewalk";
                case 'p': return "tile_city_path";
                case 'H': return "tile_city_home_entry";
                case 'C': return "tile_city_casino_entry";
                case 'K': return "tile_city_hospital_entry";
                case 'S': return "tile_city_market_entry";
                case 'w': return ((x + y) & 1) == 0 ? "tile_city_water" : "tile_city_water_alt";
                case 'u': return "tile_city_bank";
                case 'b': return "tile_city_bridge";
                case 'v': return ((x + y) & 1) == 0 ? "tile_city_park" : "tile_city_grass";
                case 'X': return "tile_city_grass";
                default: return ((x * 3 + y * 5) & 3) == 0 ? "tile_city_grass_alt" : "tile_city_grass";
            }
        }

        // Entwickler: Denis Mock
        public bool IsWater(int x, int y)
        {
            return GetTile(x, y) == 'w';
        }

        // Entwickler: Denis Mock
        public bool IsOpenGround(int x, int y)
        {
            char tile = GetTile(x, y);
            return tile == 'g' || tile == 'v';
        }

        // Entwickler: Denis Mock
        public bool IsDoorApproach(int x, int y)
        {
            return NearDoor(x, y, HomeDoorX, HomeDoorY) || NearDoor(x, y, CasinoDoorX, CasinoDoorY) ||
                   NearDoor(x, y, HospitalDoorX, HospitalDoorY) || NearDoor(x, y, MarketDoorX, MarketDoorY);
        }

        // Entwickler: Denis Mock
        public int RiverCenterXAt(int y)
        {
            return RiverCenter(y);
        }

        // Entwickler: Denis Mock
        public void AddBuildingFootprint(int x, int y, int width, int height, G1C_WorldLocation destination)
        {
            AddSolidFootprint(x, y, width, height);

            int doorX = x + width / 2;
            int doorY = y + height;
            char door;
            switch (destination)
            {
                case G1C_WorldLocation.Home:
                    door = 'H';
                    HomeDoorX = doorX;
                    HomeDoorY = doorY;
                    break;
                case G1C_WorldLocation.Casino:
                    door = 'C';
                    CasinoDoorX = doorX;
                    CasinoDoorY = doorY;
                    break;
                case G1C_WorldLocation.Hospital:
                    door = 'K';
                    HospitalDoorX = doorX;
                    HospitalDoorY = doorY;
                    break;
                case G1C_WorldLocation.Market:
                    door = 'S';
                    MarketDoorX = doorX;
                    MarketDoorY = doorY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(destination));
            }

            _tiles[doorX, doorY] = door;
            ConnectEntrance(doorX, doorY + 1);
        }

        // Entwickler: Denis Mock
        public void AddSolidFootprint(int x, int y, int width, int height)
        {
            if (x < 1 || y < 1 || width < 1 || height < 1 ||
                x + width >= MapSize - 1 || y + height >= MapSize - 1)
                throw new ArgumentOutOfRangeException(nameof(x), "Gebäude müssen vollständig innerhalb der Stadt liegen.");

            for (int tileY = y; tileY < y + height; tileY++)
                for (int tileX = x; tileX < x + width; tileX++)
                    _tiles[tileX, tileY] = 'X';
        }

        // Entwickler: Denis Mock
        private static char[,] CreateLayout()
        {
            char[,] tiles = new char[MapSize, MapSize];
            for (int y = 0; y < MapSize; y++)
            {
                for (int x = 0; x < MapSize; x++)
                    tiles[x, y] = x == 0 || y == 0 || x == MapSize - 1 || y == MapSize - 1 ? '#' : 'g';
            }

            DrawRoadNetwork(tiles);
            DrawRiver(tiles);
            DrawBridges(tiles);
            DrawParks(tiles);
            return tiles;
        }

        // Entwickler: Denis Mock
        private static void DrawRoadNetwork(char[,] tiles)
        {
            foreach (int line in StreetLines)
            {
                for (int i = 1; i < MapSize - 1; i++)
                {
                    SetIfLand(tiles, i, line - 1, 's');
                    SetIfLand(tiles, i, line, 'r');
                    SetIfLand(tiles, i, line + 1, 'r');
                    SetIfLand(tiles, i, line + 2, 's');

                    SetIfLand(tiles, line - 1, i, 's');
                    SetIfLand(tiles, line, i, 'r');
                    SetIfLand(tiles, line + 1, i, 'r');
                    SetIfLand(tiles, line + 2, i, 's');
                }
            }
        }

        // Entwickler: Denis Mock
        private static void DrawRiver(char[,] tiles)
        {
            for (int y = 1; y < MapSize - 1; y++)
            {
                int center = RiverCenter(y);
                for (int x = center - 4; x <= center + 4; x++)
                {
                    if (x <= 0 || x >= MapSize - 1) continue;
                    tiles[x, y] = Math.Abs(x - center) <= 2 ? 'w' : 'u';
                }
            }
        }

        // Entwickler: Denis Mock
        private static void DrawBridges(char[,] tiles)
        {
            foreach (int line in StreetLines)
            {
                int center = RiverCenter(line);
                for (int y = line; y <= line + 1; y++)
                    for (int x = center - 6; x <= center + 6; x++)
                        if (x > 0 && x < MapSize - 1 && y > 0 && y < MapSize - 1)
                            tiles[x, y] = 'b';
            }

            int footbridgeY = 76;
            int footbridgeCenter = RiverCenter(footbridgeY);
            for (int x = footbridgeCenter - 6; x <= footbridgeCenter + 6; x++)
                if (x > 0 && x < MapSize - 1) tiles[x, footbridgeY] = 'b';
        }

        // Entwickler: Denis Mock
        private static void DrawParks(char[,] tiles)
        {
            FillPark(tiles, 5, 68, 11, 19);
            FillPark(tiles, 115, 91, 13, 17);
            FillPark(tiles, 51, 142, 10, 12);
        }

        // Entwickler: Denis Mock
        private static void FillPark(char[,] tiles, int x, int y, int width, int height)
        {
            for (int tileY = y; tileY < y + height; tileY++)
            {
                for (int tileX = x; tileX < x + width; tileX++)
                {
                    if (tiles[tileX, tileY] == 'g') tiles[tileX, tileY] = 'v';
                }
            }

            for (int tileX = x + 1; tileX < x + width - 1; tileX++)
                if (tiles[tileX, y + height / 2] == 'v') tiles[tileX, y + height / 2] = 'p';
            for (int tileY = y + 1; tileY < y + height - 1; tileY++)
                if (tiles[x + width / 2, tileY] == 'v') tiles[x + width / 2, tileY] = 'p';
        }

        // Entwickler: Denis Mock
        private void ConnectEntrance(int x, int y)
        {
            int targetY = y;
            int distance = int.MaxValue;
            foreach (int line in StreetLines)
            {
                int roadY = line + 1;
                if (Math.Abs(roadY - y) < distance)
                {
                    targetY = roadY;
                    distance = Math.Abs(roadY - y);
                }
            }

            int step = targetY < y ? -1 : 1;
            for (int tileY = y; tileY != targetY; tileY += step)
            {
                if (_tiles[x, tileY] == 'g' || _tiles[x, tileY] == 'v') _tiles[x, tileY] = 'p';
            }
        }

        // Entwickler: Denis Mock
        private static void SetIfLand(char[,] tiles, int x, int y, char value)
        {
            if (x > 0 && y > 0 && x < MapSize - 1 && y < MapSize - 1 &&
                tiles[x, y] != 'w' && tiles[x, y] != 'u')
                tiles[x, y] = value;
        }

        // Entwickler: Denis Mock
        private static int RiverCenter(int y)
        {
            return 90 + (int)(11 * System.Math.Sin(y * 0.065) + 4 * System.Math.Sin(y * 0.17));
        }

        // Entwickler: Denis Mock
        private static bool NearDoor(int x, int y, int doorX, int doorY)
        {
            return doorX > 0 && System.Math.Abs(x - doorX) <= 1 && System.Math.Abs(y - doorY) <= 2;
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            return _tiles[x, y];
        }
    }
}
