using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Änderung durch: Patrick Lupierz – Datum: 12.11.2026
    //   Die Stadt ist kompakter (96x64 Kacheln, dafür doppelt so groß dargestellt), die Straßen
    //   verlaufen geschwungen statt im starren Raster, der Fluss liegt direkt im Gras, Türen liegen
    //   in der Gebäudefront und Tankstelle/Bekleidungsgeschäft sind als Ziele dazugekommen.
    public class G1C_ExteriorMap : IG1C_WorldMap
    {
        public const int MapWidth = 96;
        public const int MapHeight = 64;

        private const int RiverHalfWidth = 2;

        // Straßenverläufe als Stützpunkte (x, y, x, y, ...). Über dem Fluss verlaufen sie gerade (Brücken).
        private static readonly int[][] Roads =
        {
            new[] { 1, 15, 14, 13, 30, 15, 40, 14, 56, 14, 66, 13, 80, 15, 94, 14 },
            new[] { 1, 32, 16, 33, 30, 31, 40, 32, 56, 32, 64, 33, 78, 31, 94, 33 },
            new[] { 1, 50, 14, 51, 30, 49, 40, 50, 56, 50, 66, 51, 80, 49, 94, 50 },
            new[] { 18, 1, 17, 14, 19, 24, 17, 32, 19, 42, 18, 50, 17, 62 },
            new[] { 78, 1, 79, 14, 77, 24, 79, 32, 77, 42, 78, 50, 79, 62 },
        };

        private static readonly Dictionary<G1C_WorldLocation, char> DoorTiles = new Dictionary<G1C_WorldLocation, char>
        {
            { G1C_WorldLocation.Home, 'H' }, { G1C_WorldLocation.Casino, 'C' },
            { G1C_WorldLocation.Hospital, 'K' }, { G1C_WorldLocation.Market, 'S' },
            { G1C_WorldLocation.GasStation, 'T' }, { G1C_WorldLocation.ClothingStore, 'B' },
        };

        private readonly char[,] _tiles;
        private readonly Dictionary<G1C_WorldLocation, int[]> _doors = new Dictionary<G1C_WorldLocation, int[]>();

        public int Width { get { return MapWidth; } }
        public int Height { get { return MapHeight; } }

        public int HomeDoorX { get { return DoorX(G1C_WorldLocation.Home); } }
        public int HomeDoorY { get { return DoorY(G1C_WorldLocation.Home); } }
        public int CasinoDoorX { get { return DoorX(G1C_WorldLocation.Casino); } }
        public int CasinoDoorY { get { return DoorY(G1C_WorldLocation.Casino); } }
        public int HospitalDoorX { get { return DoorX(G1C_WorldLocation.Hospital); } }
        public int HospitalDoorY { get { return DoorY(G1C_WorldLocation.Hospital); } }
        public int MarketDoorX { get { return DoorX(G1C_WorldLocation.Market); } }
        public int MarketDoorY { get { return DoorY(G1C_WorldLocation.Market); } }

        // Entwickler: Denis Mock
        public G1C_ExteriorMap()
        {
            _tiles = CreateLayout();
        }

        // Entwickler: Patrick Lupierz – Türposition eines betretbaren Gebäudes (0, wenn nicht gebaut)
        public int DoorX(G1C_WorldLocation building)
        {
            int[] door;
            return _doors.TryGetValue(building, out door) ? door[0] : 0;
        }

        public int DoorY(G1C_WorldLocation building)
        {
            int[] door;
            return _doors.TryGetValue(building, out door) ? door[1] : 0;
        }

        // Entwickler: Denis Mock
        public bool IsWalkable(int x, int y)
        {
            char tile = GetTile(x, y);
            return tile == 'g' || tile == 'v' || IsPaved(tile);
        }

        // Entwickler: Denis Mock
        public G1C_WorldLocation? GetTransitionAt(int x, int y)
        {
            char tile = GetTile(x, y);
            foreach (KeyValuePair<G1C_WorldLocation, char> door in DoorTiles)
                if (door.Value == tile) return door.Key;
            return null;
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Wege, Straßen und Ufer wählen ihre Randgrafik nach den Nachbarn.
        public string GetTileSpriteId(int x, int y)
        {
            char tile = GetTile(x, y);
            switch (tile)
            {
                case '#': return "tile_city_forest";
                case 'r': return "tile_city_road_" + Mask(x, y, false);
                case 'p': return "tile_city_path_" + Mask(x, y, false);
                case 'b': return "tile_city_bridge";
                case 'w': return "tile_city_water_" + Mask(x, y, true);
                case 'v': return ((x * 7 + y * 3) % 5) == 0 ? "tile_city_flowers" : "tile_city_park";
                case 'g':
                    int hash = (x * 37 + y * 61) % 23;
                    return hash == 0 ? "tile_city_flowers" : (hash < 6 ? "tile_city_grass_alt" : "tile_city_grass");
                default: return "tile_city_grass";
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
            foreach (int[] door in _doors.Values)
                if (Math.Abs(x - door[0]) <= 1 && y > door[1] && y - door[1] <= 4) return true;
            return false;
        }

        // Entwickler: Denis Mock
        public int RiverCenterXAt(int y)
        {
            return RiverCenter(y);
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Oberste Zeile für ein Gebäude, das mit einer Kachel Abstand über der Straße steht.
        public int FitAboveRoad(int x, int width, int height, int roadY)
        {
            int top = roadY + 6;
            for (int column = x - 1; column <= x + width; column++)
                for (int row = roadY - 6; row <= roadY + 6; row++)
                    if (GetTile(column, row) == 'r') { top = Math.Min(top, row); break; }
            return top - height - 1;
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Die Tür liegt in der Gebäudefront; der Spieler steht davor.
        public void AddBuildingFootprint(int x, int y, int width, int height, G1C_WorldLocation destination)
        {
            char door;
            if (!DoorTiles.TryGetValue(destination, out door)) throw new ArgumentOutOfRangeException(nameof(destination));
            AddSolidFootprint(x, y, width, height);

            int doorX = x + width / 2;
            int doorY = y + height - 1;
            _tiles[doorX, doorY] = door;
            _doors[destination] = new[] { doorX, doorY };
            ConnectEntrance(doorX, doorY + 1);
        }

        // Entwickler: Denis Mock
        public void AddSolidFootprint(int x, int y, int width, int height)
        {
            if (x < 1 || y < 1 || width < 1 || height < 1 ||
                x + width >= MapWidth - 1 || y + height >= MapHeight - 1)
                throw new ArgumentOutOfRangeException(nameof(x), "Gebäude müssen vollständig innerhalb der Stadt liegen.");

            for (int tileY = y; tileY < y + height; tileY++)
                for (int tileX = x; tileX < x + width; tileX++)
                    _tiles[tileX, tileY] = 'X';
        }

        // Entwickler: Patrick Lupierz – Parkfläche (Blumenwiese) auf freiem Gras
        public void AddPark(int x, int y, int width, int height)
        {
            for (int tileY = y; tileY < y + height; tileY++)
                for (int tileX = x; tileX < x + width; tileX++)
                    if (GetTile(tileX, tileY) == 'g') _tiles[tileX, tileY] = 'v';
        }

        // Entwickler: Denis Mock
        private static char[,] CreateLayout()
        {
            char[,] tiles = new char[MapWidth, MapHeight];
            for (int y = 0; y < MapHeight; y++)
                for (int x = 0; x < MapWidth; x++)
                    tiles[x, y] = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1 ? '#' : 'g';

            DrawRiver(tiles);
            DrawRoadNetwork(tiles);
            return tiles;
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Straßen folgen Stützpunkten; über Wasser entsteht eine Brücke.
        private static void DrawRoadNetwork(char[,] tiles)
        {
            foreach (int[] road in Roads)
            {
                for (int i = 0; i + 3 < road.Length; i += 2)
                {
                    int steps = Math.Max(Math.Abs(road[i + 2] - road[i]), Math.Abs(road[i + 3] - road[i + 1]));
                    for (int s = 0; s <= steps; s++)
                    {
                        int x = road[i] + (int)Math.Round((road[i + 2] - road[i]) * s / (double)steps);
                        int y = road[i + 1] + (int)Math.Round((road[i + 3] - road[i + 1]) * s / (double)steps);
                        for (int dy = 0; dy < 2; dy++)
                            for (int dx = 0; dx < 2; dx++)
                                Pave(tiles, x + dx, y + dy);
                    }
                }
            }
        }

        private static void Pave(char[,] tiles, int x, int y)
        {
            if (x <= 0 || y <= 0 || x >= MapWidth - 1 || y >= MapHeight - 1) return;
            tiles[x, y] = tiles[x, y] == 'w' || tiles[x, y] == 'b' ? 'b' : 'r';
        }

        // Entwickler: Denis Mock
        private static void DrawRiver(char[,] tiles)
        {
            for (int y = 1; y < MapHeight - 1; y++)
            {
                int center = RiverCenter(y);
                for (int x = center - RiverHalfWidth; x <= center + RiverHalfWidth; x++)
                    tiles[x, y] = 'w';
            }
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Der Fußweg läuft von der Tür nach unten bis zur nächsten Straße.
        private void ConnectEntrance(int x, int y)
        {
            for (int tileY = y; tileY < y + 8 && IsOpenGround(x, tileY); tileY++)
                _tiles[x, tileY] = 'p';
        }

        // Entwickler: Denis Mock
        private static int RiverCenter(int y)
        {
            return 48 + (int)Math.Round(3 * Math.Sin(y * 0.12) + 2 * Math.Sin(y * 0.29));
        }

        private static bool IsPaved(char tile)
        {
            return tile == 'r' || tile == 'p' || tile == 'b';
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Nachbarschaft als Bitmaske (N=1, O=2, S=4, W=8) für die Randgrafiken.
        private int Mask(int x, int y, bool water)
        {
            int mask = 0;
            if (Joins(x, y - 1, water)) mask |= 1;
            if (Joins(x + 1, y, water)) mask |= 2;
            if (Joins(x, y + 1, water)) mask |= 4;
            if (Joins(x - 1, y, water)) mask |= 8;
            return mask;
        }

        private bool Joins(int x, int y, bool water)
        {
            char tile = GetTile(x, y);
            if (water) return tile == 'w' || tile == 'b' || tile == '#';
            return IsPaved(tile) || tile == '#' || DoorTiles.ContainsValue(tile);
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            return _tiles[x, y];
        }
    }
}
