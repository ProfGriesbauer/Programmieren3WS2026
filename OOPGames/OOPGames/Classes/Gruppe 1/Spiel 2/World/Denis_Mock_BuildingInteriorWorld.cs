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
        public G1C_WorldLocation Location { get; }

        // Entwickler: Denis Mock
        public G1C_BuildingInteriorWorld(G1C_WorldLocation location)
        {
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Tankstelle und Bekleidungsgeschäft
            Location = location;
            Map = new G1C_BuildingInteriorMap(location);
            switch (location)
            {
                case G1C_WorldLocation.Hospital: BuildHospital(); break;
                case G1C_WorldLocation.Market: BuildMarket(); break;
                case G1C_WorldLocation.GasStation: BuildGasStation(); break;
                case G1C_WorldLocation.ClothingStore: BuildClothingStore(); break;
                default: throw new System.ArgumentOutOfRangeException(nameof(location));
            }
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
            // Änderung durch: Patrick Lupierz – die Kasse ist jetzt ein echter Verkaufstresen
            AddFurniture("Kasse", "obj_counter", 10, 6, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Kassiererin Jana", "guest", 11, 5);
            AddFurniture("K\u00fchlschrank", "obj_clinic_cabinet", 13, 6, 1, 2);
            AddFurniture("Pflanze", "obj_plant", 2, 8, 1, 1);
        }

        // Entwickler: Patrick Lupierz
        private void BuildGasStation()
        {
            AddFurniture("Regal", "obj_store_shelf", 2, 2, 2, 2);
            AddFurniture("Regal", "obj_store_shelf", 5, 2, 2, 2);
            AddFurniture("K\u00fchlregal", "obj_clinic_cabinet", 13, 2, 1, 2);
            AddFurniture("Regal", "obj_store_shelf", 2, 6, 2, 2);
            AddFurniture("Verkaufstheke", "obj_counter", 9, 4, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Tankwart Olli", "dealer_green", 10, 3);
            AddFurniture("M\u00fclleimer", "obj_trash", 13, 9, 1, 1, G1C_InteractionType.Trash);
        }

        // Entwickler: Patrick Lupierz
        private void BuildClothingStore()
        {
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 2, 2, 2, 1);
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 5, 2, 2, 1);
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 2, 5, 2, 1);
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 5, 5, 2, 1);
            AddFurniture("Spiegel", "obj_mirror", 13, 1, 1, 1);
            AddFurniture("Verkaufstheke", "obj_counter", 10, 4, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Verk\u00e4uferin Mia", "dealer_red", 11, 3);
            AddFurniture("Pflanze", "obj_plant", 2, 9, 1, 1);
        }

        // Entwickler: Denis Mock
        private void AddFurniture(string name, string sprite, int x, int y, int width, int height,
                                  G1C_InteractionType interaction = G1C_InteractionType.None)
        {
            _objects.Add(new G1C_WorldObject(name, sprite, x, y, width, height, true, interaction));
        }

        // Entwickler: Patrick Lupierz – Verkäufer steht hinter der Theke und öffnet ebenfalls den Laden
        private void AddSeller(string name, string skin, int x, int y)
        {
            _objects.Add(new G1C_Npc(name, skin, x, y, G1C_Direction.Down, G1C_InteractionType.Shop));
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
            // Änderung durch: Patrick Lupierz – Kachelsatz je Gebäude (hospital, market, gas, clothing)
            string set = _location == G1C_WorldLocation.Hospital ? "hospital"
                       : _location == G1C_WorldLocation.GasStation ? "gas"
                       : _location == G1C_WorldLocation.ClothingStore ? "clothing" : "market";
            if (tile == '#') return "tile_" + set + "_wall";
            if (tile == 'r') return "tile_interior_rug";
            return "tile_" + set + "_floor";
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
