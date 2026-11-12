using System.Collections.Generic;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Frameworkkonforme, eingerichtete Innenr\u00e4ume f\u00fcr Krankenhaus und Supermarkt.
    public sealed class G1C_BuildingInteriorWorld : IG1C_WorldScene
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();

        public G1C_BuildingInteriorMap Map { get; }
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }
        IG1C_WorldMap IG1C_WorldScene.Grid { get { return Map; } }
        public int PlayerStartX { get { return 7; } }
        public int PlayerStartY { get { return 10; } }
        public int TileSize { get { return 24; } }

        // Entwickler: Denis Mock
        public G1C_BuildingInteriorWorld(G1C_WorldLocation location)
        {
            if (location != G1C_WorldLocation.Hospital && location != G1C_WorldLocation.Market)
                throw new System.ArgumentOutOfRangeException(nameof(location));

            Map = new G1C_BuildingInteriorMap(location);
            if (location == G1C_WorldLocation.Hospital) BuildHospital();
            else BuildMarket();
        }

        // Entwickler: Denis Mock
        private void BuildHospital()
        {
            AddFurniture("Empfangstresen", "obj_counter", 2, 2, 3, 1);
            AddFurniture("Behandlungsbett", "obj_medical_bed", 3, 4, 2, 1);
            AddFurniture("Behandlungsbett", "obj_medical_bed", 9, 4, 2, 1);
            AddFurniture("Medizinschrank", "obj_clinic_cabinet", 3, 7, 2, 2);
            AddFurniture("Pflanze", "obj_plant", 13, 2, 1, 1);
            AddFurniture("Stuhl", "obj_chair", 7, 6, 1, 1);
        }

        // Entwickler: Denis Mock
        private void BuildMarket()
        {
            AddFurniture("Regal", "obj_store_shelf", 2, 2, 2, 2);
            AddFurniture("Regal", "obj_store_shelf", 5, 2, 2, 2);
            AddFurniture("Regal", "obj_store_shelf", 9, 2, 2, 2);
            AddFurniture("Kasse", "obj_counter", 10, 6, 3, 1);
            AddFurniture("K\u00fchlschrank", "obj_clinic_cabinet", 13, 6, 1, 2);
            AddFurniture("Pflanze", "obj_plant", 2, 8, 1, 1);
        }

        // Entwickler: Denis Mock
        private void AddFurniture(string name, string sprite, int x, int y, int width, int height)
        {
            _objects.Add(new G1C_WorldObject(name, sprite, x, y, width, height, true, G1C_InteractionType.None));
        }

        // Entwickler: Denis Mock
        public G1C_WorldObject ObjectAt(int x, int y)
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i].Occupies(x, y)) return _objects[i];
            return null;
        }
    }

    // Entwickler: Denis Mock
    // Eigene Kollisions- und Bodendaten der beiden eingerichteten Gesch\u00e4ftsr\u00e4ume.
    public sealed class G1C_BuildingInteriorMap : IG1C_WorldMap
    {
        private const int MapWidth = 16;
        private const int MapHeight = 12;
        private readonly G1C_WorldLocation _location;

        // Entwickler: Denis Mock
        public G1C_BuildingInteriorMap(G1C_WorldLocation location)
        {
            _location = location;
        }

        public int Width { get { return MapWidth; } }
        public int Height { get { return MapHeight; } }

        // Entwickler: Denis Mock
        public bool IsWalkable(int x, int y)
        {
            return GetTile(x, y) == '.' || GetTile(x, y) == 'r';
        }

        // Entwickler: Denis Mock
        public G1C_WorldLocation? GetTransitionAt(int x, int y)
        {
            return GetTile(x, y) == 'E' ? G1C_WorldLocation.Exterior : (G1C_WorldLocation?)null;
        }

        // Entwickler: Denis Mock
        public string GetTileSpriteId(int x, int y)
        {
            char tile = GetTile(x, y);
            if (tile == 'E') return "tile_interior_exit";
            if (tile == '#') return _location == G1C_WorldLocation.Hospital ? "tile_hospital_wall" : "tile_market_wall";
            if (tile == 'r') return "tile_interior_rug";
            return _location == G1C_WorldLocation.Hospital ? "tile_hospital_floor" : "tile_market_floor";
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            if (y == Height - 1) return x == 7 ? 'E' : '#';
            return x == 0 || x == Width - 1 || y == 0 ? '#' : (x >= 6 && x <= 8 && y >= 7 && y <= 8 ? 'r' : '.');
        }
    }
}
